class SignalRController {
    constructor(playerName, lobbyId, dispatcher) {
        this.dispatcher = dispatcher;
        this.playerName = playerName;
        this.lobbyId = lobbyId;

        this.connection = new signalR.HubConnectionBuilder()
            .withUrl("/LobbyHub")
            .configureLogging(signalR.LogLevel.Information)
            .build();
    }

    async connect() {
        this.connection.on("OnPing", this.onPing);
        this.connection.on("OnSettingsUpdate", this.onMessage);
        this.connection.on("OnPlayerStatusUpdate", this.onMessage);
        this.connection.onclose(this.onClose);
        try {
            console.warn("SignalR Connecting.");
            //this.connection.start()
            //    .then(this.connection.invoke("Ping"))
            //    .then(this.onOpen());

            await this.connection.start();
            console.warn("SignalR Connected.");
            await this.connection.invoke("Ping");
            await this.joinLobby();
            this.onOpen();
        } catch (err) {
            console.log(err);
            this.onError();
        }
    }

    onPing(message) {
        console.warn("Ping recieved from Server:", message);
    }

    onOpen(event) {
        console.log("SignalR connection established");
        this.dispatcher.dispatch("onSocketOpen", event);
    }

    onClose(event) {
        console.log("Connection closed. Code:" + htmlEscape(event.code) + ".Reason: " + htmlEscape(event.reason));
        this.dispatcher.dispatch("onSocketClose", event);
    }

    onError(event) {
        console.warn("Error occured! ");
        this.dispatcher.dispatch("onSocketError", event);
    }

    // Methods invoked by server
    onMessage(event) {
        console.warn("SignalR received event:", event);
        var MessageObject = JSON.parse(event.data);
        this.dispatcher.dispatch("onSocketMessage", MessageObject);
    }

    // Methods to call server
    async joinLobby() {
        await this.connection.invoke("JoinLobby", this.lobbyId, this.playerName);
    }

    updateLobbySettings(settings) {
        this.connection.invoke("UpdateLobbySettings", this.wrapMessage("Settings", settings));
    }

    initiateGameStart() {
        this.connection.invoke("InitiateGameStart", this.wrapMessage("Start", message));
    }

    updatePlayerState(direction) {
        this.connection.invoke("UpdatePlayerState", this.wrapMessage("Update", direction));
    }

    // Helpers
    wrapMessage(messageType, messageBody) {
        var message = {
            sender: this.playerName,
            lobby: this.lobbyId,
            type: messageType,
            body: messageBody
        };
        return message;
    }

    close() {
        if (!this.socket || this.socket.readyState !== WebSocket.OPEN) {
            console.log("Trie to close not connected socket");
        }
        this.socket.close(1000, "Closing web socket from client");
    }

    getSocketState() {
        return this.socket.readyState;
    }
}

function htmlEscape(str) {
    return str.toString()
        .replace(/&/g, '&amp;')
        .replace(/"/g, '&quot;')
        .replace(/'/g, '&#39;')
        .replace(/</g, '&lt;')
        .replace(/>/g, '&gt;');
}
