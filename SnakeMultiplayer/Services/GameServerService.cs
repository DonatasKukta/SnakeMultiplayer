using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using JsonLibrary;
using System.Net.WebSockets;

namespace SnakeMultiplayer.Services
{
    public class GameServerService : IHostedService
    {
        ConcurrentDictionary<string, LobbyService> lobbies = new ConcurrentDictionary<string, LobbyService>();

        public Task StartAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// Creates new lobby and adds it to current lobby dictionary.
        /// </summary>
        /// <param name="lobbyId">Lobby ID</param>
        /// <param name="playerHost">Host player name</param>
        /// <returns>Returns false if lobby could not be added to lobby dictionary</returns>
        public bool createLobby(string lobbyId, string playerHost)
        {
            LobbyService newLobby = new LobbyService(lobbyId, playerHost, 4);
            return lobbies.TryAdd(lobbyId, newLobby);
        }

        /// <summary>
        /// Checks whether lobby with given id exists.
        /// </summary>
        /// <returns>True if lobby exists, otherwise false</returns>
        private bool lobbyExists(string lobbyId)
        {
            return lobbies.ContainsKey(lobbyId);
        }
        /// <summary>
        /// Forwards web socket request to lobby service
        /// </summary>
        /// <param name="webScoketRequest"></param>
        public async void Forward(WebSocket webSocket, Message message)
        {
            if (lobbyExists(message.lobby) && message.type.Equals("join"))
            {
                string result = lobbies[message.lobby].AddPlayer(message.sender, webSocket);
                if (!result.Equals(string.Empty))
                {
                    await webSocket.CloseAsync(WebSocketCloseStatus.PolicyViolation, result, CancellationToken.None);
                }
            }
            else if (!message.type.Equals("join"))
            {
                await webSocket.CloseAsync(WebSocketCloseStatus.ProtocolError, "Unexpected message type.", CancellationToken.None);
            }
            else
            {
                await webSocket.CloseAsync(WebSocketCloseStatus.ProtocolError, "Unexpected protocol behaviour.", CancellationToken.None);
            }
        }
        
    }
}
