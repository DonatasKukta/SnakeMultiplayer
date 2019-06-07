using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JsonLibrary;
using Microsoft.AspNetCore.Mvc;
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
        private System.Timers.Timer timer;
        public bool IsTimer { get; private set; }

        public LobbyService(string id, string host, int maxPlayers, [FromServices] GameServerService gameServer)
        {
            this.ID = id;
            this.hostPlayer = host;
            this.State = LobbyStates.Idle;
            this.maxPlayers = maxPlayers;
            this.creationTime = DateTime.Now;
            this.gameServer = gameServer;
            this.arena = new Arena(players);
            timer = null;
            IsTimer = false;
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
            State = LobbyStates.Initialized;

            if (arena.Speed.Equals(Speed.NoSpeed))
            {
                IsTimer = false;
                timer = null;
            }
            else
                IsTimer = true;

            string error = arena.PrepareForNewGame();
            return error;
        }

        private void StartTimer()
        {
            IsTimer = true;
            timer = new System.Timers.Timer();
            timer.Interval = 70 * (int)arena.Speed;
            timer.Elapsed += new System.Timers.ElapsedEventHandler(OnTimedUpdate);
            timer.AutoReset = true;
            timer.Start();
        }

        private void OnTimedUpdate(object source, System.Timers.ElapsedEventArgs e)
        {

            // update new status
            arena.UpdateActions();

            bool isEnd = IsGameEnd();
            // Chech if game has ended
            if (isEnd)
            {
                SendLobbyMessage(new Message("server", this.ID, "End", null));
                if (timer != null)
                {
                    this.timer.Stop();
                    this.timer.Dispose();
                }
                    this.State = LobbyStates.Idle;
            }
            else
            {
                // generate new arena status
                ArenaStatus report = arena.GenerateReport();
                // send lobby message
                SendLobbyMessage(new Message("server", this.ID, "Update", new { status = report }));
            }
        }

        private bool IsGameEnd()
        {
            var playerCount = this.players.Count();
            // If multiple player, stop when one player is left active.
            if( 1 < playerCount )
            {
                var activeCount = players.Values.Select(p => p.IsActive).Where(IsActive => IsActive == true).Count();
                return activeCount > 1 ? false : true;
            }
            // if solo player, end only if snake is deactivated.
            else if ( playerCount == 1 )
            {
                //var tmp = players.Values.Select(p => p.IsActive == true).Count();
                var activeCount = players.Values.Select(p => p.IsActive).Where(IsActive => IsActive == true).Count();
                return activeCount > 0 ? false : true;
            }
            // if no players, destroy lobby.
            else 
            {
                gameServer.removeLobby(this.ID);
                return true;
            }
            
        }

        public void sendCloseLobbyMessage(string reason)
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
                        if (message.sender.Equals(hostPlayer) && State.Equals(LobbyStates.Idle))
                        {
                            Debug.WriteLine($"Inicializuotas žaidimas {ID} lobby  ");
                            string inicializationError = InitializeGame();
                            
                            var report = arena.GenerateReport();
                            SendLobbyMessage(new Message("server", this.ID, "Start", new {Start = report }));
                            Task.Delay(2000).Wait();
                            // If is timer, delay for 2 seconds and then start updating the positions
                            if (IsTimer)
                                StartTimer();

                            State = LobbyStates.inGame;
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
                            settings = this.arena.SetSettings(settings);
                            SendLobbyMessage(new Message("server", this.ID,"Settings" ,new {Settings = settings }));
                        }
                        break;
                    case "Update": // updating only while in game
                        if (!State.Equals(LobbyStates.inGame))
                            break;
                        var direction = (MoveDirection) message.body;
                        arena.SetPendingAction(message.sender, direction);

                        // if speed is set to no speed, update on each player movement.
                        if (!IsTimer)
                            OnTimedUpdate(null, null);

                        break;
                    default: //echo
                        Debug.WriteLine($"---Unexpected message from {message.sender}, content: {message.body.ToString()}");
                        break;
                }
            }
            catch
            {
                throw;
            }
        }

        private void SendLobbyMessage(Message message)
        {
            gameServer.SendLobbyMessage(this.ID, message);
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
        Initialized,
        inGame,
        closed,
    }
}
