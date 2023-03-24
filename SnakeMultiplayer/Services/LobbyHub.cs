using System;
using System.Threading.Tasks;

using JsonLibrary;
using JsonLibrary.FromClient;

using Microsoft.AspNetCore.SignalR;

namespace SnakeMultiplayer.Services;

public class LobbyHub : Hub
{
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
    readonly ITimerService TimerService;
    readonly IServerHub ServerHub;

    public LobbyHub(IGameServerService gameServer, ITimerService timerService, IServerHub serverHub)
    {
        GameServer = gameServer;
        TimerService = timerService;
        ServerHub = serverHub;
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
        LobbyName = lobby;
        PlayerName = playerName;
        await Groups.AddToGroupAsync(Context.ConnectionId, lobby);
        GameServer.AddPlayerToLobby(LobbyName, PlayerName);
        LobbyService = GameServer.GetLobbyService(lobby);

        var players = LobbyService.GetAllPlayerStatus();
        await ServerHub.SendPlayerStatusUpdate(LobbyName, players);
    }

    public async Task UpdateLobbySettings(Message message)
    {
        if (message.type == "Settings")
        {
            var settings = Settings.Deserialize(message.body);
            settings = LobbyService.SetSettings(settings);
            await ServerHub.SendSettingsUpdate(LobbyName, settings);
        }
    }

    public async Task InitiateGameStart(Message message)
    {
        var lobby = LobbyService;
        var arenaStatus = LobbyService.InitiateGameStart(message);
        await ServerHub.InitiateGameStart(LobbyName, arenaStatus);

        // TODO: Is this needed?
        await Task.Delay(2000);

        if (lobby.Speed != Speed.NoSpeed)
            TimerService.StartRound(LobbyName, lobby.Speed);
    }

    public void UpdatePlayerState(Message message) =>
        LobbyService.OnPlayerUpdate(message);


    T GetContextItemOrDefault<T>(string key) =>
        Context.Items.TryGetValue(key, out var item)
        ? (T)item
        : default;
}

