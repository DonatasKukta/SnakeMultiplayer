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
    Task SendArenaStatusUpdate(string looby, ArenaStatus status);
    Task EndGame(string lobby);
    Task ExitGame(string lobby, string reason);
    Task InitiateGameStart(string lobby, ArenaStatus report);
    Task SendPlayerStatusUpdate(string lobby, Message message);
    Task SendSettingsUpdate(string lobby, Settings settings);
    Task SendLobbyMessage(string lobby, Message message);
}

public class LobbyHub : Hub, IClientHub, IServerHub
{
    static class ClientMethod
    {
        public const string OnSettingsUpdate = "OnSettingsUpdate";
        public const string OnPlayerStatusUpdate = "OnPlayerStatusUpdate";
        public const string OnPing = "OnPing";
        public const string OnGameEnd = "OnGameEnd";
        public const string OnLobbyMessage = "OnLobbyMessage";
        public const string OnGameStart = "OnGameStart";
        public const string OnArenaStatusUpdate = "ArenaStatusUpdate";
    }

    string PlayerName
    {
        get => GetContextItemOrDefault<string>("PlayerName");
        set => Context.Items["PlayerName"] = value;
    }

    string LobbyName
    {
        get => GetContextItemOrDefault<string>("LobbyName");
        set => Context.Items["LobbyName"] = value;
    }

    ILobbyService LobbyService
    {
        get => GetContextItemOrDefault<ILobbyService>("LobbyService");
        set => Context.Items["LobbyService"] = value;
    }

    readonly IGameServerService GameServer;

    public LobbyHub() { }

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
        await Groups.AddToGroupAsync(Context.ConnectionId, lobby);
        GameServer.AddPlayerToLobby(LobbyName, PlayerName, default);
        LobbyName = lobby;
        PlayerName = playerName;
        LobbyService = GameServer.GetLobbyService(lobby);
        var message = LobbyService.CreatePlayerStatusMessage();
        await (this as IServerHub).SendPlayerStatusUpdate(LobbyName, message);
    }

    public async Task UpdateLobbySettings(Message message)
    {
        //TODO: Check if player is host
        if (message.type == "Settings")
        {
            var settings = Settings.Deserialize(message.body);
            settings = LobbyService.SetSettings(settings);
            await (this as IServerHub).SendSettingsUpdate(LobbyName, settings);
        }
    }

    public void InitiateGameStart(Message message) =>
        LobbyService.HandleMessage(message);

    public void UpdatePlayerState(Message message) =>
        LobbyService.HandleMessage(message);

    async Task IServerHub.SendPlayerStatusUpdate(string lobby, Message message)
    {
        await Clients.Group(lobby).SendAsync(ClientMethod.OnPlayerStatusUpdate, message);
    }

    async Task IServerHub.InitiateGameStart(string lobby, ArenaStatus report)
    {
        var message = new Message("server", lobby, "Start", new { Start = report });
        await Clients.Group(lobby).SendAsync(ClientMethod.OnGameStart, message);
    }

    async Task IServerHub.SendArenaStatusUpdate(string lobby, ArenaStatus status)
    {
        var message = new Message("server", lobby, "Update", new { status });
        await Clients.Group(lobby).SendAsync(ClientMethod.OnArenaStatusUpdate, message);
    }

    async Task IServerHub.SendSettingsUpdate(string lobby, Settings settings)
    {
        var message = new Message("server", LobbyName, "Settings", new { Settings = settings });
        await Clients.Group(lobby).SendAsync(ClientMethod.OnSettingsUpdate, message);
    }

    async Task IServerHub.EndGame(string lobby)
    {
        var message = new Message("server", lobby, "End", null);
        await Clients.Group(lobby).SendAsync(ClientMethod.OnGameEnd, message);
    }

    async Task IServerHub.ExitGame(string lobby, string reason)
    {
        var message = new Message("server", lobby, "Exit", new { message = reason });
        await Clients.Group(lobby).SendAsync(ClientMethod.OnPlayerStatusUpdate, message);
    }

    public async Task SendLobbyMessage(string lobby, Message message)
    {
        await Clients.Group(lobby).SendAsync(ClientMethod.OnLobbyMessage, message);
    }

    T GetContextItemOrDefault<T>(string key) =>
        Context.Items.TryGetValue(key, out var item)
        ? (T)item
        : default;

}

