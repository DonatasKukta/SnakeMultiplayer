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

    let setPendingDirection player direction =
        { player with pendingDirection = direction }

    let applyPendingDirections arena = arena

    let setInitialPositions arena = arena

    let generateFood previousFood = { X = 0; Y = 0 }

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

    let update coordinate =
        function
        | Direction.Up -> { coordinate with X = coordinate.X + 1 }
        | Direction.Down -> { coordinate with X = coordinate.X - 1 }
        | Direction.Left -> { coordinate with Y = coordinate.Y - 1 }
        | Direction.Right -> { coordinate with Y = coordinate.Y + 1 }
        | _ -> coordinate

    let move snake food =
        let body = snake.body
        let head = List.tryHead snake.body

        let getNewBody =
            function
            | true -> body
            | false -> List.take (body.Length - 1) body

        Option.map2 update head snake.pendingDirection
        |> Option.map (fun newHead -> (newHead = food, newHead))
        |> Option.map (fun (isEaten, newHead) -> { snake with body = newHead :: getNewBody isEaten })
