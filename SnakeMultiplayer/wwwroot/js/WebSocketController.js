class WebSocketController {
    constructor(playerName, lobbyId, dispatcher) {
        this.dispatcher = dispatcher;
        var scheme = document.location.protocol === "https:" ? "wss" : "ws";
        var port = document.location.port ? (":" + document.location.port) : "";
        this.connectionUrl = scheme + "://" + document.location.hostname + port + "/ws";
        this.socket;
        console.log("Connection URL: " + this.connectionUrl);
        this.playerName = playerName;
        this.lobbyId = lobbyId;
        //this.socketMessageEvent = new Event('onSocketReceivedMessage');
        //this.socketCloseEvent   = new Event('onSocketClosed');
        //this.socketErrorEvent   = new Event('onSocketError');
    }

    connect() {
        this.socket = new WebSocket(this.connectionUrl);
        this.socket.addEventListener("open", this.onOpen.bind(this));
        this.socket.addEventListener("close", this.onClose.bind(this));
        this.socket.addEventListener("error", this.onError.bind(this));
        this.socket.addEventListener("message", this.onMessage.bind(this));
    }

    onOpen(event) {
        console.log("Socket opened");
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

    onMessage(event) {
        var MessageObject = JSON.parse(event.data);
        //console.warn("Received message: ", MessageObject);
        this.dispatcher.dispatch("onSocketMessage", MessageObject);
    }

    send(messageType, messageBody) {
        if (!this.socket || this.socket.readyState !== WebSocket.OPEN) {
            console.warn("trying to send data to not open socket!");
            return false;
        }
        var message = this.wrapMessage(messageType, messageBody);
        this.socket.send(JSON.stringify(message));
        console.log("Sent message: " , message);
        return true;
    }

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
