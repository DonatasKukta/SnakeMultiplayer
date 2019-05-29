(function () {
    window.addEventListener('resize', reSet, false);
    var gameController = new GameController();
    
    gameController.setEnvironment();
    gameController.setCellContainer(new CellGridContainer(cellCount, baseCell, CanvasContext, TLborder, BRborder)); gameController.doStubActions();
    
    //Absurdsss
    /*
    gameController.socket.onopen = function (e) {
        gameController.send("inicijuota...");
        gameController.send("inicijuota23..");
    }*/
    

    document.onkeydown = function (e) {
        switch (e.key) {
            case 'ArrowUp':
                gameController.sendUpdate(MoveDirection.Up);
                break;
            case 'ArrowDown':
                gameController.sendUpdate(MoveDirection.Down);
                gameController.onOpenedSocket();
                break;
            case 'ArrowLeft':
                gameController.sendUpdate(MoveDirection.Left);
                break;
            case 'ArrowRight':
                gameController.sendUpdate(MoveDirection.Right);
                break;
        }
    };

    function metodasKazkoks() {
        console.log("iskviestas metodas");
    }

    function reSet() {
        onResize();
        gameController.setCellContainer(new CellGridContainer(cellCount, baseCell, CanvasContext, TLborder, BRborder));
        gameController.drawSnakes();       
    }

    function onSnakeMovement() {

    }
})();