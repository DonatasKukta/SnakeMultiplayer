class SignalRController {
    constructor(playerName, lobbyId, dispatcher) {
        this.dispatcher = dispatcher;
        this.playerName = playerName;
        this.lobbyId = lobbyId;

        this.connection = new signalR.HubConnectionBuilder()
            .withUrl("/LobbyHub")
            .configureLogging(signalR.LogLevel.Information)
            .build();

        this.connection.on("OnPing", () => this.onPing());

        //TODO: Refactor to separate methods
        this.connection.on("OnSettingsUpdate", (message) => this.onMessage("OnSettingsUpdate", message));
        this.connection.on("OnPlayerStatusUpdate", (message) => this.onMessage("OnPlayerStatusUpdate", message));
        this.connection.on("OnGameEnd", (message) => this.onMessage("OnGameEnd", message));
        this.connection.on("OnLobbyMessage", (message) => this.onMessage("OnLobbyMessage", message));
        this.connection.on("OnGameStart", (message) => this.onMessage("OnGameStart", message));
        this.connection.on("OnArenaStatusUpdate", (message) => this.onMessage("OnArenaStatusUpdate", message));
        this.connection.onclose((event) => this.onClose(event));
    }

    async connect() {
        try {
            console.warn("SignalR Connecting.");
            await this.connection.start();
            console.warn("SignalR Connected.");
            await this.connection.invoke("Ping");
            await this.joinLobby();
            this.onOpen();
        } catch (err) {
            console.log(err);
            this.onError(err);
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
        console.warn("onClose event", event);
        this.dispatcher.dispatch("onSocketClose", event);
    }

    onError(event) {
        console.warn("Error occured! ", event);
        this.dispatcher.dispatch("onSocketError", event);
    }

    // Methods invoked by server
    onMessage(invokedMethod, MessageObject) {
        console.warn(`Server invoked method ${invokedMethod}:`, MessageObject);
        console.warn("onMessage:dispatcher", this.dispatcher);
        this.dispatcher.dispatch("onSocketMessage", MessageObject);
    }

    // Methods to call server
    async joinLobby() {
        await this.connection.invoke("JoinLobby", this.lobbyId, this.playerName);
    }

    updateLobbySettings(settings) {
        this.connection.invoke("UpdateLobbySettings", settings);
    }

    initiateGameStart() {
        this.connection.invoke("InitiateGameStart");
    }

    updatePlayerState(direction) {
        this.connection.invoke("UpdatePlayerState", direction);
    }

    //TODO: implement
    close() {
        console.log("Tried to close SignalR connection");
        //if (!this.socket || this.socket.readyState !== WebSocket.OPEN) {
        //    console.log("Trie to close not connected socket");
        //}
        //this.socket.close(1000, "Closing web socket from client");
    }
}

