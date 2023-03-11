using System;
using System.Threading.Tasks;

using JsonLibrary;
using JsonLibrary.FromClient;
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
    Task UpdateLobbySettings(Message message);
    void InitiateGameStart(Message message);
    void UpdatePlayerState(Message message);
}

/// <summary>
/// Methods available for Server to call the Clients
/// </summary>
public interface IServerHub
{
    Task SendPlayerStatusUpdate(string lobby);
    Task SendArenaStatusUpdate(string looby, ArenaStatus status);
    Task EndGame();
}

public class LobbyHub : Hub, IClientHub, IServerHub
{
    static class ClientMethod
    {
        public const string OnSettingsUpdate = "OnSettingsUpdate";
        public const string OnPlayerStatusUpdate = "OnPlayerStatusUpdate";
        public const string OnPing = "OnPing";
    }

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

    public async Task UpdateLobbySettings(Message message)
    {
        if (message.type == "Settings")
        {
            var settings = Settings.Deserialize(message.body);
            //TODO: understand why LobbyService is null
            settings = LobbyService.SetSettings(settings);
            var update = new Message("server", LobbyName, "Settings", new { Settings = settings });
            await Clients.Group(LobbyName).SendAsync(ClientMethod.OnSettingsUpdate, update);
        }
    }

    public void InitiateGameStart(Message message) =>
        LobbyService.HandleMessage(message);

    public void UpdatePlayerState(Message message) =>
        LobbyService.HandleMessage(message);

    async Task IServerHub.SendPlayerStatusUpdate(string lobby)
    {
        var status = LobbyService.CreatePlayerStatusMessage();
        await Clients.Group(lobby).SendAsync(ClientMethod.OnPlayerStatusUpdate, status);
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

