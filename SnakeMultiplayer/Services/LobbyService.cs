using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using JsonLibrary;
using JsonLibrary.FromClient;
using JsonLibrary.FromServer;

namespace SnakeMultiplayer.Services;

public interface ILobbyService
{
    LobbyStates State { get; }
    Speed Speed { get; }
    string ID { get; }

    int GetPlayerCount();
    bool IsLobbyFull();
    bool IsActive();
    List<Player> GetAllPlayerStatus();
    void EndGame();
    ArenaStatus UpdateLobbyState();
    string AddPlayer(string playerName);
    void OnPlayerUpdate(Message message);
    Settings SetSettings(Settings settings);
    ArenaStatus InitiateGameStart(Message message);
}

public class LobbyService : ILobbyService
{
    public string ID { get; private set; }
    public LobbyStates State { get; private set; }
    public bool IsTimer { get; private set; }
    public Speed Speed { get => Arena.Speed; }

    readonly ConcurrentDictionary<string, Snake> players = new();

    readonly Arena Arena;
    readonly int MaxPlayers;
    readonly string HostPlayer;

    public LobbyService(string id, string host, int maxPlayers)
    {
        ID = id;
        HostPlayer = host;
        State = LobbyStates.Idle;
        MaxPlayers = maxPlayers;
        Arena = new Arena(players);
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

    public void EndGame()
    {
        State = LobbyStates.Idle;
    }

    private string InitializeGame()
    {
        State = LobbyStates.Initialized;
        var error = Arena.PrepareForNewGame();
        return error;
    }

    public ArenaStatus UpdateLobbyState()
    {
        Arena.UpdateActions();

        if (!IsGameEnd())
            return Arena.GenerateReport();

        State = LobbyStates.Idle;
        return null;
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

    public List<Player> GetAllPlayerStatus()
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

    public ArenaStatus InitiateGameStart(Message message)
    {
        if (message.sender.Equals(HostPlayer) && State.Equals(LobbyStates.Idle))
        {
            Debug.WriteLine($"Game initialised in {ID} lobby.");
            _ = InitializeGame();
            State = LobbyStates.inGame;
            return Arena.GenerateReport();
        }

        throw new Exception($"Incorrect game start message: {Message.Serialize(message)}");
    }

    public void OnPlayerUpdate(Message message)
    {
        if (!State.Equals(LobbyStates.inGame))
        {
            return;
        }

        var direction = (MoveDirection)message.body.GetInt32();
        Arena.SetPendingAction(message.sender, direction);
    }

    public Settings SetSettings(Settings settings) => Arena.SetSettings(settings);

    public List<Player> RemovePlayer(string playerName)
    {
        if (playerName == null)
        {
            throw new ArgumentNullException(nameof(playerName), "Attempt to remove player with null string.");
        }
        // TODO: Move up
        //if (playerName == HostPlayer)
        //{
        //    GameServer.RemoveLobby(ID);
        //}

        _ = Arena.ClearSnake(playerName);
        _ = players.TryRemove(playerName, out _);
        return GetAllPlayerStatus();

        //TODO: Move up
        //ServerHub.SendLobbyMessage(ID, new Message("server", ID, "Players", new { players = status, removed = playerName }));
    }

    public bool PlayerExists(string playerName) =>
        playerName == null
            ? throw new ArgumentNullException(nameof(playerName))
        : players.ContainsKey(playerName);

    public bool IsLobbyFull() => MaxPlayers <= players.Count;

    //TODO: Implement 
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
