using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using JsonLibrary;
using JsonLibrary.FromClient;
using JsonLibrary.FromServer;

using Microsoft.AspNetCore.SignalR;

namespace SnakeMultiplayer.Services;

/// <summary>
/// Methods available for Server to call the Clients
/// </summary>
public interface IServerHub
{
    Task SendArenaStatusUpdate(string looby, ArenaStatus status);
    Task SendEndGame(string lobby);
    Task ExitGame(string lobby, string reason);
    Task InitiateGameStart(string lobby, ArenaStatus report);
    Task SendSettingsUpdate(string lobby, Settings settings);
    Task SendPlayerStatusUpdate(string lobby, List<Player> players, string removedlayerName = null);
}

static class ClientMethod
{
    public const string OnSettingsUpdate = "OnSettingsUpdate";
    public const string OnPlayerStatusUpdate = "OnPlayerStatusUpdate";
    public const string OnPing = "OnPing";
    public const string OnGameEnd = "OnGameEnd";
    public const string OnLobbyMessage = "OnLobbyMessage";
    public const string OnGameStart = "OnGameStart";
    public const string OnArenaStatusUpdate = "OnArenaStatusUpdate";
}

public class ServerHub : IServerHub
{
    public IHubContext<LobbyHub> HubContext { get; }

    public ServerHub(IHubContext<LobbyHub> hubContext)
    {
        HubContext = hubContext;
    }

    public Task SendPlayerStatusUpdate(string lobby, List<Player> players, string removedPlayerName = null)
    {
        var message = new Message("server", lobby, "Players", new { players, removed = removedPlayerName });
        return HubContext.Clients.Group(lobby).SendAsync(ClientMethod.OnPlayerStatusUpdate, message);
    }

    public Task InitiateGameStart(string lobby, ArenaStatus report)
    {
        var message = new Message("server", lobby, "Start", new { Start = report });
        return HubContext.Clients.Group(lobby).SendAsync(ClientMethod.OnGameStart, message);
    }

    public Task SendArenaStatusUpdate(string lobby, ArenaStatus status)
    {
        var message = new Message("server", lobby, "Update", new { status });
        Console.WriteLine($"Sending: {Message.Serialize(message)}");
        return HubContext.Clients.Group(lobby).SendAsync(ClientMethod.OnArenaStatusUpdate, message);
    }

    public Task SendSettingsUpdate(string lobby, Settings settings)
    {
        var message = new Message("server", lobby, "Settings", new { Settings = settings });
        return HubContext.Clients.Group(lobby).SendAsync(ClientMethod.OnSettingsUpdate, message);
    }

    public Task SendEndGame(string lobby)
    {
        var message = new Message("server", lobby, "End", null);
        return HubContext.Clients.Group(lobby).SendAsync(ClientMethod.OnGameEnd, message);
    }

    public Task ExitGame(string lobby, string reason)
    {
        var message = new Message("server", lobby, "Exit", new { message = reason });
        return HubContext.Clients.Group(lobby).SendAsync(ClientMethod.OnPlayerStatusUpdate, message);
    }
}

