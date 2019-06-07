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

    clearCoords(coordinateArray) {
        if (coordinateArray == null)
            return;
        var i;
        for (i = 0; i < coordinateArray.length; i++) {
            //var cordx = this.getCellCoord();
            //var cordy = this.getCellCoord();

            this.drawBaseCell(coordinateArray[i].x, coordinateArray[i].y);
        }
    }

    initializeSnake(snakeBody, color) {
        //iterate through snakeBody
        var i;
        for (i = 0; i < snakeBody.length; i++) {
            this.updateSnake(color, snakeBody[i]);
        }
    }

    updateSnake(snakeColor, head, tail = null) {
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

    drawBaseCell(x, y) {
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

    drawGameOver() {
        var gameOverY = this.getCellCoord(7);
        var gameOverX = this.getCellCoord(4);
        DrawText("Game Over", gameOverY, gameOverY);
    }
}