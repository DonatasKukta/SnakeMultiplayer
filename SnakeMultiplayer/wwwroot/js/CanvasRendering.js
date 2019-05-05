(function () {
    var MoveDirection = Object.freeze({
        "None": 0,
        "Up": 1,
        "Right": 2,
        "Down": 3,
        "Left": 4
    });


    class Coordinate {
        constructor(x, y) {
            this.x = x;
            this.y = y;
        }
    }

    class Cell {
        constructor(length = -1, innerColor, outlineColor, x = -1, y = -1) {
            this.size = length,
            this.innerColor = innerColor,
            this.outlineColor = outlineColor
            this.x = x;
            this.y = y;
        }
        update(direction) {
            switch (direction) {
                case MoveDirection.Up:
                    this.y -= 1;
                    break;
                case MoveDirection.Right:
                    this.x += 1;
                    break;
                case MoveDirection.Down:
                    this.y += 1;
                    break;
                case MoveDirection.Left:
                    this.x -= 1;
                    break;
                case MoveDirection.None:
                    break;
                default:
                    console.error("Unexpected direction value!", direction);
                    return;
            }
        }
    }

    class CellGridContainer {
        constructor(gridSize, baseCellParams, canvasCtx, startBorder, endBorder) {
            this.gridSize = gridSize;
            this.baseCellParams = baseCellParams;
            this.canvasCtx = canvasCtx;
            this.startBorder = startBorder;
            this.endBorder = endBorder;
        }

        createGrid(draw = true) {
            this.Cells = new Array(this.gridSize);
            var x, y;
            for (x = 0; x < this.gridSize; x++) {
                this.Cells[x] = new Array(this.gridSize);
                for (y = 0; y < this.gridSize; y++) {
                    this.Cells[x][y] = new Cell(this.baseCellParams.size, this.baseCellParams.innerColor, this.baseCellParams.outlineColor, x, y);
                    if (draw === true) {
                        this.drawCustomCell(this.Cells[x][y]);
                    }
                }
            }
        }

        drawGrid() {
            var x, y;
            for (x = 0; x < this.gridSize; x++) {
                for (y = 0; y < this.gridSize; y++) {
                    this.drawCustomCell(this.Cells[x][y]);
                }
            }
        }

        initializeSnake(snakeBody, color) {
            //iterate through snakeBody
            var i;
            for (i = 0; i < snakeBody.length; i++) {
                this.updateSnake(color, snakeBody[i]);
            }
        }

        updateSnake( snakeColor, head, tail = null) {
            this.drawCell(head.x, head.y, snakeColor);
            if (tail !== null) {
                this.drawCell(tail.x, tail.y, this.baseCellParams.innerColor);
            }
        }
        
        drawCell(x, y, fillColor) {
            var coordx = this.getCellCoord(x);
            var coordy = this.getCellCoord(y);
            DrawFillRenctangle(coordx, coordy, this.baseCellParams.size, fillColor);
            DrawOutlineRectangle(coordx, coordy, this.baseCellParams.size, this.baseCellParams.outlineColor);
        }

        drawBaseCell(x,y) {
            var coordx = this.getCellCoord(x);
            var coordy = this.getCellCoord(y);
            DrawFillRenctangle(coordx, coordy, this.baseCellParams.size, this.baseCellParams.innerColor);
            DrawOutlineRectangle(coordx, coordy, this.baseCellParams.size, this.baseCellParams.outlineColor);
        }

        drawCustomCell(cell) {
            var xCoord = this.getCellCoord(cell.x);
            var yCoord = this.getCellCoord(cell.y);
            DrawFillRenctangle(xCoord, yCoord, cell.size, cell.innerColor);
            DrawOutlineRectangle(xCoord, yCoord, cell.size, cell.outlineColor);
        }

        getCellCoord(cellNumber) {
            return this.startBorder + (cellNumber * this.baseCellParams.size);
        }
    }

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
            var newHead = this.body.getFirst();
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

        createSnake() {

        }
        
        moveSnake(direction) {
            
        }

        sendUpdate(direction) {
            
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
            
       }

    }
    //---------------------------------
    //---------Entry point-------------
    //---------------------------------

    var Canvas = document.getElementById("Canvas");
    var CanvasContext = Canvas.getContext("2d");
    window.addEventListener('resize', onResize, false);
    // Constants that depend on current screen size 
    var length;
    var TLborder;
    var BRborder;
    var margin;
    var baseCell = new Cell(undefined, "red", "green");
    var cellCount = 10;
    var relMarginSize = 0.1;
    
    var gameController = new GameController();
    gameController.setEnvironment();
    gameController.doStubActions();

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
    function onResize() {
        console.log(">>Resizing")
        ClearCanvas();
        ResizeCanvas();
        DrawCanvas();
        setCells();
    }

    function onSnakeMovement() {

    }

    function setCells() {
        //CellsContainer = new CellGridContainer(cellCount, baseCell, CanvasContext, TLborder, BRborder);
        //CellsContainer.createGrid(false);
        //CellsContainer.drawGrid();
        gameController.setCellContainer(new CellGridContainer(cellCount, baseCell, CanvasContext, TLborder, BRborder));
    }

    function ResizeCanvas() {
        var canvasLength = -1;
        if (window.innerWidth > window.innerHeight) {
            canvasLength = window.innerHeight - 5;
        } else {
            canvasLength = window.innerWidth - 5;
        }
        canvasLength = getCanvasLength(canvasLength);
        SetCanvasVariables(canvasLength);
    }

    function SetCanvasVariables(canvasLength) {
        Canvas.width = canvasLength;
        Canvas.height = canvasLength;
        console.log("Canvas length: ", canvasLength);
        setOtherCanvasVariables(canvasLength);
    }

    /*
        Returns corrected canvas length, so that cell size would be whole number.
    */
    function getCanvasLength(currentLength) {
        var arenaLength = currentLength * (1-(relMarginSize));
        var cellSize = arenaLength / cellCount;
        baseCell.size = cellSize;
        return (Math.floor(cellSize) * cellCount) / (1 - (relMarginSize));
    }
    
    function ClearCanvas() {
        CanvasContext.clearRect(0, 0, Canvas.width, Canvas.height);
    }

    function setOtherCanvasVariables(canvasLength) {
        length = canvasLength;

        margin = length * 0.1;

        TLborder = length * relMarginSize * 0.5;
        BRborder = length - (TLborder * 2);
        console.log("Real arena length:", BRborder-TLborder);

        //baseCell.size = length * 0.05;
        console.log("Cell Length: ", baseCell.size);
    }

    function DrawCanvas() {

        console.log(TLborder, BRborder);
        CanvasContext.fillStyle = "yellow";
        CanvasContext.fillRect(0, 0, length, length);
        CanvasContext.stroke();

        CanvasContext.fillStyle = "white";
        CanvasContext.fillRect(TLborder, TLborder, BRborder, BRborder);
        CanvasContext.stroke();

        DrawCanvasBorder();
      
    }
    function DrawCanvasBorder() {
        CanvasContext.beginPath();
        CanvasContext.moveTo(1, 1);
        CanvasContext.lineTo(Canvas.width-1, 1);
        CanvasContext.lineTo(Canvas.width-1, Canvas.height-1);
        CanvasContext.lineTo(1, Canvas.height-1);
        CanvasContext.lineTo(1, 1);
        CanvasContext.strokeStyle = "red";
        CanvasContext.stroke();
    }

    function DrawGrid() {
        var x = TLborder;
        var y = TLborder;

        while (x <= BRborder) {
            while (y <= BRborder) {
                DrawFillRenctangle(x, y, baseCell.size, "red");
                DrawOutlineRectangle(x, y, baseCell.size, "green");
                y += baseCell.size;
            }
            y = TLborder;
            x += baseCell.size;
        }
    }

    function DrawFillRenctangle(x, y, length, fillColor) {
        CanvasContext.fillStyle = fillColor;
        CanvasContext.fillRect(x, y, length, length);
    }

    function DrawOutlineRectangle(x, y, length, outlineColor) {
        CanvasContext.strokeStyle = outlineColor;
        //CanvasContext.rect(x, y, x + length, y + length);
        CanvasContext.rect(x, y, length, length);
        CanvasContext.stroke();
    }

    function DrawRectangle(x, y, xx, yy, fillColor, outlineColor) {
        CanvasContext.fillStyle = fillColor;
        CanvasContext.fillRect(x, y, xx, yy);
        CanvasContext.strokeStyle =outlineColor;
        CanvasContext.rect(x, y, xx, yy);
        CanvasContext.stroke();
    }
})();