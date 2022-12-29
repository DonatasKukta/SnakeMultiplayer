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

    public string AddPlayer(string playerName) =>
        IsLobbyFull()
            ? "Lobby is full."
        : PlayerExists(playerName)
            ? $"Player {playerName} already exists in lobby"
        : players.TryAdd(playerName, new Snake(GetValidPlayerColor()))
            ? string.Empty
        : "An error has occured. Please try again later.";

    private void EndGame()
    {
        State = LobbyStates.Idle;
        timer.Stop();
    }

    private string InitializeGame()
    {
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
        arena.UpdateActions();

        if (IsGameEnd())
        {
            SendLobbyMessage(new Message("server", ID, "End", null));
            if (timer != null)
            {
                timer.Stop();
                timer.Dispose();
            }
            State = LobbyStates.Idle;
            return;
        }

        SendLobbyMessage(new Message("server", ID, "Update", new { status = arena.GenerateReport() }));
    }

    private bool IsGameEnd()
    {
        var playerCount = players.Count();
        if (1 < playerCount)
        {
            var activeCount = players.Values.Select(p => p.IsActive).Where(IsActive => IsActive == true).Count();
            return activeCount <= 1;
        }
        else if (playerCount == 1)
        {
            var activeCount = players.Values.Select(p => p.IsActive).Where(IsActive => IsActive == true).Count();
            return activeCount <= 0;
        }
        else
        {
            gameServer.RemoveLobby(ID);
            return true;
        }
    }

    public int GetPlayerCount() => players.Count();
    public void SendCloseLobbyMessage(string reason) => SendLobbyMessage(new Message("server", ID, "Exit", new { message = reason }));
    public void SendPLayerStatusMessage()
    {
        var playersStatus = CreatePlayerStatusMessage();
        SendLobbyMessage(playersStatus);
    }

    private Message CreatePlayerStatusMessage() =>
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
                        Debug.WriteLine($"Game initialised in {ID} lobby.");
                        var inicializationError = InitializeGame();

                        var report = arena.GenerateReport();
                        SendLobbyMessage(new Message("server", ID, "Start", new { Start = report }));
                        Task.Delay(2000).Wait();

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
                        var settings = Settings.Deserialize(message.body);
                        settings = arena.SetSettings(settings);
                        SendLobbyMessage(new Message("server", ID, "Settings", new { Settings = settings }));
                    }
                    break;
                case "Update":
                    if (!State.Equals(LobbyStates.inGame))
                    {
                        break;
                    }

                    var direction = (MoveDirection)message.body;
                    arena.SetPendingAction(message.sender, direction);

                    if (!IsTimer)
                    {
                        OnTimedUpdate(null, null);
                    }

                    break;
                default:
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

    //TODO: Implement; take into account time of existance. // => players.Count > 0; 
    public bool IsActive() => true;

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
