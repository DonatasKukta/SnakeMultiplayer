﻿using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using JsonLibrary;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace SnakeMultiplayer.Services
{
    /// <summary>
    /// Gives abstraction layer to web socket based communication:
    /// Distributes incoming messages to relevant lobbies and 
    /// forwads messages from lobbies to web sockets
    /// </summary>
    public class GameServerService : IHostedService
    {
        /// <summary>
        /// Represents lobbies and its players, with which server is communicating
        /// </summary>
        private ConcurrentDictionary<string, Lobby> lobbies = new ConcurrentDictionary<string, Lobby>();

        /// <summary>
        /// Represents the buffer length, in which received messages from web sockets will be stored.
        /// </summary>
        private readonly int bufferSize = 1024 * 4; // Get this value from app settings
        private readonly int maxPlayers = 4;

        public Task StartAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        private string AddPlayerToLobby(string lobby, string player, WebSocket socket)
        {
            if (socket == null)
                throw new ArgumentNullException("Tried to add null web socket to concurrent list");
            try
            {
                return lobbies[lobby].AddPlayer(player, socket);
            }
            catch (KeyNotFoundException ex)
            {
                throw ex;
            }
        }

        public void SendLobbyMessage(string lobby, Message message)
        {
            if (!lobbies.TryGetValue(lobby, out Lobby currLobby))
                return;
            var sockets = currLobby.GetPlayersWebSockets();
            foreach (WebSocket socket in sockets)
            {
                SendMessageAsync(socket, message);
            }
        }

        public async Task HandleWebSocketAsync(WebSocket webSocket)
        {
            var closeStatus = WebSocketCloseStatus.Empty;
            try
            {
                Message message = await ReceiveMessageAsync(webSocket);
                string lobby = message.lobby;
                string playerName = message.sender;
                string errorMessage = AddPlayerToLobby(lobby, playerName, webSocket);
                if (!errorMessage.Equals(string.Empty))
                    throw new OperationCanceledException("errorMessage");

                lobbies[lobby].LobbyService.SendPLayerStatusMessage();

                // While current player is in lobby
                while (lobbies[lobby].PlayerExists(playerName))
                {
                    //message = await ReceiveMessageAsync(webSocket);
                    byte[] buffer = new byte[bufferSize];
                    WebSocketReceiveResult result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                    string receivedMessage = Strings.getString(buffer);
                    dynamic msgObj = Strings.getObject(receivedMessage);
                    Message msg = new Message((string)msgObj.sender, (string)msgObj.lobby, (string) msgObj.type, msgObj.body);
                    lobbies[lobby].LobbyService.HandleMessage(msg);
                }
            }
            catch (OperationCanceledException e)
            {
                // Unexpected error! try again pls
                // log exception
                Console.WriteLine(e.Message);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            finally
            {
                // remove player from this.lobbies
                // remove player from gameserverservice.lobbies
                // close socket
                CloseSocketAsync(webSocket, closeStatus);
            }
        }

        private async void CloseSocketAsync(WebSocket webSocket, WebSocketCloseStatus status)
        {
            await webSocket.CloseAsync(status, null, CancellationToken.None);
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
            string text = Strings.getString(buffer);
            return Message.Deserialize(text);
        }

        public bool TryCreateLobby(string lobbyName, string hostPlayerName, GameServerService service)
        {
            return lobbies.TryAdd(lobbyName, new Lobby(lobbyName, hostPlayerName, maxPlayers, service));
        }

        public string CanJoin(string lobbyName, string playerName)
        {
            if (!lobbies.TryGetValue(lobbyName, out Lobby lobby))
                return $"Lobby {lobbyName} does not exist. Please try a different name";
            else if(!lobby.IsActive())
                return $"Lobby {lobbyName} is not active, therefore you cannot join it.";
            else if (lobby.IsFull())
                return $"Lobby {lobbyName} is full. Please try again later.";
            else if (lobby.PlayerExists(playerName))
                return $"Name {playerName} is already taken. Please use another name.";
            else
                return string.Empty;
        }

        public bool LobbyExists(string lobbyName)
        {
            return lobbies.ContainsKey(lobbyName);
        }

        public bool PlayerExists(string lobbyName, string playerName)
        {
            if (lobbies.TryGetValue(lobbyName, out Lobby current))
            {
                return current.PlayerExists(playerName);
            }
            else
            {
                throw new EntryPointNotFoundException($"Lobby {lobbyName} does not exists");
            }
        }

        // Implement locks on modification methods?
        private class Lobby
        {
            private ConcurrentDictionary<string, WebSocket> players;
            public LobbyService LobbyService { get; private set; }

            public Lobby(string name, string hostName, int maxPlayers, GameServerService gameServer)
            {
                this.players = new ConcurrentDictionary<string, WebSocket>();
                this.LobbyService = new LobbyService(name, hostName, maxPlayers, gameServer);
            }

            public string AddPlayer(string playerName, WebSocket webSocket)
            {
                if (playerName == null)
                    return "Attempt to add player with null string.";
                else if (webSocket == null)
                    return $"Attempt to add player {playerName} with null WebSocket.";
                else if (!IsActive())
                    return "Lobby {LobbyService.ID} is not active. Please join another lobby";
                else if (IsFull())
                    return $"Lobby {LobbyService.ID} is full. Please join another lobby.";
                string errorMessage = LobbyService.AddPlayer(playerName);
                if (!errorMessage.Equals(string.Empty))
                    return errorMessage;
                if(!players.TryAdd(playerName, webSocket))
                {
                    LobbyService.RemovePlayer(playerName);
                    // log
                    return $"Unexpected error while trying to join {LobbyService.ID}. Please try again later";
                }
                return string.Empty;
            }

            public WebSocket[] GetPlayersWebSockets()
            {
                return players.Values.ToArray();
            }

            public bool PlayerExists(string playerName)
            {
                if (playerName == null)
                    throw new ArgumentNullException("Attempt to check existance of player with null string.");

                return players.ContainsKey(playerName);
            }

            public bool RemovePlayer(string player)
            {
                if (player == null)
                    throw new ArgumentNullException("Attempt to remove player with null string.");

                return players.TryRemove(player, out WebSocket @null);
            }

            public bool IsFull()
            {
                return LobbyService.IsLobbyFull();
            }
            public bool IsActive()
            {
                return LobbyService.IsActive();
            }
        }
    }
}

