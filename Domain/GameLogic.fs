namespace Domain

module Functions =
    //TODO: Move System.Random from Domain.
    let random = System.Random()
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

    let setPendingDirection player direction =
        { player with pendingDirection = direction }

    let update coordinate =
        function
        | Direction.Up -> { coordinate with X = coordinate.X + 1 }
        | Direction.Down -> { coordinate with X = coordinate.X - 1 }
        | Direction.Left -> { coordinate with Y = coordinate.Y - 1 }
        | Direction.Right -> { coordinate with Y = coordinate.Y + 1 }
        | _ -> coordinate

    /// Returns snake with updated body if it's new head is inside the board; otherwise snake with empty body
    let move snake (board: Cell [,]) cellCount : Snake =
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
            else
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
            | _ ->
                clearSnakeCells ()
                Some { snake with body = List.empty }

        Option.map2 update head snake.pendingDirection
        |> Option.map clearSnakeIfHeadOutsideBoard
        |> Option.flatten
        |> Option.bind moveAndUpdateSnake
        |> Option.defaultValue { snake with body = List.empty }

    let generateFoodAnywhere max =
        { X = random.Next(0, max)
          Y = random.Next(0, max) }

    // TODO: This will cause endless loop if all board cells are not empty.
    let rec generateRandomFood (board: Cell [,]) max =
        match (random.Next(0, max), random.Next(0, max)) with
        | (x, y) when board.[x, y] = Cell.Empty -> { X = x; Y = y }
        | _ -> generateRandomFood board max

    let applyPendingDirections (arena: Arena) =

        let updatedPlayers =
            Map.map (fun name snake -> move snake arena.board arena.settings.cellCount) arena.players

        let newFood =
            match arena.board.[arena.food.X, arena.food.Y] with
            | Cell.Food -> arena.food
            | Cell.Snake -> generateRandomFood arena.board arena.settings.cellCount
            | _ -> failwith "Previous food coordinate points to empty Cell, which is invalid."

        { arena with
            food = newFood
            players = updatedPlayers }

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
