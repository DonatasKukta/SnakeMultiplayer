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
        private ConcurrentDictionary<string, Snake> players = new ConcurrentDictionary<string, Snake>();
        private string HostPlayer;
        private int maxPlayers;
        private readonly DateTime creationTime;
        private Arena arena;

        public LobbyService(string id, string host, int maxPlayers)
        {
            this.ID = id;
            this.HostPlayer = host;
            this.state = LobbyStates.Idle;
            this.maxPlayers = maxPlayers;
            this.creationTime = DateTime.Now;
        }

        public string AddPlayer(string playerName, WebSocket socket)
        {
            if (isLobbyFull())
                return "Lobby is full.";
            else if (playerExists(playerName))
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
        
        private async void ReceiveMessageAsync(Message message)
        {
            throw new NotImplementedException();
        }

        private async void SendMessage(Message message )
        {
            throw new NotImplementedException();
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

        public bool isLobbyFull()
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
