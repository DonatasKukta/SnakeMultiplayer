namespace Domain

open System

module FromServerMappings =
    open System.Collections.Generic
    open JsonLibrary

    let private playerType playerName hostName =
        playerName = hostName
        |> function
            | true -> "Host"
            | _ -> "Player"

    let settingsDtoToSettings (settingsDto: JsonLibrary.FromClient.Settings) =
        { cellCount = settingsDto.cellCount
          speed = Enum.Parse(typedefof<Speed>, settingsDto.speed) :?> Speed }

    let snakeToPlayerDto hostName snake =
        let n = new JsonLibrary.FromServer.Player()
        n.name <- snake.name
        n.color <- string snake.color
        n.``type`` <- playerType snake.name hostName

        n

    let arenaToPlayerStatus arena =
        arena.players.Values
        |> Seq.map (snakeToPlayerDto arena.hostPlayer)


    let coordinateToXyDto (coordinate: Coordinate) =
        new JsonLibrary.FromServer.XY(coordinate.X, coordinate.Y)

    let snakeToSnakeDto (snake: Snake) =
        new JsonLibrary.FromServer.Snake(
            snake.name,
            string snake.color,
            snake.body |> Seq.head |> coordinateToXyDto,
            snake.body |> Seq.last |> coordinateToXyDto
        )

    let arenaToArenaStatusDto arena =
        let n = new JsonLibrary.FromServer.ArenaStatus(coordinateToXyDto arena.food)

        n.DisabledSnakes <-
            arena.players.Values
            |> Seq.filter (fun s -> Seq.isEmpty s.body)
            |> Seq.map (fun s -> s.name)
            |> List<string>

        n.ActiveSnakes <-
            arena.players.Values
            |> Seq.filter (fun s -> not (Seq.isEmpty s.body))
            |> Seq.map snakeToSnakeDto
            |> List<JsonLibrary.FromServer.Snake>

        n
