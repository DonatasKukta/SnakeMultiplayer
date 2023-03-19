using System;
using System.Timers;

namespace SnakeMultiplayer.Services
{
    public interface ITimerService
    {
        void StartRound(Speed speed, Action onTimerUpdate);
        void Stop();
    }

    public class TimerService : ITimerService
    {
        readonly Timer timer = new();

        public void StartRound(Speed speed, Action onTimerUpdate)
        {
            var timerDelegate = (object source, ElapsedEventArgs e) => onTimerUpdate();
            timer.Interval = 70 * (int)speed;
            timer.Elapsed += new ElapsedEventHandler(timerDelegate);
            timer.AutoReset = true;
            timer.Start();
        }

        public void Stop() => timer.Stop();
    }
}
