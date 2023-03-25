using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

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

    public override Task OnDisconnectedAsync(Exception? exception)
    {
        if (LobbyService == null)
            return base.OnDisconnectedAsync(exception);

        var players = LobbyService.RemovePlayer(PlayerName);

        if (players == null)
        {
            GameServer.RemoveLobby(LobbyName);
            ServerHub.ExitGame(LobbyName, "Host player left the game.");
        }
        else if (players.Any())
            ServerHub.SendPlayerStatusUpdate(LobbyName, players, PlayerName);
        else
            ServerHub.SendEndGame(LobbyName);

        return base.OnDisconnectedAsync(exception);
    }

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

    public async Task UpdateLobbySettings(JsonElement input)
    {
        if (!LobbyService.IsHost(PlayerName))
            return;
        var settingsStr = input.GetRawText();

        var settings = Settings.Deserialize(settingsStr);
        if (settings == null)
            return;

        var newSettings = LobbyService.SetSettings(settings);
        await ServerHub.SendSettingsUpdate(LobbyName, newSettings);
    }

    public async Task InitiateGameStart()
    {
        if (!LobbyService.IsHost(PlayerName))
            return;

        var arenaStatus = LobbyService.InitiateGameStart();
        await ServerHub.InitiateGameStart(LobbyName, arenaStatus);

        await Task.Delay(2000);

        if (!LobbyService.IsNoSpeed)
            TimerService.StartGame(LobbyName, LobbyService.Speed);
    }

    public void UpdatePlayerState(MoveDirection direction)
    {
        LobbyService.OnPlayerUpdate(PlayerName, direction);

        if (!LobbyService.IsNoSpeed)
            return;

        var status = LobbyService.UpdateLobbyState();

        if (status == null)
            _ = ServerHub.SendEndGame(LobbyName);
        else
            _ = ServerHub.SendArenaStatusUpdate(LobbyName, status);
    }

    T GetContextItemOrDefault<T>(string key) =>
        Context.Items.TryGetValue(key, out var item)
        ? (T)item
        : default;
}

