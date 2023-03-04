﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.WebSockets;
using System.Text.RegularExpressions;
using System.Threading;

using JsonLibrary;

namespace SnakeMultiplayer.Services;

/// <summary>
/// Gives abstraction layer to web socket based communication:
/// Distributes incoming messages to relevant lobbies and 
/// forwads messages from lobbies to web sockets
/// </summary>
public class GameServerService
{
    //TODO: Move to constants
    public static Regex ValidStringRegex = new(@"^[a-zA-Z0-9]+[a-zA-Z0-9\s_]*[a-zA-Z0-9]+$");
    readonly int MaxPlayersInLobby = 4;

    readonly ConcurrentDictionary<string, Lobby> lobbies = new();

    public string AddPlayerToLobby(string lobby, string player, WebSocket socket)
    {
        if (socket == null)
        {
            return "Tried to add null web socket to concurrent list";
        }

        try
        {
            return lobbies[lobby].AddPlayer(player, socket);
        }
        catch (Exception ex)
        {
            return ex.Message;
        }
    }

    public void SendPLayerStatusMessage(string lobby) =>
        lobbies[lobby].LobbyService.SendPLayerStatusMessage();

    public void HandleLobbyMessage(string lobby, Message message)
    {
        lobbies[lobby].LobbyService.HandleMessage(message);
    }

    public void SendLobbyMessage(string lobby, Message message)
    {
        if (!lobbies.TryGetValue(lobby, out var currLobby))
        {
            return;
        }

        var sockets = currLobby.GetPlayersWebSockets();
        foreach (var socket in sockets)
        {
            SendMessageAsync(socket, message);
        }
    }

    public bool TryCreateLobby(string lobbyName, string hostPlayerName, GameServerService service)
        => lobbies.TryAdd(lobbyName, new Lobby(lobbyName, hostPlayerName, MaxPlayersInLobby, service));

    public string CanJoin(string lobbyName, string playerName) =>
        !lobbies.TryGetValue(lobbyName, out var lobby)
            ? $"Lobby {lobbyName} does not exist. Please try a different name"
        : !lobby.IsActive()
            ? $"Lobby {lobbyName} is not active, therefore you cannot join it."
        : lobby.IsFull()
            ? $"Lobby {lobbyName} is full. Please try again later."
        : lobby.PlayerExists(playerName)
            ? $"Name {playerName} is already taken. Please use another name." : string.Empty;

    public bool LobbyExists(string lobbyName) => lobbies.ContainsKey(lobbyName);

    public bool PlayerExists(string lobbyName, string playerName) =>
        lobbies.TryGetValue(lobbyName, out var current)
        ? current.PlayerExists(playerName)
        : throw new EntryPointNotFoundException($"Lobby {lobbyName} does not exists");

    public void RemoveLobby(string lobby)
    {
        if (lobby == null)
        {
            throw new ArgumentNullException(nameof(lobby), "Tried to remove null lobby from lobby dictionary");
        }

        if (lobbies.ContainsKey(lobby))
        {
            lobbies[lobby].LobbyService.SendCloseLobbyMessage("Host has left the lobby.\n Please create new or join another lobby.");
        }

        _ = lobbies.TryRemove(lobby, out _);
    }

    public void RemovePlayer(string lobby, string player)
    {
        lobbies[lobby].RemovePlayer(player);
    }

    public List<Tuple<string, string>> GetLobbyStatus()
    {
        var lobbyList = new List<Tuple<string, string>>(lobbies.Count);

        foreach (var pair in lobbies)
        {
            lobbyList.Add(new Tuple<string, string>(pair.Key, pair.Value.GetPlayerCount().ToString()));
        }
        return lobbyList;
    }

    static async void SendMessageAsync(WebSocket webSocket, Message message)
    {
        try
        {
            var buffer = Strings.getBytes(Message.Serialize(message));
            await webSocket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, CancellationToken.None);
        }
        catch (Exception ex)
        {
            // TODO: Handle
            Debug.WriteLine($"Could not send to lobby {message.lobby}, of type {message.type}. Error: {ex.Message}");
        }
    }
}