namespace Domain

module FromServerMappings =
    open System.Collections.Generic
    let private playerType playerName hostName = 
        playerName = hostName
        |> function |true -> "Host" | _ -> "Player"

    let snakeToPlayerDto player hostName = 
        let n = new JsonLibrary.FromServer.Player()
        n.name <- player.name
        n.color <- string player.color
        n.``type`` <- playerType player.name hostName

    let coordinateToXyDto (coordinate: Coordinate)=
        new JsonLibrary.FromServer.XY(coordinate.X, coordinate.Y)

    let snakeToSnakeDto (snake :Snake) = 
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