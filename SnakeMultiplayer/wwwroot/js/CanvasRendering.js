﻿(function () {
    //DATA STRUCTURES
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

        createGrid() {
            this.Cells = new Array(this.gridSize);
            var x, y;
            for (x = 0; x < this.gridSize; x++) {
                this.Cells[x] = new Array(this.gridSize);
                for (y = 0; y < this.gridSize; y++) {
                    this.Cells[x][y] = new Cell(this.baseCellParams.size, this.baseCellParams.innerColor, this.baseCellParams.outlineColor, this.getCellCoord(x), this.getCellCoord(y));
                    this.drawFullCell(this.Cells[x][y]);
                }
            }
        }

        getCellCoord(cellNumber) {
            return this.startBorder + (cellNumber * this.baseCellParams.size);
        }

        drawFullCell(cell) {
            DrawFillRenctangle(cell.x, cell.y, cell.size, cell.innerColor);
            DrawOutlineRectangle(cell.x, cell.y, cell.size, cell.outlineColor);
        }
    }

    class Snake {
        constructor(name, color) {
            var x = new Array
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
    var cellCount = 18;
    var relMarginSize = 0.1;

    var CellsContainer;
    onResize();

    var linkedList = new CustomLinkedList();
    //-----------------------

    function onResize() {
        console.log(">>Resizing")
        ClearCanvas();
        ResizeCanvas();
        DrawCanvas();
        //DrawGrid();
        setCells();
    }

    function setCells() {
        CellsContainer = new CellGridContainer(cellCount, baseCell, CanvasContext, TLborder, BRborder);
        //CellsContainer.drawGrid();
        CellsContainer.createGrid();
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
        console.log("canvas params: ", Canvas.width, Canvas.height);
        setOtherCanvasVariables(canvasLength);
    }

    /*
        Returns corrected canvas length, so that cell size would be whole number.
    */
    function getCanvasLength(currentLength) {
        var arenaLength = currentLength * (1-(relMarginSize));
        cellSize = arenaLength / cellCount;
        return (Math.floor(cellSize) * cellCount) / (1 - (relMarginSize))
    }
    
    function ClearCanvas() {
        CanvasContext.clearRect(0, 0, Canvas.width, Canvas.height);
    }

    function setOtherCanvasVariables(canvasLength) {
        length = canvasLength;
        console.log("Length:", length);

        margin = length * 0.1;

        TLborder = length * relMarginSize * 0.5;
        BRborder = length - (TLborder * 2);
        console.log("Real arena length:", BRborder-TLborder);

        baseCell.size = length * 0.05;
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