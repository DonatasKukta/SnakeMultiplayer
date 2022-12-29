﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.WebSockets;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

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
    readonly int WebSocketMessageBufferSize = 1024 * 4;
    readonly int MaxPlayersInLobby = 4;

    readonly ConcurrentDictionary<string, Lobby> lobbies = new();

    private string AddPlayerToLobby(string lobby, string player, WebSocket socket)
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

    public async Task HandleWebSocketAsync(WebSocket webSocket)
    {
        var lobby = string.Empty;
        var playerName = string.Empty;
        var closeStatus = WebSocketCloseStatus.Empty;
        try
        {
            var message = await ReceiveMessageAsync(webSocket);
            lobby = message.lobby;
            playerName = message.sender;

            if (string.IsNullOrEmpty(lobby) || !ValidStringRegex.IsMatch(lobby))
            {
                throw new ArgumentException($"Incorrent lobby name \"{lobby}\" received from web socket");
            }
            else if (string.IsNullOrEmpty(playerName) || !ValidStringRegex.IsMatch(playerName))
            {
                throw new ArgumentException($"Incorrent player name \"{playerName}\" received from web socket");
            }

            var errorMessage = AddPlayerToLobby(lobby, playerName, webSocket);

            if (!errorMessage.Equals(string.Empty))
            {
                throw new OperationCanceledException(errorMessage);
            }

            lobbies[lobby].LobbyService.SendPLayerStatusMessage();

            while (lobbies[lobby].PlayerExists(playerName))
            {
                var buffer = new byte[WebSocketMessageBufferSize];
                var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                if (result.CloseStatus.HasValue)
                {
                    Debug.WriteLine($"Player {playerName} disconnected from lobby {lobby}.");
                    break;
                }
                var receivedMessage = Strings.getString(buffer);
                dynamic msgObj = Strings.getObject(receivedMessage);
                var msg = new Message((string)msgObj.sender, (string)msgObj.lobby, (string)msgObj.type, msgObj.body);
                lobbies[lobby].LobbyService.HandleMessage(msg);
            }
        }
        catch (Exception)
        {
            // TODO: Handle
        }
        finally
        {

            lobbies[lobby].RemovePlayer(playerName);

            if (webSocket.State != WebSocketState.Closed)
            {
                CloseSocketAsync(webSocket, closeStatus);
            }
        }
    }

    private async void CloseSocketAsync(WebSocket webSocket, WebSocketCloseStatus status)
    {
        try
        {
            await webSocket.CloseAsync(status, null, CancellationToken.None);
        }
        catch (Exception)
        {
            // TODO: Handle
        }
    }

    private async void SendMessageAsync(WebSocket webSocket, Message message)
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

    private async Task<Message> ReceiveMessageAsync(WebSocket webSocket)
    {
        var buffer = new byte[WebSocketMessageBufferSize];
        _ = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

        var text = Strings.getString(buffer);
        return Message.Deserialize(text);
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
            throw new ArgumentNullException("Tried to remove null lobby from lobby dictionary");
        }

        if (lobbies.ContainsKey(lobby))
        {
            lobbies[lobby].LobbyService.SendCloseLobbyMessage("Host has left the lobby.\n Please create new or join another lobby.");
        }

        _ = lobbies.TryRemove(lobby, out _);
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

    //TODO: Refactor
    private class Lobby
    {
        readonly ConcurrentDictionary<string, WebSocket> players;
        public LobbyService LobbyService { get; private set; }

        public Lobby(string name, string hostName, int maxPlayers, GameServerService gameServer)
        {
            players = new ConcurrentDictionary<string, WebSocket>();
            LobbyService = new LobbyService(name, hostName, maxPlayers, gameServer);
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


        public bool PlayerExists(string playerName) => playerName == null
            ? throw new ArgumentNullException("Attempt to check existance of player with null string.")
            : players.ContainsKey(playerName);

        public void RemovePlayer(string player)
        {
            if (player == null)
            {
                throw new ArgumentNullException("Attempt to remove player with null string.");
            }

            LobbyService.RemovePlayer(player);
            _ = players.TryRemove(player, out _);
        }

        public WebSocket[] GetPlayersWebSockets() => players.Values.ToArray();
        public bool IsFull() => LobbyService.IsLobbyFull();
        public bool IsActive() => LobbyService.IsActive();
    }
}