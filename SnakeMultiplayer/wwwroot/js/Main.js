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
        redirectToErrorPage("An error has occured. Probably could not use cookies. Please enable cookies and try again.");
    }

    var MainDispatcher = new Dispatcher();
    MainDispatcher.on("onPlayerListReceived", updatePlayers.bind(this));
    MainDispatcher.on("onExitReceived", redirectToErrorPage.bind(this));
    MainDispatcher.on("onStartReceived", onGameStartRececeived.bind(this));
    MainDispatcher.on("onGameEndReceived", onGameEndReceived.bind(this));
    MainDispatcher.on("onWebSocketOpened", EnableHostButtons.bind(this));
    MainDispatcher.on("onWebSocketClosed", redirectToErrorPage.bind(this)); // to be implemented
    MainDispatcher.on("onSettingsReceived", onUpdateSettings.bind(this));


    var gameController = new GameController(PlayerName, LobbyId, MainDispatcher);
    gameController.setEnvironment();
    gameController.setCellContainer(new CellGridContainer(cellCount, baseCell, CanvasContext, TLborder, BRborder)); 

    window.addEventListener('beforeunload', (event) => {
        gameController.socketController.close();
    });    
    window.addEventListener('resize', reSet.bind(this), false);
    
    DisableHostButtons();

    if (document.getElementById("startButton") !== null) {
        document.getElementById("startButton").onclick = onStartGameButtonClick.bind(this);
    }

    if (document.getElementById("SlowSpeedBtn") !== null) {
        document.getElementById("SlowSpeedBtn").addEventListener('click', function () {
            gameController.sendSettingUpdate({ speed: "Slow" });
        })
        }
    
    if (document.getElementById("NormalSpeedBtn") !== null) {
        document.getElementById("NormalSpeedBtn").addEventListener('click', function () {
            gameController.sendSettingUpdate({ speed: "Normal" });
        })
    }
    if (document.getElementById("FastSpeedBtn") !== null) {
        document.getElementById("FastSpeedBtn").addEventListener('click', function () {
            gameController.sendSettingUpdate({ speed: "Fast" });
        })
    }
    if (document.getElementById("NoSpeedBtn") !== null) {
        document.getElementById("NoSpeedBtn").addEventListener('click', function () {
            gameController.sendSettingUpdate({ speed: "NoSpeed" });
        })
    }

    var keyDownController = {};

    document.onkeydown = function (e) {
        switch (e.key) {
            case 'ArrowUp':
                e.preventDefault();
                if (keyDownController['ArrowUp'] == null) {
                    gameController.sendMovementUpdate(MoveDirection.Up);
                    keyDownController['ArrowUp'] = true;
                }
                break;
            case 'ArrowDown':
                e.preventDefault();
                if (keyDownController['ArrowDown'] == null) {
                    gameController.sendMovementUpdate(MoveDirection.Down);
                    keyDownController['ArrowDown'] = true;
                }
                break;
            case 'ArrowLeft':
                e.preventDefault();
                if (keyDownController['ArrowLeft'] == null) {
                    gameController.sendMovementUpdate(MoveDirection.Left);
                    keyDownController['ArrowLeft'] = true;
                }
                break;
            case 'ArrowRight':
                e.preventDefault();
                if (keyDownController['ArrowRight'] == null) {
                    gameController.sendMovementUpdate(MoveDirection.Right);
                    keyDownController['ArrowRight'] = true;
                }
                break;
        }
    };

    document.onkeyup = function (e) {
        switch (e.key) {
            case 'ArrowUp':
                keyDownController['ArrowUp'] = null;
                break;
            case 'ArrowDown':
                e.preventDefault();
                keyDownController['ArrowDown'] = null;
                break;
            case 'ArrowLeft':
                e.preventDefault();
                keyDownController['ArrowLeft'] = null;
                break;
            case 'ArrowRight':
                e.preventDefault();
                keyDownController['ArrowRight'] = null;
                break;
        }
    }

    function reSet() {
        onResize();
        gameController.setCellContainer(new CellGridContainer(cellCount, baseCell, CanvasContext, TLborder, BRborder));
        gameController.drawSnakes();       
    }

    function EnableHostButtons() {
        if (document.getElementById("startButton") !== null) {
        document.getElementById("startButton").disabled = false;
        }
        if (document.getElementById("SpeedBtn") !== null) {
            document.getElementById("SpeedBtn").disabled = false;
        }
    }
    function DisableHostButtons() {
        if (document.getElementById("startButton") !== null) {
        document.getElementById("startButton").disabled = true;
        }
        if (document.getElementById("SpeedBtn") !== null) {
            document.getElementById("SpeedBtn").disabled = true;
        }
    }


    function onUpdateSettings(settings) {
        var speed;
        if (settings !== null || settings.speed !== null) {
            speed = settings.speed;
            var speedBtn = document.getElementById("SpeedBtn");
            if (speedBtn !=null)
                speedBtn.textContent = "Speed: " + speed;
        }
    }

    function onStartGameButtonClick(e) {
        gameController.sendGameStart();
    }
    
    function onGameStartRececeived(e) {
        var element = document.getElementById('Canvas');
        element.style.visibility = 'visible';
        element.scrollIntoView({
            behavior: "smooth",
            block: "start",
            inline: "start",
        });
        DisableHostButtons();
    }

    async function onGameEndReceived(e) {
        await Sleep(3000);
        var element = document.getElementById('Canvas');
        console.log("canvas element:", element);
        //element.style.display = 'none'; //or
        element.style.visibility = 'hidden';
        document.getElementById('navigation_bar').scrollIntoView({
            behavior: "smooth",
            block: "start",
            inline: "start",
        });
        
        EnableHostButtons();
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
    // source: https://stackoverflow.com/questions/951021/what-is-the-javascript-version-of-sleep
    function Sleep(ms) {
        return new Promise(resolve => setTimeout(resolve, ms));
    }
})();