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
using System.Collections;

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
        private System.Timers.Timer timer;

        public LobbyService(string id, string host, int maxPlayers, [FromServices] GameServerService gameServer)
        {
            this.ID = id;
            this.hostPlayer = host;
            this.State = LobbyStates.Idle;
            this.maxPlayers = maxPlayers;
            this.creationTime = DateTime.Now;
            this.gameServer = gameServer;
            this.arena = new Arena(players);
        }

        public string AddPlayer(string playerName)
        {
            if (IsLobbyFull())
                return "Lobby is full.";
            else if (PlayerExists(playerName))
                return $"Player {playerName} already exists in lobby";
            else
            {
                if (players.TryAdd(playerName, new Snake(getValidPlayerColor())))
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

        private void EndGame()
        {
            State = LobbyStates.Idle;
            timer.Stop();
        }

        private string InitializeGame()
        {
            // Create snakes with positions and colors
            State = LobbyStates.inGame;
            string error = arena.PrepareForNewGame();
            if (!error.Equals(string.Empty))
                return error;

            //initialize timer
            timer = new System.Timers.Timer(1000);
            timer.Interval = 2000; // 1 second
            timer.Elapsed += new System.Timers.ElapsedEventHandler(OnTimedUpdate);
            timer.AutoReset = true;
            timer.Start();
            return string.Empty;
        }

        private async void OnTimedUpdate(object source, System.Timers.ElapsedEventArgs e)
        {
            // update new status
            arena.updateActions();
            // generate new arena status
            ArenaStatus report = arena.GenerateReport();
            // send lobby message

            //string temp = Strings.get
            //dynamic temp = report;

            SendLobbyMessage(new Message("server", this.ID, "Update", new { status = report }));
        }

        public async void sendCloseLobbyMessage(string reason)
        {
            SendLobbyMessage(new Message("server", this.ID, "Exit", new { message = reason}));
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
                    case "Settings":
                        if (message.sender.Equals(hostPlayer))
                        {
                            //Debug.WriteLine("Gauti settings:" + message.body.ToString());
                            var settings = Settings.Deserialize(message.body);
                            this.arena.SetSettings(settings);
                        }
                        break;
                    case "Update": // updating only while in game
                        if (!State.Equals(LobbyStates.inGame))
                            break;

                        var direction = (MoveDirection) message.body;
                        SetNewPendingAction(message.sender, direction);
                        break;
                    default: //echo
                        Debug.WriteLine($"---Unexpected message from {message.sender}, content: {message.body.ToString()}");
                        break;
                }
            }
            catch (Exception ex)
            {

            }
        }

        private void SetNewPendingAction(string player, MoveDirection direction)
        {
            arena.SetPendingAction(player, direction);
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
            // If host player is being removed, disband whole party.
            if (playerName == this.hostPlayer)
                this.gameServer.removeLobby(this.ID);

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

    public enum LobbyStates
    {
        Idle,
        inGame,
        closed,
    }
}
