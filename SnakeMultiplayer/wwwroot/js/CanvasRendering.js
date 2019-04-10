(function () {

    var Canvas = document.getElementById("Canvas");
    var CanvasContext = Canvas.getContext("2d");
    window.addEventListener('resize', ResizeCanvas, false);

    var canvasWidth;
    var canvasHeight;
    var contextWidth;
    var contextHeight;

    ResizeCanvas();
    DrawCanvas();

    function ResizeCanvas() {
        var size = -1;
        if (window.innerWidth > window.innerHeight) {
            size = window.innerHeight - 10;
        } else {
            size = window.innerWidth - 10;
        }

        Canvas.width = size;
        Canvas.height = size;
        console.log("canvas params: ", Canvas.width, Canvas.height);
        DrawCanvas();
    }

    function DrawCanvas() {

        CanvasContext.fillStyle = "yellow";
        CanvasContext.fillRect(0, 0, Canvas.width, Canvas.height);
        CanvasContext.stroke();

        CanvasContext.fillStyle = "blue";
        CanvasContext.fillRect(50, 50, Canvas.width - 100, Canvas.height - 100);
        CanvasContext.stroke();

        DrawBorder();
    }
    function DrawBorder() {
        CanvasContext.beginPath();
        CanvasContext.moveTo(1, 1);
        CanvasContext.lineTo(Canvas.width, 1);
        CanvasContext.lineTo(Canvas.width, Canvas.height);
        CanvasContext.lineTo(1, Canvas.height);
        CanvasContext.lineTo(1, 1);
        CanvasContext.strokeStyle = "#red";
        CanvasContext.stroke();

    }
})();