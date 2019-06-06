class Snake {
    constructor(name, color) {
        this.body = new CustomLinkedList();
        this.name = name;
        this.color = color;
    }

    setStartPoint(x, y) {
        this.currDirection = MoveDirection.Down;
        this.body.addFirst(new Cell(baseCell.size, this.color, baseCell.outlineColor, x, y));
    }
    
    // Always returns added head coordinates AND deleted tail coordinates.
    update(direction, isFood) {
        var newHead = this.body.getFirst().getCopy();
        newHead.update(direction);

        this.body.addFirst(newHead);

        if (isFood === true) {
            return { head: newHead, tail: null };
        } else {
            return { head: newHead, tail: this.body.deleteLast() };
        }
    }

    getBodyArray() {
        return this.body.getArray();
    }
}

class GameController {
    constructor(playerName, lobbyId, mainDispatcher) {
        this.snakes = new Array();
        this.snakeCount = 0;
        //this.socket;
        this.mainDispatcher = mainDispatcher;
        this.socketDispatcher = new Dispatcher();
        //this.socketDispatcher.on("onSocketOpen", this.onOpenedSocket.call(this, event));
        this.socketDispatcher.on("onSocketOpen", this.onOpenedSocket.bind(this));
        this.socketDispatcher.on("onSocketMessage", this.onMessageReceived.bind(this));
        this.socketDispatcher.on("onSocketClose", this.onMessageReceived.bind(this));
        this.socketDispatcher.on("onSocketError", this.onMessageReceived.bind(this));
        this.playerName = playerName;
        this.lobbyId = lobbyId;

        this.socketController = new WebSocketController(this.playerName, this.lobbyId, this.socketDispatcher);
        this.socketController.connect();
        //this.connect();
    }

    onOpenedSocket(e) {
        console.log("Socket opened");
        this.sendPlayerListRequest();
        var settings = { cellCount: this.cellContainer.gridSize, isWall: false, speed: "Normal" };
        this.sendSettingUpdate(settings);
        this.mainDispatcher.dispatch("onWebSocketOpened");
    }

    onMessageReceived(message) {
        console.log("GameController received message:", message);

        switch (message.type) {
            case "Players":
                // Show all current players
                var playerUpdate = message.body.players;
                console.log("New player list: ", playerUpdate);
                this.mainDispatcher.dispatch("onPlayerListReceived", playerUpdate);
                break;
            case "Update":
                // Update game state
                this.HandleUpdate(message.body.status);
                break;
            case "Settings":
                this.mainDispatcher.dispatch("onSettingsReceived", message.body.Settings);
                break;
            case "Start":
                // Raise game started event
                this.mainDispatcher.dispatch("onStartReceived");
                // Do count down

                // Initialize positions
                this.HandleStart(message.body.Start);
                break;
            case "End":
                // close web socket connection, throw error.
                this.cellContainer.drawGameOver();
                this.mainDispatcher.dispatch("onGameEndReceived");
                break;
            case "Exit":
                this.mainDispatcher.dispatch("onExitReceived", message.body);
                break;
        }
    }

    HandleStart(startMessage) {
        this.cellContainer.drawGrid();
        var food = startMessage.food;
        if (food !== null) {
            this.cellContainer.drawCell(food.x, food.y, "black");
        }

        var snakesArray = startMessage.ActiveSnakes;
        var i;
        for (i = 0; i < snakesArray.length; i++) {
            var head = snakesArray[i].head;
            var tail = snakesArray[i].tail;
            var player = snakesArray[i].player;
            var color = snakesArray[i].color;

            //this.cellContainer.updateSnake(color, head, tail);
            var snake = new Snake(player, color);
            snake.setStartPoint(head.x, head.y);
            this.snakes.push(snake);
        }
        this.drawSnakes();
    }

    HandleUpdate(updateMessage) {
        var food = updateMessage.food;
        if (food !== null) {
            this.cellContainer.drawCell(food.x, food.y, "black");
        }
        // Update active snakes
        var snakesArray = updateMessage.ActiveSnakes;
        var i;
        for (i = 0; i < snakesArray.length; i++) {
            var head = snakesArray[i].head;
            var tail = snakesArray[i].tail;
            var player = snakesArray[i].player;
            var color = snakesArray[i].color;

            this.cellContainer.updateSnake(color, head, tail);
        }
        // Disable inactive snakes
        var snakesArray = updateMessage.DisabledSnakes;
        for (i = 0; i < snakesArray.length; i++) {

        }
    }

    setEnvironment() {
        onResize();
    }

    setCellContainer(container) {
        this.cellContainer = container;
        this.cellContainer.createGrid(false);
        this.cellContainer.drawGrid();
    }

    drawElements() {
        this.cellContainer.createGrid(false);
        this.cellContainer.drawGrid();
        this.drawSnakes();
    }

    raiseOnOpen() {

    }

    createSnake() {

    }

    moveSnake(direction) {

    }

    sendPlayerListRequest() {
        var messageBody = "";
        this.socketController.send("Players", JSON.stringify(messageBody));
    }

    sendMovementUpdate(direction) {
        this.socketController.send("Update", JSON.stringify(direction));
    }

    sendGameStart() {
        this.socketController.send("Start", null);
    }

    sendSettingUpdate(settings) {
        //var settings = { cellCount : this.cellContainer.gridSize, isWall: false };

        this.socketController.send("Settings", settings);
    }

    drawSnakes() {
        for (var i = 0; i < this.snakes.length; i++) {
            this.cellContainer.initializeSnake(this.snakes[i].getBodyArray(), this.snakes[i].color);
        }
    }
}


function htmlEscape(str) {
    return str.toString()
        .replace(/&/g, '&amp;')
        .replace(/"/g, '&quot;')
        .replace(/'/g, '&#39;')
        .replace(/</g, '&lt;')
        .replace(/>/g, '&gt;');
}