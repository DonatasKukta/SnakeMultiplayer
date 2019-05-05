class WebSocketController {
    constructor() {
        var scheme = document.location.protocol === "https:" ? "wss" : "ws";
        var port = document.location.port ? (":" + document.location.port) : "";
        this.connectionUrl = scheme + "://" + document.location.hostname + port + "/ws";
        this.socket;
        this.state = "initialized";
        console.log("Connection URL: " + this.connectionUrl);
        //this.messageReceivedEvent = new CustomEvent();
    }

    connect() {
        this.socket = new WebSocket(this.connectionUrl);
        this.state = WebSocket.CONNECTING;
        this.socket.onopen = function (event) {
            this.state = "Open";
            console.log("Socket opened");
        };
        this.socket.onclose = function (event) {
            this.state = "Closed";
            console.log("Connection closed. Code:" + htmlEscape(event.code) + ".Reason: " + htmlEscape(event.reason));
        };
        this.socket.onerror = function () {
            this.state = "Error";
            console.warn("Error occured! ");
        }

        this.socket.onmessage = function (event) {
            console.warn("Received message: " + htmlEscape(event.data));
        };
    }

    send(message) {
        if (!this.socket || this.socket.readyState !== WebSocket.OPEN) {
            console.warn("trying to send data to not open socket!");
            return false;
        }
        this.socket.send(message);
        console.log("Sent message: " + message);
        return true;
    }

    close() {
        if (!this.socket || this.socket.readyState !== WebSocket.OPEN) {
            console.log("socket not connected");
        }
        this.socket.close(1000, "Closing from client");
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