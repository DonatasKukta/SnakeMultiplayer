var Canvas = document.getElementById("Canvas");
var CanvasContext = Canvas.getContext("2d");
// Constants that depend on current screen size 
var length;
var TLborder;
var BRborder;
var margin;
var baseCell = new Cell(undefined, "whitesmoke", "dimgrey");
var cellCount = 20;
var relMarginSize = 0.1;
var canvasLength = -1;

function onResize() {
    console.log(">>Resizing")
    ClearCanvas();
    ResizeCanvas();
    DrawBaseCanvas();
}

function ResizeCanvas() {
    
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
    //console.log("Canvas length: ", canvasLength);
    setOtherCanvasVariables(canvasLength);
}

/*
    Returns corrected canvas length, so that cell size would be whole number.
*/
function getCanvasLength(currentLength) {
    var arenaLength = currentLength * (1 - (relMarginSize));
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
    //console.log("Real arena length:", BRborder - TLborder);
    //console.log("Cell Length: ", baseCell.size);
}

function DrawBaseCanvas() {

    console.log(TLborder, BRborder);
    CanvasContext.fillStyle = "lightgrey";
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
    CanvasContext.lineTo(Canvas.width - 1, 1);
    CanvasContext.lineTo(Canvas.width - 1, Canvas.height - 1);
    CanvasContext.lineTo(1, Canvas.height - 1);
    CanvasContext.lineTo(1, 1);
    CanvasContext.strokeStyle = "red";
    CanvasContext.stroke();
}
function DrawFillRenctangle(x, y, length, fillColor) {
    CanvasContext.fillStyle = fillColor;
    CanvasContext.fillRect(x, y, length, length);
}

function DrawOutlineRectangle(x, y, length, outlineColor) {
    CanvasContext.strokeStyle = outlineColor;
    CanvasContext.rect(x, y, length, length);
    CanvasContext.stroke();
}

function DrawRectangle(x, y, xx, yy, fillColor, outlineColor) {
    CanvasContext.fillStyle = fillColor;
    CanvasContext.fillRect(x, y, xx, yy);
    CanvasContext.strokeStyle = outlineColor;
    CanvasContext.rect(x, y, xx, yy);
    CanvasContext.stroke();
}

function DrawText(text, startx, starty) {
    CanvasContext.save();
    var middle = canvasLength / 2;
    CanvasContext.textAlign = "center";

    CanvasContext.font = "80px  Arial"

    CanvasContext.strokeStyle = "black";
    CanvasContext.lineWidth = 8;
    //CanvasContext.strokeText(text, startx, starty)
    CanvasContext.strokeText(text, middle, starty)
    CanvasContext.fillStyle = "red"
    //CanvasContext.fillText(text, startx, starty);
    CanvasContext.fillText(text, middle, starty);
    CanvasContext.restore();
}
