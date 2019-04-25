(function () {
    //ENUMERATORS:
    
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


    //DATA STRUCTURES:
class CustomLinkedListNode {
    constructor(element) {
        this.element = element;
        this.next = null;
        this.previous = null;
    }
}

class CustomLinkedList {
    constructor() {
        this.count = 0;
        this.head = null;
        this.tail = null;
        
    }

    getFirst() {
        return this.head.element;
    }

    addFirst(element) {
        var newNode = new CustomLinkedListNode(element);

        if (this.head == null) {
            this.head = newNode;
        } else if (this.count === 1) {
            newNode.next = this.head;
            this.head = newNode;
            this.tail = this.head.next;
            this.tail.previous = this.head;
        } else {
            newNode.next = this.head
            this.head = newNode;
            this.head.next.previous = this.head;
        }
        this.count += 1;
    }

    deleteLast() {
        var deleted;
        this.count -= 1;

        if (this.head === null) {
            console.warn("Trying to delete element from empty linked list!");
            return null;
        } else if (this.head.next == null) {
            deleted = this.head.element;
            this.head = null;
            return deleted;
        } else {
            deleted = this.tail.element;
            this.tail = this.tail.previous;
            this.tail.next = null;
            return deleted;
        }
    }

    print() {
        console.log("LinkedList elements:");
        var node = this.head;
        while (node !== null) {
            console.log(node.element);
            node = node.next;
        }
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
        }

        updateSnake( snakeColor, head, tail = null) {
            this.drawCell(this.getCellCoord(head.x), this.getCellCoord(head.y), snakeColor);
            if (tail !== null) {
                this.drawCell(tail.x, tail.y, this.baseCellParams.innerColor);
            }
        }
        
        drawCell(x, y, fillColor) {
            var coordx = getCellCoord(x);
            var coordy = getCellCoord(y);
            DrawFillRenctangle(coordx, coordy, this.baseCellParams.size, fillColor);
            DrawOutlineRectangle(coordx, coordy, this.baseCellParams.size, this.baseCellParams.outlineColor);
        }

        drawBaseCell(x,y) {
            var coordx = getCellCoord(x);
            var coordy = getCellCoord(y);
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
            this.color = color;
        }

        setStartPoint(x, y) {
            this.currDirection = MoveDirection.Down;
            this.body.addFirst(new Cell(baseCell.size, this.color, baseCell.outlineColor,x,y));
        }


        //direction- enum objekto reiksme
        update(direction, isFood) {
            var currentHead = this.body.getFirst();
            var newHead;

            switch (direction) {
                case MoveDirection.Up:
                    newHead = currentHead.y - 1;
                    break;
                case MoveDirection.Right:
                    newHead = currentHead.x + 1;
                    this.y += 1;
                    break;
                case MoveDirection.Down:
                    newHead = currentHead.y + 1;
                    this.x -= 1;
                    break;
                case MoveDirection.Left:
                    newHead = currentHead.x - 1;
                    break;
                case MoveDirection.None:
                    return;
                default:
                    console.error("Unexpected direction value!", direction);
                    return;
                }

            body.addFirst(newHead);
            //
            // container -> drawCell(newHead.x, newHead.y, this.color);
            if (isFood === true) {

            } else {
                var tail = body.deleteLast();
                //
                // container -> drawBaseCell(tail.x,tail.y);
            }
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
    var CellsContainer;
    var snake = new Snake("Donatas", "blue");
    snake.setStartPoint(2, 2);


    onResize();

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
        CellsContainer = new CellGridContainer(cellCount, baseCell, CanvasContext, TLborder, BRborder);
        CellsContainer.createGrid(false);
        CellsContainer.drawGrid();
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