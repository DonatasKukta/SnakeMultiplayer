using JsonLibrary;
using System.Diagnostics;
using System.Net.WebSockets;
using System.Threading.Tasks;
using System.Threading;
using System;

namespace SnakeMultiplayer.Services;

public interface IWebSocketHandler
{
    Task HandleWebSocketAsync(WebSocket webSocket);
}

public class WebSocketHandler : IWebSocketHandler
{
    readonly int WebSocketMessageBufferSize = 1024 * 4;

    readonly GameServerService gameServer;

    public WebSocketHandler(GameServerService gameServer)
    {
        this.gameServer = gameServer;
    }

    public async Task HandleWebSocketAsync(WebSocket webSocket)
    {
        var lobby = string.Empty;
        var playerName = string.Empty;
        var closeStatus = WebSocketCloseStatus.Empty;
        try
        {
            var message = await ReceiveMessageAsync(webSocket);
            lobby = message.lobby;
            playerName = message.sender;

            if (string.IsNullOrEmpty(lobby) || !GameServerService.ValidStringRegex.IsMatch(lobby))
            {
                throw new ArgumentException($"Incorrent lobby name \"{lobby}\" received from web socket");
            }
            else if (string.IsNullOrEmpty(playerName) || !GameServerService.ValidStringRegex.IsMatch(playerName))
            {
                throw new ArgumentException($"Incorrent player name \"{playerName}\" received from web socket");
            }

            var errorMessage = gameServer.AddPlayerToLobby(lobby, playerName, webSocket);

            if (!errorMessage.Equals(string.Empty))
            {
                throw new OperationCanceledException(errorMessage);
            }
            gameServer.SendPLayerStatusMessage(lobby);

            while (gameServer.PlayerExists(lobby, playerName))
            {
                var buffer = new byte[WebSocketMessageBufferSize];
                var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                if (result.CloseStatus.HasValue)
                {
                    Debug.WriteLine($"Player {playerName} disconnected from lobby {lobby}.");
                    break;
                }
                var receivedMessage = Strings.getString(buffer);
                dynamic msgObj = Strings.getObject(receivedMessage);
                var msg = new Message((string)msgObj.sender, (string)msgObj.lobby, (string)msgObj.type, msgObj.body);
                gameServer.HandleLobbyMessage(lobby, msg);
            }
        }
        catch (Exception exception)
        {
            // TODO: Handle
            Console.WriteLine(exception);
        }
        finally
        {
            gameServer.RemovePlayer(lobby, playerName);

            if (webSocket.State != WebSocketState.Closed)
            {
                CloseSocketAsync(webSocket, closeStatus);
            }
        }
    }

    static async void CloseSocketAsync(WebSocket webSocket, WebSocketCloseStatus status)
    {
        try
        {
            await webSocket.CloseAsync(status, null, CancellationToken.None);
        }
        catch (Exception exception)
        {
            // TODO: Handle
            Console.WriteLine(exception);
        }
    }

    async Task<Message> ReceiveMessageAsync(WebSocket webSocket)
    {
        var buffer = new byte[WebSocketMessageBufferSize];
        _ = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

        var text = Strings.getString(buffer);
        return Message.Deserialize(text);
    }
}
