using System;
using System.Timers;

namespace SnakeMultiplayer.Services
{
    public interface ITimerService
    {
        void EndRound();
        void StartRound(Speed speed, Action onTimerUpdate);
    }

    public class TimerService : ITimerService
    {
        readonly Timer timer = new();

        public TimerService() => timer.AutoReset = true;


        public void StartRound(Speed speed, Action onTimerUpdate)
        {
            var timerDelegate = (object source, ElapsedEventArgs e) => onTimerUpdate();
            timer.Interval = 70 * (int)speed;
            timer.Elapsed += new ElapsedEventHandler(timerDelegate);
            timer.Start();
        }

        public void EndRound() => timer.Stop();
    }
}
