using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

using JsonLibrary.FromClient;
using JsonLibrary.FromServer;

using Microsoft.AspNetCore.SignalR;
using Microsoft.FSharp.Core;

using static Domain.Functions;

namespace SnakeMultiplayer.Services;

public class LobbyHub : Hub
{
    string PlayerId
    {
        get => GetContextItemOrDefault<string>("playerId");
        set => Context.Items["playerId"] = value;
    }

    string ArenaId
    {
        get => GetContextItemOrDefault<string>("LobbyName");
        set => Context.Items["LobbyName"] = value;
    }

    readonly GameServer GameServer;
    readonly ITimerService TimerService;
    readonly IServerHub ServerHub;

    public LobbyHub(GameServer gameServer, ITimerService timerService, IServerHub serverHub)
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
        //if (LobbyService == null)
        //    return base.OnDisconnectedAsync(exception);
        //
        //var players = LobbyService.RemovePlayer(playerId);

        //TODO: Catch exception
        var result = GameServer.RemovePlayer(ArenaId, PlayerId);

        ServerHub.ExitGame(ArenaId, "Host player left the game.");
        var players = GameServer.GetPlayers(ArenaId);

        if (FSharpOption<IEnumerable<Player>>.get_IsSome(players))
            ServerHub.SendPlayerStatusUpdate(ArenaId, players.Value, PlayerId);
        else
            ServerHub.ExitGame(ArenaId, "No players active in lobby.");

        return base.OnDisconnectedAsync(exception);
    }

    public async Task JoinLobby(string arenaId, string playerId)
    {
        ArenaId = arenaId;
        PlayerId = playerId;
        await Groups.AddToGroupAsync(Context.ConnectionId, arenaId);
        GameServer.AddPlayer(ArenaId, PlayerId);

        var players = GameServer.GetPlayers(ArenaId).Value.ToList();
        await ServerHub.SendPlayerStatusUpdate(ArenaId, players);
    }

    public async Task UpdateLobbySettings(JsonElement input)
    {
        if (!GameServer.isHost(ArenaId, PlayerId))
            return;
        var settingsStr = input.GetRawText();

        var settingsDto = Settings.Deserialize(settingsStr);
        if (settingsDto == null)
            return;
        var settings = Domain.settingsDtoToSettings(settingsDto);
        var newSettings = GameServer.setSettings(ArenaId, settings);
        await ServerHub.SendSettingsUpdate(ArenaId, settingsDto);
    }

    public async Task InitiateGameStart()
    {
        if (!LobbyService.IsHost(PlayerId))
            return;

        var arenaStatus = LobbyService.InitiateGameStart();
        await ServerHub.InitiateGameStart(ArenaId, arenaStatus);

        await Task.Delay(2000);

        if (!LobbyService.IsNoSpeed)
            TimerService.StartGame(ArenaId, LobbyService.Speed);
    }

    public void UpdatePlayerState(MoveDirection direction)
    {
        LobbyService.OnPlayerUpdate(PlayerId, direction);

        if (!LobbyService.IsNoSpeed)
            return;

        var status = LobbyService.UpdateLobbyState();

        if (status == null)
            _ = ServerHub.SendEndGame(ArenaId);
        else
            _ = ServerHub.SendArenaStatusUpdate(ArenaId, status);
    }

    T GetContextItemOrDefault<T>(string key) =>
        Context.Items.TryGetValue(key, out var item)
        ? (T)item
        : default;
}

