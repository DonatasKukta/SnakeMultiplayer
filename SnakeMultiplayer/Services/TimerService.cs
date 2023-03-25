using System.Collections.Generic;
using System.Timers;

namespace SnakeMultiplayer.Services
{
    public interface ITimerService
    {
        void StartGame(string lobby, Speed speed);
    }

    public class TimerService : ITimerService
    {
        readonly Dictionary<string, Timer> Timers = new();
        readonly IGameServerService GameServer;
        readonly IServerHub ServerHub;


        public TimerService(IGameServerService gameServer, IServerHub serverHub)
        {
            GameServer = gameServer;
            ServerHub = serverHub;
        }

        Timer GetTimer(string name) => Timers.TryGetValue(name, out var timer) ? timer : null;

        public void StartGame(string lobby, Speed speed)
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
            Timers.Add(lobby, timer);
        }

        public void OnTimedUpdate(ILobbyService lobby)
        {
            var status = lobby.UpdateLobbyState();

            if (status == null)
                EndGame(lobby.ID);
            else
                _ = ServerHub.SendArenaStatusUpdate(lobby.ID, status);
        }

        public void EndGame(string lobby)
        {
            GetTimer(lobby)?.Stop();
            _ = Timers.Remove(lobby);

            GameServer.EndGame(lobby);
            _ = ServerHub.SendEndGame(lobby);
        }
    }
}
