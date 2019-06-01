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
using System.Diagnostics;

namespace SnakeMultiplayer.Services
{
    public class LobbyService
    {
        public readonly string ID;
        public LobbyStates State { get; private set; }
        private ConcurrentDictionary<string, Snake> players = new ConcurrentDictionary<string, Snake>(); // make read only?

        private readonly string hostPlayer;
        private readonly int maxPlayers;
        private readonly DateTime creationTime;
        private readonly GameServerService gameServer;
        private Arena arena;

        public LobbyService(string id, string host, int maxPlayers, [FromServices] GameServerService gameServer)
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
                if (players.TryAdd(playerName, new Snake(null, getValidPlayerColor())))
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

        private void InitializeGame()
        {
            // Create snakes with positions and colors

        }

        public void SendPLayerStatusMessage()
        {
            Message playersStatus = CreatePlayerStatusMessage();
            SendLobbyMessage(playersStatus);
        }

        private Message CreatePlayerStatusMessage()
        {
            //string players = Players.Serialize(getallPlayerStatus());

            return new Message("server", this.ID, "Players", new { players = getallPlayerStatus() });
        }

        private List<Player> getallPlayerStatus()
        {
            List<Player> list = new List<Player>(players.Count);
            foreach (var player in players)
            {
                Player newPlayer = new Player();
                newPlayer.name = player.Key;
                newPlayer.color = player.Value.GetColorString();
                newPlayer.type = newPlayer.name.Equals(hostPlayer) ? "Host" : "Player";
                list.Add(newPlayer);
            }
            return list;
        }

        public void HandleMessage(Message message)
        {
            try
            {
                switch (message.type)
                {
                    case "Start":
                        if (message.sender.Equals(hostPlayer))
                        {
                        Debug.WriteLine($"Inicializuotas žaidimas {ID} lobby  ");
                        InitializeGame();
                        }
                    break;
                    case "Players":
                        SendPLayerStatusMessage();
                        break;
                    case "Update":
                        SendLobbyMessage(new Message("server", this.ID, "Update",
                            new { messageUpdate = "Zinute gauta. Veiksmas ale uzfiksuotas" }));
                        break;
                    default: //echo
                        SendLobbyMessage(message);
                        break;
                }
            }
            catch (Exception ex)
            {

            }
        }

        private async void SendLobbyMessage(Message message)
        {
            gameServer.SendLobbyMessage(this.ID, message);
        }

        private async void SendPlayerMessage(Message message)
        {
            throw new NotImplementedException();
        }

        public bool RemovePlayer(string playerName)
        {
            return players.TryRemove(playerName, out Snake value);
        }

        private bool PlayerExists(string playerName)
        {
            foreach (var player in players)
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

        private PlayerColor getValidPlayerColor()
        {
            var players = this.players.Values.ToList();
            var takenColors = players.Select(p => p.color).ToList();
            var allColors = Enum.GetValues(typeof(PlayerColor)).Cast<PlayerColor>().ToList();

            foreach(var color in allColors)
            {
                if (!takenColors.Contains(color))
                    return color;
            }

            throw new InvalidOperationException("Cannot find unused player color, because all are used.");
        }
    }

    public enum PlayerColor
    {
        greenyellow,
        dodgerblue,
        orange,
        mediumpurple,
    }

    public enum InitialPosition
    {
        UpLeft,
        UpRight, 
        DownLeft, 
        DownRight,
    }

    public enum LobbyStates
    {
        Idle,
        inGame,
        closed,
    }
}
