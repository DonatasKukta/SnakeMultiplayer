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
    document.getElementById("startButton").onclick = onStartGameButtonClick.bind(this);
    }
    
    window.addEventListener('resize', reSet, false);
    var MainDispatcher = new Dispatcher();
    MainDispatcher.on("onPlayerListReceived", updatePlayers.bind(this));
    MainDispatcher.on("onExitReceived", redirectToErrorPage.bind(this));
    MainDispatcher.on("onStartReceived", onGameStartRececeived.bind(this));
    MainDispatcher.on("onGameEndReceived", onGameEndReceived.bind(this));
    MainDispatcher.on("onWebSocketOpened", EnableStartButton.bind(this));
    MainDispatcher.on("onWebSocketClosed", null); // to be implemented

    DisableStartButton();

    var gameController = new GameController(PlayerName, LobbyId, MainDispatcher);
    gameController.setEnvironment();
    gameController.setCellContainer(new CellGridContainer(cellCount, baseCell, CanvasContext, TLborder, BRborder)); 

    window.addEventListener('beforeunload', (event) => {
        gameController.socketController.close();
    });    
    document.onkeydown = function (e) {
        switch (e.key) {
            case 'ArrowUp':
                e.preventDefault();
                gameController.sendMovementUpdate(MoveDirection.Up);
                break;
            case 'ArrowDown':
                e.preventDefault();
                gameController.sendMovementUpdate(MoveDirection.Down);
                break;
            case 'ArrowLeft':
                e.preventDefault();
                gameController.sendMovementUpdate(MoveDirection.Left);
                break;
            case 'ArrowRight':
                e.preventDefault();
                gameController.sendMovementUpdate(MoveDirection.Right);
                break;
        }
    };

    function reSet() {
        onResize();
        gameController.setCellContainer(new CellGridContainer(cellCount, baseCell, CanvasContext, TLborder, BRborder));
        gameController.drawSnakes();       
    }

    function EnableStartButton() {
        if (document.getElementById("startButton") !== null) {
        document.getElementById("startButton").disabled = false;
        }
    }
    function DisableStartButton() {
        if (document.getElementById("startButton") !== null) {
        document.getElementById("startButton").disabled = true;
        }
    }

    // To be implemented
    function onUpdateSettings() {
        gameController.sendSettingUpdate();
    }
    // To be implemented
    function onCountDownEvent(e) {

    }

    function onStartGameButtonClick(e) {
        gameController.sendGameStart();
    }
    
    function onGameStartRececeived(e) {
        var element = document.getElementById('Canvas');
        element.style.visibility = 'visible';
        element.scrollIntoView();
        DisableStartButton();
    }

    function onGameEndReceived(e) {
        var element = document.getElementById('Canvas');
        console.log("canvas element:", element);
        //element.style.display = 'none'; //or
        element.style.visibility = 'hidden';
        document.getElementById('navigation_bar').scrollIntoView();
        EnableStartButton();
    }

    function updatePlayers(players) {
        console.log("HTML Received update players: ", players);

        if (!Array.isArray(players)) {
            console.error("Not array error received at update table: ", players);
        }

        var playerCardList = document.getElementById("playerCards");
        while (playerCardList.hasChildNodes()) {
            playerCardList.removeChild(playerCardList.firstChild);
        }
        players.forEach(function (player) {
            var playerCard = document.createElement("div");
            playerCard.className = "playerCard card";
            playerCard.style = "background-color: " + player.color + ";";
            var hostString = (player.type === "Host") ? " [host]" : "";
            playerCard.innerHTML = "<h5 class=\"card-header\"> " + player.name + hostString + "</h5>";
            playerCardList.appendChild(playerCard);
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