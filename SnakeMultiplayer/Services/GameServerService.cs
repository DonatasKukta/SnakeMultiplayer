using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using JsonLibrary;
using Microsoft.AspNetCore.Mvc;

namespace SnakeMultiplayer.Services
{
    /// <summary>
    /// Gives abstraction layer to web socket based communication.
    /// </summary>
    public class GameServerService : IHostedService
    {
        /// <summary>
        /// Represents lobbies and its players, with which server is communicating
        /// </summary>
        ConcurrentDictionary<string, Lobby> lobbies = new ConcurrentDictionary<string, Lobby>();

        /// <summary>
        /// Represents the buffer length, in which received messages from web sockets will be stored.
        /// </summary>
        private  readonly int bufferSize = 1024 * 4; // Get this value from app settings
        private readonly int maxPlayers = 4;

        public Task StartAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public bool AddLobby(string hostName, string lobbyName)
        {
            return lobbies.TryAdd(lobbyName, new Lobby(lobbyName, hostName, maxPlayers));
        }

        private bool addPlayerToLobby(string lobby, string player, WebSocket socket)
        {
            try
            {
                return lobbies[lobby].AddPlayer(player, socket);
            }
            catch (KeyNotFoundException ex)
            {
                throw (ex);
            }
        }

        public async Task HandleWebSocketAsync(WebSocket webSocket, [FromServices] GameServerService gameServer)
        {
            var closeStatus = WebSocketCloseStatus.Empty;
            try
            {
                Message message = await ReceiveMessageAsync(webSocket);
                string lobby = message.lobby;
                string playerName = message.sender;

                if (!addPlayerToLobby(lobby, playerName, webSocket))
                    throw new OperationCanceledException($"Could not add player {playerName} to {lobby} lobby.");
                // While current player is in lobby

                SendMessageAsync(webSocket, new Message("Server", "None", "Join", "Prisjungti pavyko"));
                while (lobbies[lobby].PlayerExists(playerName))
                {
                    //message = await ReceiveMessageAsync(webSocket);
                    byte[] buffer = new byte[bufferSize];
                    WebSocketReceiveResult result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                    message = Message.Deserialize(Strings.getString(buffer));

                    // Echoing:
                    SendMessageAsync(webSocket, message);
                }
            }
            catch(OperationCanceledException e)
            {
                // Unexpected error! try again pls
                // log exception
            }
            catch(Exception e)
            {

            }
            finally
            {
                // remove player from this.lobbies
                // remove player from gameserverservice.lobbies
                // close socket
                CloseSocketAsync(webSocket, closeStatus);
            }
        }

        private async void CloseSocketAsync(WebSocket webSocket,  WebSocketCloseStatus status)
        {
            await webSocket.CloseAsync(status, null,  CancellationToken.None);
        }

        private async void SendMessageAsync(WebSocket webSocket, Message message)
        {
            var buffer = Strings.getBytes(Message.Serialize(message));
            await webSocket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, CancellationToken.None);
        }

        private async Task<Message> ReceiveMessageAsync(WebSocket webSocket)
        {
            byte[] buffer = new byte[bufferSize];
            WebSocketReceiveResult result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            return Message.Deserialize(Strings.getString(buffer));
        }

        public bool TryCreateLobby(string lobbyName, string hostPlayerName)
        {
            return lobbies.TryAdd(lobbyName, new Lobby(lobbyName, hostPlayerName, maxPlayers));
        }

        public bool LobbyExists(string lobbyName)
        {
            return lobbies.ContainsKey(lobbyName);
        }

        public bool playerExists(string lobbyName, string playerName)
        {
            Lobby current;
            if (lobbies.TryGetValue(lobbyName,out current))
            {
                return current.PlayerExists(playerName);
            }
            else
            {
                throw new EntryPointNotFoundException($"Lobby {lobbyName} does not exists");
            }
        }

        private class Lobby
        {
            public ConcurrentDictionary<string, WebSocket> players { get; private set; }
            public LobbyService lobby { get; private set; }

            public Lobby(string name, string hostName, int maxPlayers)
            {
                this.players = new ConcurrentDictionary<string, WebSocket>();
                this.lobby = new LobbyService(name, hostName, maxPlayers);
            }

            public bool AddPlayer(string playerName, WebSocket webSocket)
            {
                if (playerName == null)
                    throw new ArgumentNullException("Attempt to add player with null string.");
                else if (webSocket == null)
                    throw new ArgumentNullException($"Attempt to add player {playerName} with null WebSocket.");

                return players.TryAdd(playerName, webSocket);
            }

            public bool PlayerExists(string playerName)
            {
                if (playerName == null)
                    throw new ArgumentNullException("Attempt to check existance of player with null string.");

                return players.ContainsKey(playerName);
            }

            public bool removePlayer(string player)
            {
                if (player == null)
                    throw new ArgumentNullException("Attempt to remove player with null string.");

                WebSocket @null;
                return players.TryRemove(player,out @null);
            }
        }
        /*
        public async void Forward(WebSocket webSocket, Message message)
        {
            if (lobbyExists(message.lobby) && message.type.Equals("join"))
            {
                string result = lobbies[message.lobby].AddPlayer(message.sender, webSocket);
                if (!result.Equals(string.Empty))
                {
                    await webSocket.CloseAsync(WebSocketCloseStatus.PolicyViolation, result, CancellationToken.None);
                }
            }
            else if (!message.type.Equals("join"))
            {
                await webSocket.CloseAsync(WebSocketCloseStatus.ProtocolError, "Unexpected message type.", CancellationToken.None);
            }
            else
            {
                await webSocket.CloseAsync(WebSocketCloseStatus.ProtocolError, "Unexpected protocol behaviour.", CancellationToken.None);
            }
        }*/
    }
}

