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
        public readonly string ID;
        public LobbyStates State { get; private set; }
        private ConcurrentDictionary<string, Snake> players = new ConcurrentDictionary<string, Snake>();
        private readonly string hostPlayer;
        private readonly int maxPlayers;
        private readonly DateTime creationTime;
        private readonly GameServerService gameServer;
        private Arena arena;

        public LobbyService(string id, string host, int maxPlayers,[FromServices] GameServerService gameServer)
        {
            this.ID = id;
            this.hostPlayer = host;
            this.State = LobbyStates.Idle;
            this.maxPlayers = maxPlayers;
            this.creationTime = DateTime.Now;
            this.gameServer = gameServer;
        }

        public string AddPlayer(string playerName, WebSocket socket)
        {
            if (IsLobbyFull())
                return "Lobby is full.";
            else if (PlayerExists(playerName))
                return $"Player {playerName} already exists in lobby";
            else
            {
                if (players.TryAdd(playerName, new Snake(new Coordinate(1,1))))
                {
                    //SendMessage(socket, "Sekmingai pridetas i lobby", WebSocketMessageType.Text, true);
                    return string.Empty;
                }
                else
                    return "An error has occured. Please try again later.";
            }
        }
        
        public async void ReceiveMessageAsync(Message message)
        {
            //Echoing
            SendLobbyMessage(message);
        }

        private async void SendLobbyMessage(Message message)
        {
            gameServer.SendLobbyMessage(this.ID, message);
        }

        private async void SendPlayerMessage(Message message )
        {
            throw new NotImplementedException();
        }

        private bool PlayerExists(string playerName)
        {
            foreach(var player in players)
            {
                if (player.Key.Equals(playerName))
                    return false;
            }
            return false;
        }

        public bool IsLobbyFull()
        {
            return maxPlayers <= players.Count ? true : false;
        }

        public bool IsActive()
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
