using System;
using System.Collections.Generic;
using System.Timers;

using JsonLibrary;

using Microsoft.AspNetCore.SignalR;

namespace SnakeMultiplayer.Services
{
    public interface ITimerService
    {
        void StartRound(string lobby, Speed speed);
    }

    public class TimerService : ITimerService
    {
        readonly Dictionary<string, Timer> timers = new();
        readonly IGameServerService GameServer;
        readonly IHubContext<LobbyHub> LobbyHub;


        public TimerService(IGameServerService gameServer, IHubContext<LobbyHub> lobbyHub)
        {
            GameServer = gameServer;
            LobbyHub = lobbyHub;
        }

        Timer GetTimer(string name) => timers.TryGetValue(name, out var timer) ? timer : null;

        public void StartRound(string lobby, Speed speed)
        {
            var lobbyService = GameServer.GetLobbyService(lobby);
            if (lobbyService == null)
                return;

            var timerDelegate = (object source, ElapsedEventArgs e) => OnTimedUpdate(lobbyService);
            var timer = new Timer();
            timer.Interval = 70 * (int)speed;
            timer.Elapsed += new ElapsedEventHandler(timerDelegate);
            timer.AutoReset = true;
            timer.Start();
            timers.Add(lobby, timer);
        }

        void OnTimedUpdate(ILobbyService lobby)
        {
            var status = lobby.UpdateLobbyState();

            if (status == null)
                EndRound(lobby.ID);
            else
            {
                var message = new Message("server", lobby.ID, "Update", new { status });
                Console.WriteLine($"Sending: {Message.Serialize(message)}");
                LobbyHub.Clients.Group(lobby.ID).SendAsync("OnArenaStatusUpdate", message);
            }
        }

        public void EndRound(string lobby)
        {
            GetTimer(lobby)?.Stop();
            _ = timers.Remove(lobby);

            var message = new Message("server", lobby, "End", null);
            LobbyHub.Clients.Group(lobby).SendAsync("OnGameEnd", message);
        }
    }
}
