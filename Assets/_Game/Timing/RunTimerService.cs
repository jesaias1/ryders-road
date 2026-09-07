using Avoidance.Core.Services;

namespace Avoidance.Gameplay.Timing
{
    public sealed class RunTimerService : IRunTimerService
    {
        private double _elapsedSeconds;
        private bool _completed;

        public bool IsRunning { get; private set; }
        public bool IsCompleted => _completed;
        public double ElapsedSeconds => _elapsedSeconds;
        public double FinalSeconds { get; private set; }

        public void Start()
        {
            if (_completed)
            {
                return;
            }

            IsRunning = true;
        }

        public void Tick(double deltaSeconds)
        {
            if (IsRunning && deltaSeconds > 0d)
            {
                _elapsedSeconds += deltaSeconds;
            }
        }

        public void Stop()
        {
            IsRunning = false;
        }

        public double Complete()
        {
            if (!_completed)
            {
                _completed = true;
                FinalSeconds = _elapsedSeconds;
                IsRunning = false;
            }

            return FinalSeconds;
        }

        public void Reset()
        {
            _elapsedSeconds = 0d;
            FinalSeconds = 0d;
            _completed = false;
            IsRunning = false;
        }
    }

    public static class RunTimerFormatting
    {
        public static string Format(double seconds)
        {
            if (seconds < 0d || double.IsNaN(seconds))
            {
                seconds = 0d;
            }

            var totalMilliseconds = (long)System.Math.Round(
                seconds * 1000d,
                System.MidpointRounding.AwayFromZero);
            var minutes = totalMilliseconds / 60000L;
            var wholeSeconds = (totalMilliseconds % 60000L) / 1000L;
            var milliseconds = totalMilliseconds % 1000L;
            return $"{minutes:00}:{wholeSeconds:00}.{milliseconds:000}";
        }
    }
}
