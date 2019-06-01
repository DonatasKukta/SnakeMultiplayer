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
        var messageBody = "";
        this.socketController.send("Players", JSON.stringify(messageBody));
    }

    onMessageReceived(message) {
        console.log("GameController received message:", message);

        switch (message.type) {
            case "Players":
                // Show all current players
                var playerUpdate = message.body.players;
                console.warn("New player list: ", playerUpdate);
                this.mainDispatcher.dispatch("onPlayerListReceived", playerUpdate);
                break;
            case "Update":
                // Update game state
                var gameUpdate = message.body;
                console.warn("New update message: ", gameUpdate);
                break;
            case "Start":
                // Do count down
                break;
            case "Close": // ?
                // close web socket connection, throw error.
                break;
        }
    }

    beginStub() {
        this.webSocketController.send("nusiusta zinute");
        this.webSocketController.send("kita zinute");
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

    sendUpdate(direction) {
        var messageBody = { direction : direction };
        this.socketController.send("Update",JSON.stringify(messageBody));
    }

    drawSnakes() {
        for (var i = 0; i < this.snakes.length; i++) {
            this.cellContainer.initializeSnake(this.snakes[i].getBodyArray(), this.snakes[i].color);
        }
    }

    doStubActions() {
        var snake = new Snake("Donatas", "blue");
        snake.setStartPoint(2, 2);
        this.cellContainer.initializeSnake(snake.getBodyArray(), snake.color);

        var newCoords = snake.update(MoveDirection.Right, true);
        this.cellContainer.updateSnake(snake.innerColor, newCoords.head, newCoords.tail);
        newCoords = snake.update(MoveDirection.Right, true);
        this.cellContainer.updateSnake(snake.innerColor, newCoords.head, newCoords.tail);
        newCoords = snake.update(MoveDirection.Right, true);
        this.cellContainer.updateSnake(snake.innerColor, newCoords.head, newCoords.tail);
        this.snakes.push(snake);
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