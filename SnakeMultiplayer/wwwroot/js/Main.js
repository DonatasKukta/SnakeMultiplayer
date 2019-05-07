(function () {
    class Snake {
        constructor(name, color) {
            this.body = new CustomLinkedList();
            this.name = name;
            this.color = color;
        }

        setStartPoint(x, y) {
            this.currDirection = MoveDirection.Down;
            this.body.addFirst(new Cell(baseCell.size, this.color, baseCell.outlineColor,x,y));
        }

        // Always returns added head coordinates AND deleted tail coordinates.
        update(direction, isFood) {
            var newHead = this.body.getFirst().getCopy();
            newHead.update(direction);

            this.body.addFirst(newHead);

            if (isFood === true) {
                return { head: newHead, tail : null };
            } else {
                return { head: newHead, tail:  this.body.deleteLast()};
            }
        }

        getBodyArray() {
            return this.body.getArray();
        }
    }

    class GameController {
        constructor() {
            this.snakes = new Array();
            this.snakeCount = 0;
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

        createSnake() {

        }
        
        moveSnake(direction) {
            
        }

        sendUpdate(direction) {
            
        }

        drawSnakes() {
            for (var i = 0; i < this.snakes.length; i++) {
                this.cellContainer.initializeSnake(this.snakes[i].getBodyArray(), this.snakes[i].color);
            }
        }

        receiveUpdate() {
            // According to received information from web socket, update snakes and cell container.
            /*
             * for each snake moves nake
             * */
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

    window.addEventListener('resize', reSet, false);
    var gameController = new GameController();
    gameController.setEnvironment();
    gameController.setCellContainer(new CellGridContainer(cellCount, baseCell, CanvasContext, TLborder, BRborder));    gameController.doStubActions();

    var socketController = new WebSocketController();
    socketController.connect();

    const sleep = (milliseconds) => {
        return new Promise(resolve => setTimeout(resolve, milliseconds))
    }

    sleep(500).then(() => {
        socketController.send("nusiusta zinute");
        socketController.send("kita zinute");
    });



    document.onkeydown = function (e) {
        switch (e.key) {
            case 'ArrowUp':
                gameController.sendUpdate(MoveDirection.Up);
                break;
            case 'ArrowDown':
                gameController.sendUpdate(MoveDirection.Down);
                break;
            case 'ArrowLeft':
                gameController.sendUpdate(MoveDirection.Left);
                break;
            case 'ArrowRight':
                gameController.sendUpdate(MoveDirection.Right);
                break;
        }
    };

    //-----------------------
    // Public methods:
    //-----------------------

    function reSet() {
        onResize();
        gameController.setCellContainer(new CellGridContainer(cellCount, baseCell, CanvasContext, TLborder, BRborder));
        gameController.drawSnakes();       
    }

    /*
    function onResize() {
        console.log(">>Resizing")
        ClearCanvas();
        ResizeCanvas();
        DrawBaseCanvas();

    } */

    function onSnakeMovement() {

    }

    function setCells() {
        //CellsContainer = new CellGridContainer(cellCount, baseCell, CanvasContext, TLborder, BRborder);
        //CellsContainer.createGrid(false);
        //CellsContainer.drawGrid();
        //gameController.setCellContainer(new CellGridContainer(cellCount, baseCell, CanvasContext, TLborder, BRborder));
    }
})();