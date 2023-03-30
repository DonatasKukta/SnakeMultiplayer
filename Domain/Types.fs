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

    let firstFrom = List.tryHead
    let lastFrom = List.tryLast
    let firstAndLast list = (firstFrom list, lastFrom list)
    let itself x = x

    let someFun name =
        Option.map (fun coords -> (name, coords))

    let setPendingDirection player direction =
        { player with pendingDirection = direction }

    let generateFood previousFood = { X = 0; Y = 0 }

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
            [ fst coords |> someFun name
              snd coords |> someFun name ])
        |> List.choose itself
        |> List.map updateBoard
        |> List.choose itself
        |> removeEmptyPlayers
        |> fun arena -> { arena with food = generateFood arena.food }


    let setInitialPositions arena = arena

    let createArena settings =
        { settings = settings
          board = Array2D.zeroCreate settings.cellCount settings.cellCount
          players = Map.empty
          food = generateFood <| Some { X = 0; Y = 0 } }

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
