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
    public class LobbyService
    {
        public string ID {  get; private set; }
        public LobbyStates state { get; private set; }
        private ConcurrentDictionary<string, WebSocket> players = new ConcurrentDictionary<string, WebSocket>();
        private string HostPlayer;
        private int maxPlayers;
        private DateTime creationTime;
        private Arena arena;

        public LobbyService(string id, string host, int maxPlayers)
        {
            this.ID = id;
            this.HostPlayer = host;
            this.state = LobbyStates.Idle;
            this.maxPlayers = maxPlayers;
        }

        public string AddPlayer(string playerName, WebSocket socket)
        {
            if (isLobbyFull())
                return "Lobby is full.";
            else if (playerExists(playerName))
                return $"Player {playerName} already exists in lobby";
            else
            {
                if (players.TryAdd(playerName, socket))
                {
                    //ReceiveMessageAsync(socket);
                    SendMessage(socket, "Sekmingai pridetas i lobby", WebSocketMessageType.Text, true);
                    return string.Empty;
                }
                else
                    return "An error has occured";
            }
        }
        
        private async void ReceiveMessageAsync(WebSocket webSocket)
        {
            var buffer = new byte[1024 * 4];
            WebSocketReceiveResult result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            while (!result.CloseStatus.HasValue)
            {
                //await webSocket.SendAsync(new ArraySegment<byte>(buffer, 0, result.Count), result.MessageType, result.EndOfMessage, CancellationToken.None);
                string receivedMessage = Strings.getString(buffer);
                SendMessage(webSocket, receivedMessage, result.MessageType, result.EndOfMessage);

                result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            }
            await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
        }

        private async void SendMessage(WebSocket webSocket,string message, WebSocketMessageType mtype, bool endOfMessage)
        {
            byte[] buffer = Strings.getBytes(message);
            await webSocket.SendAsync(new ArraySegment<byte>(buffer), mtype, endOfMessage, CancellationToken.None);
        }

        private bool playerExists(string playerName)
        {
            foreach(var player in players)
            {
                if (player.Key.Equals(playerName))
                    return false;
            }
            return false;
        }

        private bool isLobbyFull()
        {
            return maxPlayers <= players.Count ? true : false;
        }

        public bool isActive()
        {
            return players.Count > 0 ? true : false;
        }

    }

    public enum LobbyStates
    {
        Idle,
        inGame,
        closed,
    }
}
