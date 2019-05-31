using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using JsonLibrary;
using Microsoft.AspNetCore.Mvc;
using JsonLibrary;


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

        public string AddPlayer(string playerName)
        {
            if (IsLobbyFull())
                return "Lobby is full.";
            else if (PlayerExists(playerName))
                return $"Player {playerName} already exists in lobby";
            else
            {
                if (players.TryAdd(playerName, new Snake(new Coordinate(1,1), "black")))
                {
                    // Update lobby status to all lobby players.
                    //Message playersStatus = CreatePlayerStatusMessage();
                    //SendLobbyMessage(playersStatus);
                    return string.Empty;
                }
                else
                    return "An error has occured. Please try again later.";
            }
        }

        public void SendPLayerStatusMessage()
        {
            Message playersStatus = CreatePlayerStatusMessage();
            SendLobbyMessage(playersStatus);
        }

        private Message CreatePlayerStatusMessage()
        {
            string players = Players.Serialize(getallPlayerStatus());
            return new Message("server", this.ID, "Players", players);
        }

        private Players getallPlayerStatus()
        {
            List<Player> list = new List<Player>(players.Count);
            foreach(var player in players)
            {
                Player newPlayer = new Player();
                newPlayer.name = player.Key;
                newPlayer.color = player.Value.color;
                list.Add(newPlayer);
            }
            return new Players(list);
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

        public bool RemovePlayer(string playerName)
        {
            return players.TryRemove(playerName, out Snake value);
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
            return true;
            // Take into account time of existance.
            //return players.Count > 0 ? true : false; 
        }
    }

    public enum LobbyStates
    {
        Idle,
        inGame,
        closed,
    }
}
