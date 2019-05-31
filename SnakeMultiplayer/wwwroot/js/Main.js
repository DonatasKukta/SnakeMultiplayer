(function () {
    function getCookie(name) {
        var value = "; " + document.cookie;
        var parts = value.split("; " + name + "=");
        if (parts.length == 2) return parts.pop().split(";").shift();
    }
    var PlayerName = getCookie("PlayerName");
    var LobbyId = getCookie("LobbyId");

    if (PlayerName == null || LobbyId == null) {
        // Redirect to error page.
    }

    window.addEventListener('resize', reSet, false);
    var gameController = new GameController(PlayerName, LobbyId);
    
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