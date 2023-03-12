using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Linq;
using System;

namespace SnakeMultiplayer.Services;

//TODO: Delete class
class Lobby
{
    public LobbyService LobbyService { get; set; }

    readonly ConcurrentDictionary<string, WebSocket> players;

    public Lobby(string name, string hostName, int maxPlayers, IGameServerService gameServer, LobbyHub lobbyHub)
    {
        players = new ConcurrentDictionary<string, WebSocket>();
        LobbyService = new LobbyService(name, hostName, maxPlayers, gameServer, lobbyHub);
    }

    public int GetPlayerCount() => LobbyService.GetPlayerCount();

    public string AddPlayer(string playerName, WebSocket webSocket)
    {
        if (playerName == null)
        {
            return "Attempt to add player with null string.";
        }
        else if (webSocket == null)
        {
            return $"Attempt to add player {playerName} with null WebSocket.";
        }
        else if (!IsActive())
        {
            return "Lobby {LobbyService.ID} is not active. Please join another lobby";
        }
        else if (IsFull())
        {
            return $"Lobby {LobbyService.ID} is full. Please join another lobby.";
        }

        var errorMessage = LobbyService.AddPlayer(playerName);
        if (!errorMessage.Equals(string.Empty))
        {
            return errorMessage;
        }

        if (!players.TryAdd(playerName, webSocket))
        {
            LobbyService.RemovePlayer(playerName);
            return $"Unexpected error while trying to join {LobbyService.ID}. Please try again later";
        }
        return string.Empty;
    }

    public bool PlayerExists(string playerName) =>
        playerName == null
        ? throw new ArgumentNullException(nameof(playerName), "Attempt to check existance of player with null string.")
        : players.ContainsKey(playerName);

    public void RemovePlayer(string player)
    {
        if (player == null)
        {
            throw new ArgumentNullException(nameof(player), "Attempt to remove player with null string.");
        }

        LobbyService.RemovePlayer(player);
        _ = players.TryRemove(player, out _);
    }

    public LobbyService GetLobbyService() => LobbyService;
    public WebSocket[] GetPlayersWebSockets() => players.Values.ToArray();
    public bool IsFull() => LobbyService.IsLobbyFull();
    public bool IsActive() => LobbyService.IsActive();
}