using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using JsonLibrary.FromClient;
using JsonLibrary.FromServer;

namespace SnakeMultiplayer.Services;

public interface ILobbyService
{
    LobbyStates State { get; }
    Speed Speed { get; }
    string ID { get; }
    public bool IsNoSpeed { get; }

    int GetPlayerCount();
    bool IsLobbyFull();
    bool IsActive();
    bool IsHost(string playerName);
    List<Player> GetAllPlayerStatus();
    void EndGame();
    ArenaStatus UpdateLobbyState();
    string AddPlayer(string playerName);
    void OnPlayerUpdate(string player, MoveDirection message);
    Settings SetSettings(Settings settings);
    ArenaStatus InitiateGameStart();
    List<Player> RemovePlayer(string playerName);
}

public class LobbyService : ILobbyService
{
    public string ID { get; private set; }
    public LobbyStates State { get; private set; }
    public bool IsTimer { get; private set; }
    public Speed Speed { get => Arena.Speed; }
    public bool IsNoSpeed { get => Arena.Speed == Speed.NoSpeed; }

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

    public bool IsHost(string playerName) => playerName == HostPlayer;

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

    public ArenaStatus InitiateGameStart()
    {
        if (!State.Equals(LobbyStates.Idle))
            throw new Exception($"Tried to initialise game start while lobby {ID} is in Idle state.");

        State = LobbyStates.Initialized;
        _ = Arena.PrepareForNewGame();
        State = LobbyStates.inGame;

        Debug.WriteLine($"Game initialised in {ID} lobby.");
        return Arena.GenerateReport();
    }

    public void OnPlayerUpdate(string player, MoveDirection direction)
    {
        if (State.Equals(LobbyStates.inGame))
            Arena.SetPendingAction(player, direction);
    }

    public Settings SetSettings(Settings settings) => Arena.SetSettings(settings);

    public List<Player> RemovePlayer(string playerName)
    {
        if (playerName == null)
        {
            throw new ArgumentNullException(nameof(playerName), "Attempt to remove player with null string.");
        }

        if (!PlayerExists(playerName))
            return GetAllPlayerStatus();

        if (playerName == HostPlayer)
            return null;

        _ = Arena.ClearSnake(playerName);
        _ = players.TryRemove(playerName, out _);
        return GetAllPlayerStatus();
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
