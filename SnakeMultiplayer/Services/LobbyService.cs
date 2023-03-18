using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;

using JsonLibrary;
using JsonLibrary.FromClient;
using JsonLibrary.FromServer;

namespace SnakeMultiplayer.Services;

public interface ILobbyService
{
    bool IsTimer { get; }
    LobbyStates State { get; }

    string AddPlayer(string playerName);
    Message CreatePlayerStatusMessage();
    int GetPlayerCount();
    void HandleMessage(Message message);
    bool IsActive();
    bool IsLobbyFull();
    void RemovePlayer(string playerName);
    void SendCloseLobbyMessage(string reason);
    void SendPLayerStatusMessage();
    Settings SetSettings(Settings settings);
}

public class LobbyService : ILobbyService
{
    public readonly string ID;
    public LobbyStates State { get; private set; }
    public bool IsTimer { get; private set; }

    readonly ConcurrentDictionary<string, Snake> players = new();

    readonly string HostPlayer;
    readonly int MaxPlayers;
    readonly DateTime CreationTime;
    readonly IGameServerService GameServer;
    readonly IServerHub ServerHub;
    readonly Arena Arena;
    Timer Timer;

    public LobbyService(string id, string host, int maxPlayers, IGameServerService gameServer)
    {
        ID = id;
        HostPlayer = host;
        State = LobbyStates.Idle;
        MaxPlayers = maxPlayers;
        CreationTime = DateTime.Now;
        GameServer = gameServer;
        Arena = new Arena(players);
        Timer = null;
        IsTimer = false;
    }

    public string AddPlayer(string playerName)
    {
        var reason = CanJoin(playerName);
        if (!string.IsNullOrWhiteSpace(reason))
        {
            return reason;
        }

        if (!players.TryAdd(playerName, new Snake(GetValidPlayerColor())))
        {
            return "An error has occured. Please try again later.";
        }

        return string.Empty;
    }

    public string CanJoin(string playerName) =>
        string.IsNullOrWhiteSpace(playerName)
            ? "Empty (null) player name."
        : !IsActive()
            ? $"Lobby {ID} is not active. Please join another lobby"
        : IsLobbyFull()
            ? "Lobby is full."
        : PlayerExists(playerName)
            ? $"Player {playerName} already exists in lobby"
        : string.Empty;

    private void EndGame()
    {
        State = LobbyStates.Idle;
        Timer.Stop();
    }

    private string InitializeGame()
    {
        State = LobbyStates.Initialized;

        if (Arena.Speed.Equals(Speed.NoSpeed))
        {
            IsTimer = false;
            Timer = null;
        }
        else
        {
            IsTimer = true;
        }

        var error = Arena.PrepareForNewGame();
        return error;
    }

    private void StartTimer()
    {
        IsTimer = true;
        Timer = new Timer
        {
            Interval = 70 * (int)Arena.Speed
        };
        Timer.Elapsed += new ElapsedEventHandler(OnTimedUpdate);
        Timer.AutoReset = true;
        Timer.Start();
    }

    private void OnTimedUpdate(object source, ElapsedEventArgs e)
    {
        Arena.UpdateActions();

        if (!IsGameEnd())
        {
            ServerHub.SendArenaStatusUpdate(ID, Arena.GenerateReport());
            return;
        }

        ServerHub.EndGame(ID);
        State = LobbyStates.Idle;
        if (Timer != null)
        {
            Timer.Stop();
            Timer.Dispose();
        }
    }

    private bool IsGameEnd()
    {
        var activePlayers = players.Values.Where(player => player.IsActive);
        var playerCount = players.Count;

        if (1 < playerCount)
        {
            return activePlayers.Count() <= 1;
        }
        else if (playerCount == 1)
        {
            return !activePlayers.Any();
        }

        return true;
    }

    public int GetPlayerCount() => players.Count;

    public void SendCloseLobbyMessage(string reason) =>
        ServerHub.ExitGame(ID, reason);


    public void SendPLayerStatusMessage()
    {
        var playersStatus = CreatePlayerStatusMessage();
        ServerHub.SendPlayerStatusUpdate(ID, playersStatus);
    }

    public Message CreatePlayerStatusMessage() =>
        new("server", ID, "Players", new { players = GetAllPlayerStatus() });

    private List<Player> GetAllPlayerStatus()
    {
        var list = new List<Player>(players.Count);
        foreach (var player in players)
        {
            var newPlayer = new Player
            {
                name = player.Key,
                color = player.Value.GetColorString()
            };
            newPlayer.type = newPlayer.name.Equals(HostPlayer) ? "Host" : "Player";
            list.Add(newPlayer);
        }
        return list;
    }

    public void HandleMessage(Message message)
    {
        switch (message.type)
        {
            case "Start":
                if (message.sender.Equals(HostPlayer) && State.Equals(LobbyStates.Idle))
                {
                    Debug.WriteLine($"Game initialised in {ID} lobby.");
                    _ = InitializeGame();

                    var report = Arena.GenerateReport();
                    ServerHub.InitiateGameStart(ID, report);
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
            case "Update":
                if (!State.Equals(LobbyStates.inGame))
                {
                    break;
                }

                var direction = (MoveDirection)message.body;
                Arena.SetPendingAction(message.sender, direction);

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

    public Settings SetSettings(Settings settings) => Arena.SetSettings(settings);

    public void RemovePlayer(string playerName)
    {
        if (playerName == null)
        {
            throw new ArgumentNullException(nameof(playerName), "Attempt to remove player with null string.");
        }

        if (playerName == HostPlayer)
        {
            GameServer.RemoveLobby(ID);
        }

        _ = Arena.ClearSnake(playerName);
        _ = players.TryRemove(playerName, out _);
        var status = GetAllPlayerStatus();
        ServerHub.SendLobbyMessage(ID, new Message("server", ID, "Players", new { players = status, removed = playerName }));
    }

    public bool PlayerExists(string playerName) =>
        playerName == null
            ? throw new ArgumentNullException(nameof(playerName))
        : players.ContainsKey(playerName);

    public bool IsLobbyFull() => MaxPlayers <= players.Count;

    //TODO: Implement 
    public bool IsActive() => players.Count > 0;

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
