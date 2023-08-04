namespace Domain

open System

module Functions =
    open System.Collections.Concurrent
    //TODO: Move System.Random from Domain.
    let random = Random()
    let firstFrom = List.tryHead
    let lastFrom = List.tryLast
    let firstAndLast list = (firstFrom list, lastFrom list)
    let itself x = x
    let OptionOfBool x b = if b then Some x else None
    let createOptionUnit x = Option.map (fun y -> (x, y))

    let isInsideBoard coord cellCount =
        coord.X >= 0
        && coord.X < cellCount
        && coord.Y >= 0
        && coord.Y < cellCount

    let update coordinate =
        function
        | Direction.Up -> { coordinate with X = coordinate.X + 1 }
        | Direction.Down -> { coordinate with X = coordinate.X - 1 }
        | Direction.Left -> { coordinate with Y = coordinate.Y - 1 }
        | Direction.Right -> { coordinate with Y = coordinate.Y + 1 }
        | _ -> coordinate

    let emptySnake snake =
        { snake with
            body = List.empty
            previousDirection = None }

    /// Returns snake with updated body if it's new head is inside the board; otherwise snake with empty body
    let move (board: Cell [,]) cellCount snake pendingDirection  : Snake =
        let body = snake.body
        let head = List.tryHead snake.body
        let last = List.tryLast snake.body

        let getNewBody =
            function
            | true -> body
            | false -> List.take (body.Length - 1) body

        let clearSnakeCells () =
            Seq.iter (fun coord -> board[coord.X, coord.Y] <- Cell.Empty) snake.body

        let setCell coord cell = board[coord.X, coord.Y] <- cell

        let clearSnakeIfHeadOutsideBoard newHead =
            if isInsideBoard newHead cellCount then
                Some newHead
            else // TODO: Is this needed?
                clearSnakeCells ()
                None

        let moveAndUpdateSnake newHead =
            match board.[newHead.X, newHead.Y] with
            | Cell.Food ->
                setCell newHead Cell.Snake
                Some { snake with body = newHead :: getNewBody true }
            | Cell.Empty ->
                setCell newHead Cell.Snake

                if last <> None then
                    setCell last.Value Cell.Empty

                Some { snake with body = newHead :: getNewBody false }
            | _ -> None

        let deactivateSnake = 
            clearSnakeCells()
            emptySnake snake

        Option.map2 update head pendingDirection
        |> Option.map clearSnakeIfHeadOutsideBoard
        |> Option.flatten
        |> Option.bind moveAndUpdateSnake
        |> Option.defaultValue deactivateSnake

    let generateFoodAnywhere max =
        { X = random.Next(0, max)
          Y = random.Next(0, max) }

    // TODO: This will cause endless loop if all board cells are not empty.
    let rec generateRandomFood (board: Cell [,]) max =
        match (random.Next(0, max), random.Next(0, max)) with
        | (x, y) when board.[x, y] = Cell.Empty -> { X = x; Y = y }
        | _ -> generateRandomFood board max

    let applyPendingDirections pendingDirections arena  =
        let moveSnake = move arena.board arena.settings.cellCount
        let getPendingDirection name = Map.tryFind name pendingDirections

        let updatedPlayers =
            Map.map (fun name snake -> moveSnake snake (getPendingDirection name)) arena.players

        match arena.board.[arena.food.X, arena.food.Y] with
            | Cell.Food -> Ok arena.food
            | Cell.Snake -> Ok <| generateRandomFood arena.board arena.settings.cellCount
            | _ -> Error "Previous food coordinate points to empty Cell, which is invalid."
        |> Result.map (fun newFood ->  { arena with food = newFood; players = updatedPlayers })

    let emptyBoard cellCount = Array2D.zeroCreate cellCount cellCount

    let clearArena arena =
        { arena with
            board = emptyBoard arena.settings.cellCount
            food = generateFoodAnywhere arena.settings.cellCount
            players =
                arena.players
                |> Map.map (fun name snake -> { snake with body = List.empty }) }

    let createArena host settings =
        { settings = settings
          board = emptyBoard settings.cellCount
          players = Map.empty
          food = generateFoodAnywhere settings.cellCount
          hostPlayer = host
          maxPlayers = 4 }

    let createSnake name coordinate color =
        { name = name
          body = [ coordinate ]
          color = color
          previousDirection = None }

    let removeSnake name arena =
        Ok {arena with players = Map.remove name arena.players}

    let getInitialPositionAndColor arena = 
        let min = 1
        let max = arena.settings.cellCount - 2

        match arena.players.Keys.Count with
        | 0 -> Ok (GreenYellow,  { X = min; Y = min })
        | 1 -> Ok (DodgerBlue,   { X = min; Y = max })
        | 2 -> Ok (Orange,       { X = max; Y = min })
        | 3 -> Ok (MediumPurple, { X = max; Y = max })
        | _ -> Error "Only 4 players can play in same lobby."

    let canPlayerJoin (name: string) arena : Result<unit, string> =
        if arena.players.Count >= arena.maxPlayers then
            Error "Lobby is full"
        elif String.IsNullOrWhiteSpace name then
            Error "Player Name cannot be empty"
        elif Seq.forall Char.IsLetter (name.Trim().ToCharArray()) then
            Error "Player Name can only contain letters"
        elif arena.players.Keys.Contains(name) then
            Error $"Player {name} already exists in the lobby."
        else 
            Ok ()

    let addPlayer name arena =
        canPlayerJoin name arena
        |> Result.bind (fun _ -> getInitialPositionAndColor arena)
        |> Result.map  (fun (color, coord) -> createSnake name coord color)
        |> Result.map  (fun newSnake -> Map.add name newSnake arena.players)
        |> Result.map  (fun updatedPlayers -> {arena with players = updatedPlayers })

    let isGameEnd (arena:Arena) : bool = 
        let hostPlayerExists = Seq.exists (fun p -> p.name = arena.hostPlayer) arena.players.Values 
        if not hostPlayerExists then true
        else

        let isSnakeWithBody snake = not (Seq.isEmpty snake.body)
        let isSinglePlayer = Seq.length arena.players = 1
        let isMultiplayer = not isSinglePlayer
        arena.players.Values 
        |> Seq.filter isSnakeWithBody 
        |> Seq.length
        |> function
            | 0 -> true
            | 1 when isMultiplayer -> true
            | 1 when isSinglePlayer -> false
            | _ -> false
            
    type GameServer() =
        let arenas = ConcurrentDictionary<ArenaId, Arena>()
        let pendingDirections = ConcurrentDictionary<ArenaId * PlayerId, Direction>()
        
        member this.getPendingDirection arenaId playerId =
            match pendingDirections.TryGetValue((arenaId, playerId)) with
                | true, direction -> Some direction
                | _ -> None

        member this.getPendingDirections arenaId =
            let getPlayerWithDirection playerId = 
                this.getPendingDirection arenaId playerId 
                |> Option.map (fun direction -> (playerId , direction))
                
            this.TryGetArena arenaId
            |> Option.map (fun arena -> arena.players.Keys)
            |> Option.map (Seq.map getPlayerWithDirection)
            |> Option.map (Seq.choose id)
            |> Option.map Map.ofSeq
            |> Option.defaultValue Map.empty

        member this.removePendingDirections arenaId playerIds =
            playerIds
            |> Seq.iter (fun playerId -> pendingDirections.TryRemove((arenaId, playerId)) |> ignore)

        member this.CreateArena arenaId host =
            let addArena newArena = 
                match arenas.TryAdd(arenaId, newArena) with
                | false -> Error $"Arena with name '{arenaId}' already exists."
                | true -> Ok "Arena created succesfully"
            //TODO: Default arena settings shouldn't be hardcoded. 
            createArena host {cellCount = 20; speed = Speed.Normal }
            |> addPlayer host
            |> Result.bind addArena
            
        //TODO: Try to refactor and  avoid failWith and errorMessage.
        member this.AddPlayer arenaId playerId = 
            this.Update arenaId (addPlayer playerId)

        member private this.TryGetArena arenaId =
            match arenas.TryGetValue(arenaId) with
            | true, value -> Some(value)
            | _ -> None

        member private this.Update arenaId  updateArenaFunc = 
            let mutable result : Result<unit, string> = Ok()
            let saveError errorStr = 
                result <- Error errorStr
                errorStr
            
            let addValueFactory = (fun _ -> failwith $"Arena '{arenaId}' does not yet exist.")
            let updateValueFactory = 
                Func<string, Arena, Arena>(
                    fun _ oldArena -> 
                    updateArenaFunc oldArena 
                    |> Result.mapError saveError
                    |> Result.defaultValue oldArena) 
                
            arenas.AddOrUpdate(arenaId, addValueFactory, updateValueFactory) |> ignore
            result

        member this.UpdateArena arenaId =
            this.getPendingDirections arenaId
            |> applyPendingDirections
            |> this.Update arenaId 
            // TODO: Generate status report.

        member this.RemoveArena arenaId =
            let mutable removedArena = Unchecked.defaultof<Arena>
            if not (arenas.TryRemove(arenaId, &removedArena)) then
                Error $"Arena '{arenaId}' does not exist."
            else
            removedArena.players.Keys
            |> this.removePendingDirections arenaId
            Ok()

        member this.RemovePlayer arenaId playerId =
            removeSnake playerId
            |> this.Update arenaId

        member this.setPendingAction arenaId playerId (direction: Direction) =
            pendingDirections[(arenaId, playerId)] <- direction

