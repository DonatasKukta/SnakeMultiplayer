using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

using JsonLibrary;
using JsonLibrary.FromClient;
using JsonLibrary.FromServer;

using Microsoft.AspNetCore.Mvc;

namespace SnakeMultiplayer.Services;

public class LobbyService
{
    public readonly string ID;
    public LobbyStates State { get; private set; }
    private readonly ConcurrentDictionary<string, Snake> players = new(); // make read only?

    private readonly string hostPlayer;
    private readonly int maxPlayers;
    private readonly DateTime creationTime;
    private readonly GameServerService gameServer;
    private readonly Arena arena;
    private System.Timers.Timer timer;
    public bool IsTimer { get; private set; }

    public LobbyService(string id, string host, int maxPlayers, [FromServices] GameServerService gameServer)
    {
        ID = id;
        hostPlayer = host;
        State = LobbyStates.Idle;
        this.maxPlayers = maxPlayers;
        creationTime = DateTime.Now;
        this.gameServer = gameServer;
        arena = new Arena(players);
        timer = null;
        IsTimer = false;
    }

    public int getPlayerCount() => players.Count();

    public string AddPlayer(string playerName) => IsLobbyFull()
            ? "Lobby is full."
            : PlayerExists(playerName)
                ? $"Player {playerName} already exists in lobby"
                : players.TryAdd(playerName, new Snake(GetValidPlayerColor())) ? string.Empty : "An error has occured. Please try again later.";

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
        {
            IsTimer = true;
        }

        var error = arena.PrepareForNewGame();
        return error;
    }

    private void StartTimer()
    {
        IsTimer = true;
        timer = new System.Timers.Timer
        {
            Interval = 70 * (int)arena.Speed
        };
        timer.Elapsed += new System.Timers.ElapsedEventHandler(OnTimedUpdate);
        timer.AutoReset = true;
        timer.Start();
    }

    private void OnTimedUpdate(object source, System.Timers.ElapsedEventArgs e)
    {

        // update new status
        arena.UpdateActions();

        var isEnd = IsGameEnd();
        // Chech if game has ended
        if (isEnd)
        {
            SendLobbyMessage(new Message("server", ID, "End", null));
            if (timer != null)
            {
                timer.Stop();
                timer.Dispose();
            }
            State = LobbyStates.Idle;
        }
        else
        {
            // generate new arena status
            var report = arena.GenerateReport();
            // send lobby message
            SendLobbyMessage(new Message("server", ID, "Update", new { status = report }));
        }
    }

    private bool IsGameEnd()
    {
        var playerCount = players.Count();
        // If multiple player, stop when one player is left active.
        if (1 < playerCount)
        {
            var activeCount = players.Values.Select(p => p.IsActive).Where(IsActive => IsActive == true).Count();
            return activeCount <= 1;
        }
        // if solo player, end only if snake is deactivated.
        else if (playerCount == 1)
        {
            //var tmp = players.Values.Select(p => p.IsActive == true).Count();
            var activeCount = players.Values.Select(p => p.IsActive).Where(IsActive => IsActive == true).Count();
            return activeCount <= 0;
        }
        // if no players, destroy lobby.
        else
        {
            gameServer.RemoveLobby(ID);
            return true;
        }
    }

    public void SendCloseLobbyMessage(string reason) => SendLobbyMessage(new Message("server", ID, "Exit", new { message = reason }));
    public void SendPLayerStatusMessage()
    {
        var playersStatus = CreatePlayerStatusMessage();
        SendLobbyMessage(playersStatus);
    }

    private Message CreatePlayerStatusMessage() =>
        //string players = Players.Serialize(getallPlayerStatus());

        new("server", ID, "Players", new { players = GetallPlayerStatus() });

    private List<Player> GetallPlayerStatus()
    {
        var list = new List<Player>(players.Count);
        foreach (var player in players)
        {
            var newPlayer = new Player
            {
                name = player.Key,
                color = player.Value.GetColorString()
            };
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
                        var inicializationError = InitializeGame();

                        var report = arena.GenerateReport();
                        SendLobbyMessage(new Message("server", ID, "Start", new { Start = report }));
                        Task.Delay(2000).Wait();
                        // If is timer, delay for 2 seconds and then start updating the positions
                        if (IsTimer)
                        {
                            StartTimer();
                        }

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
                        settings = arena.SetSettings(settings);
                        SendLobbyMessage(new Message("server", ID, "Settings", new { Settings = settings }));
                    }
                    break;
                case "Update": // updating only while in game
                    if (!State.Equals(LobbyStates.inGame))
                    {
                        break;
                    }

                    var direction = (MoveDirection)message.body;
                    arena.SetPendingAction(message.sender, direction);

                    // if speed is set to no speed, update on each player movement.
                    if (!IsTimer)
                    {
                        OnTimedUpdate(null, null);
                    }

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

    private void SendLobbyMessage(Message message) => gameServer.SendLobbyMessage(ID, message);

    public void RemovePlayer(string playerName)
    {
        // If host player is being removed, disband whole party.
        if (playerName == hostPlayer)
        {
            gameServer.RemoveLobby(ID);
        }

        _ = arena.ClearSnake(playerName);
        _ = players.TryRemove(playerName, out _);
        var status = GetallPlayerStatus();
        SendLobbyMessage(new Message("server", ID, "Players", new { players = status, removed = playerName }));
    }

    private bool PlayerExists(string playerName)
    {
        foreach (var player in players)
        {
            if (player.Key.Equals(playerName))
            {
                return false;
            }
        }
        return false;
    }

    public bool IsLobbyFull() => maxPlayers <= players.Count;

    public bool IsActive() => true;// Take into account time of existance.//return players.Count > 0 ? true : false; 

    private PlayerColor GetValidPlayerColor()
    {
        var players = this.players.Values.ToList();
        var takenColors = players.Select(p => p.color).ToList();
        var allColors = Enum.GetValues(typeof(PlayerColor)).Cast<PlayerColor>().ToList();

        foreach (var color in allColors)
        {
            if (!takenColors.Contains(color))
            {
                return color;
            }
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