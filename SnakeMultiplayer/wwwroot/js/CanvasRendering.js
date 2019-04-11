(function () {

    var Canvas = document.getElementById("Canvas");
    var CanvasContext = Canvas.getContext("2d");
    window.addEventListener('resize', onResize, false);
    // Constants that depend on current screen size 
    var length;
    var TLborder;
    var BRborder;

    var canvasWidth;
    var canvasHeight;

    //Initialization of page:
    onResize();

    function onResize() {
        ResizeCanvas();
        DrawCanvas();
    }

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
        length = size;
        console.log("Length:", length);
        TLborder = length * 0.1 * 0.5;
        BRborder = length - (TLborder *2);
        //return size;
    }

    function DrawCanvas() {

        console.log(TLborder, BRborder);
        CanvasContext.fillStyle = "yellow";
        CanvasContext.fillRect(0, 0, length, length);
        CanvasContext.stroke();

        CanvasContext.fillStyle = "blue";
        CanvasContext.fillRect(TLborder, TLborder, BRborder, BRborder);
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