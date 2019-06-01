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

    if (document.getElementById("startButton") !== null) {
    document.getElementById("startButton").onclick = onStartGameButtonClick;
    }



    window.addEventListener('resize', reSet, false);
    var MainDispatcher = new Dispatcher();
    MainDispatcher.on("onPlayerListReceived", updatePlayers.bind(this));
    MainDispatcher.on("onExitReceived", redirectToErrorPage.bind(this));


    var gameController = new GameController(PlayerName, LobbyId, MainDispatcher);
    gameController.setEnvironment();
    gameController.setCellContainer(new CellGridContainer(cellCount, baseCell, CanvasContext, TLborder, BRborder)); gameController.doStubActions();
    
    window.addEventListener('beforeunload', (event) => {
        gameController.socketController.close();
    });    
    document.onkeydown = function (e) {
        switch (e.key) {
            case 'ArrowUp':
                gameController.sendMovementUpdate(MoveDirection.Up);
                break;
            case 'ArrowDown':
                gameController.sendMovementUpdate(MoveDirection.Down);
                break;
            case 'ArrowLeft':
                gameController.sendMovementUpdate(MoveDirection.Left);
                break;
            case 'ArrowRight':
                gameController.sendMovementUpdate(MoveDirection.Right);
                break;
        }
    };

    function reSet() {
        onResize();
        gameController.setCellContainer(new CellGridContainer(cellCount, baseCell, CanvasContext, TLborder, BRborder));
        gameController.drawSnakes();       
    }

    function onUpdateSettings() {
        this.gameController.sendSettingUpdate();
    }

    function onLobbyExit() {

    }

    function onStartGameButtonClick(e) {
        //console.warn("Iskviestas pradėjimo mygtukas");
        this.gameController.sendUpdate();
    }

    function onCountDownEvent(e) {

    }

    function onGameStartRececeived(e) {

        showCanvas();
    }

    function onGameEndReceived(e) {
        //re-set canvas state
        hideCanvas();
    }

    function showCanvas() {
        var element = document.getElementById('Canvas');
        console.log("canvas element:", element);
        element.style.visibility = 'visible';
    }

    function hideCanvas() {
        var element = document.getElementById('Canvas');
        console.log("canvas element:", element);
        //element.style.display = 'none'; //or
        element.style.visibility = 'hidden';
    }

    function updatePlayers(players) {
        console.log("HTML Received update players: ", players);

        if (!Array.isArray(players)) {
            console.error("Not error received at update table: ", players);
        }

        var playerCardList = document.getElementById("playerCards");
        while (playerCardList.hasChildNodes()) {
            playerCardList.removeChild(playerCardList.firstChild);
        }
        /* Card html example
     <div class="playerCard card" style="background-color: greenyellow;">
        <h5 class="card-header">Donatas</h5>
    </div>
         */

        players.forEach(function (player) {
            var playerCard = document.createElement("div");
            playerCard.className = "playerCard card";
            playerCard.style = "background-color: " + player.color + ";";
            //playerCard.innerHTML = "<h5 class=\"card-header\"> " + player.name + "</h5> <div class=\"card-body\"><p class=\"card-text\">" + player.type + "</p></div>";
            var hostString = (player.type === "Host") ? " [host]" : "";
            playerCard.innerHTML = "<h5 class=\"card-header\"> " + player.name + hostString + "</h5>";
            console.log("Kortele:", playerCard);
            playerCardList.appendChild(playerCard);
            console.log("Naujas sarasas: ", playerCardList);
        });
    }

    function redirectToErrorPage(message) {
        submitForm("/Home/Error", message);
    }
    // Source: https://stackoverflow.com/questions/133925/javascript-post-request-like-a-form-submit
    function submitForm(path, params, method = 'post') {

        // The rest of this code assumes you are not using a library.
        // It can be made less wordy if you use one.
        const form = document.createElement('form');
        form.method = method;
        form.action = path;

        for (const key in params) {
            if (params.hasOwnProperty(key)) {
                const hiddenField = document.createElement('input');
                hiddenField.type = 'hidden';
                hiddenField.name = key;
                hiddenField.value = params[key];

                form.appendChild(hiddenField);
            }
        }

        document.body.appendChild(form);
        form.submit();
    }
})();