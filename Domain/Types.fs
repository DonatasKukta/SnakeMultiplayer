namespace Domain

type Speed =
    | NoSpeed = 0
    | Fast = 1
    | Normal = 2
    | Slow = 3

type Direction =
    | Up = 1
    | Right = 2
    | Down = 3
    | Left = 4

type InitialPosition =
    | UpLeft
    | UpRight
    | DownLeft
    | DownRight

type Cell =
    | Empty = 0
    | Food = 1
    | Snake = 2

type Color =
    | GreenYellow
    | DodgerBlue
    | Orange
    | MediumPurple

type LobbyStates =
    | Idle
    | Initialized
    | InGame
    | Closed

type Settings = { cellCount: int; speed: int }

type Coordinate = { X: int; Y: int }
type Food = Coordinate

type Snake =
    { name: string
      body: list<Coordinate>
      color: Color
      pendingDirection: Direction option
      previousDirection: Direction option }

type Arena =
    { settings: Settings
      board: Cell [,]
      players: Map<string, Snake>
      food: Food }

module Functions =
    let random = System.Random()
    let firstFrom = List.tryHead
    let lastFrom = List.tryLast
    let firstAndLast list = (firstFrom list, lastFrom list)
    let itself x = x

    let createOptionUnit x = Option.map (fun y -> (x, y))

    let setPendingDirection player direction =
        { player with pendingDirection = direction }

    let move snake food =
        let body = snake.body
        let head = List.tryHead snake.body

        let update coordinate =
            function
            | Direction.Up -> { coordinate with X = coordinate.X + 1 }
            | Direction.Down -> { coordinate with X = coordinate.X - 1 }
            | Direction.Left -> { coordinate with Y = coordinate.Y - 1 }
            | Direction.Right -> { coordinate with Y = coordinate.Y + 1 }
            | _ -> coordinate

        let getNewBody =
            function
            | true -> body
            | false -> List.take (body.Length - 1) body

        Option.map2 update head snake.pendingDirection
        |> Option.map (fun newHead -> (newHead = food, newHead))
        |> Option.map (fun (withFood, newHead) -> { snake with body = newHead :: getNewBody withFood })
        |> Option.defaultValue snake

    let generateFoodAnywhere max =
        { X = random.Next(0, max)
          Y = random.Next(0, max) }

    // TODO: This will cause endless loop if all board cells are not empty.
    let rec generateRandomFood (board: Cell [,]) max =
        match (random.Next(0, max), random.Next(0, max)) with
        | (x, y) when board.[x, y] = Cell.Empty -> { X = x; Y = y }
        | _ -> generateRandomFood board max

    let applyPendingDirections (arena: Arena) =

        let newPlayers =
            arena.players
            |> Map.map (fun name snake -> move snake arena.food)

        let updateBoard (name, coord) =
            match arena.board[coord.X, coord.Y] with
            | Cell.Snake ->
                Map.tryFind name newPlayers
                |> fun snake ->
                    if Option.isSome snake then
                        Seq.iter (fun coord -> arena.board[ coord.X, coord.Y ] <- Cell.Empty) snake.Value.body
                        Some snake.Value.name
                    else
                        None
            | _ ->
                arena.board[ coord.X, coord.Y ] <- Cell.Snake
                None

        let removeEmptyPlayers emptyPlayers =
            emptyPlayers
            |> List.fold
                (fun acc name ->
                    match Map.tryFind name acc.players with
                    | Some snake -> { acc with players = Map.add name { snake with body = List.empty } acc.players }
                    | None -> acc)
                arena

        newPlayers
        |> List.ofSeq
        |> List.map (fun pair -> (pair.Key, firstAndLast pair.Value.body))
        |> List.collect (fun (name, coords) ->
            [ fst coords |> createOptionUnit name
              snd coords |> createOptionUnit name ])
        |> List.choose itself
        |> List.map updateBoard
        |> List.choose itself
        |> removeEmptyPlayers
        |> fun arena -> { arena with food = generateRandomFood arena.board arena.settings.cellCount }

    let emptyBoard cellCount = Array2D.zeroCreate cellCount cellCount

    let clearArena arena =
        { arena with
            board = emptyBoard arena.settings.cellCount
            food = generateFoodAnywhere arena.settings.cellCount
            players =
                arena.players
                |> Map.map (fun name snake -> { snake with body = List.empty }) }

    let createArena settings =
        { settings = settings
          board = emptyBoard settings.cellCount
          players = Map.empty
          food = generateFoodAnywhere settings.cellCount }

    let createSnake name coordinate color =
        { name = name
          body = [ coordinate ]
          color = color
          pendingDirection = None
          previousDirection = None }

    let deactivate snake =
        { snake with
            body = List.empty
            pendingDirection = None
            previousDirection = None }
