(function () {
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