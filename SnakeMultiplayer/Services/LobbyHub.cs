using System;
using System.Threading.Tasks;

using JsonLibrary;
using JsonLibrary.FromServer;

using Microsoft.AspNetCore.SignalR;

namespace SnakeMultiplayer.Services;

/// <summary>
/// Methods available for Clients to call the Server
/// </summary>
public interface IClientHub
{
    Task Ping();
    Task JoinLobby(string lobbyName, string playerName);
    void InitiateGameStart(Message message);
    void UpdateLobbySettings(Message message);
    void OnPlayerUpdate(Message message);
}

/// <summary>
/// Methods available for Server to call the Clients
/// </summary>
public interface IServerHub
{
    Task SendPlayerStatusUpdate(string lobby);
    Task SendSettingsUpdate();
    Task StartGame();
    Task SendArenaStatusUpdate(string looby, ArenaStatus status);
    Task EndGame();
}

public class LobbyHub : Hub, IClientHub, IServerHub
{
    string PlayerName;
    string LobbyName;
    ILobbyService LobbyService;
    readonly IGameServerService GameServer;

    public LobbyHub(IGameServerService gameServer)
    {
        GameServer = gameServer;
    }

    public async Task Ping()
    {
        Console.WriteLine("Ping received from Client");
        await Clients.All.SendAsync("Ping", DateTime.Now);
    }

    public override Task OnDisconnectedAsync(Exception? exception) =>
        base.OnDisconnectedAsync(exception);

    public async Task JoinLobby(string lobby, string playerName)
    {
        await Groups.AddToGroupAsync(this.Context.ConnectionId, lobby);
        GameServer.AddPlayerToLobby(LobbyName, PlayerName, default);
        LobbyName = lobby;
        PlayerName = playerName;
        LobbyService = GameServer.GetLobbyService(lobby);
        await (this as IServerHub).SendPlayerStatusUpdate(LobbyName);
    }

    public void UpdateLobbySettings(Message message) =>
        LobbyService.HandleMessage(message);

    public void InitiateGameStart(Message message) =>
        LobbyService.HandleMessage(message);

    public void OnPlayerUpdate(Message message) =>
        LobbyService.HandleMessage(message);

    Task IServerHub.SendPlayerStatusUpdate(string lobby)
    {
        return Task.CompletedTask;
    }

    Task IServerHub.SendSettingsUpdate()
    {
        return Task.CompletedTask;
    }

    Task IServerHub.StartGame()
    {
        return Task.CompletedTask;
    }

    Task IServerHub.SendArenaStatusUpdate(string looby, ArenaStatus status)
    {
        return Task.CompletedTask;
    }

    Task IServerHub.EndGame()
    {
        return Task.CompletedTask;
    }
}

