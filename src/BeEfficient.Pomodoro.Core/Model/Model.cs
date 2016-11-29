using System;

namespace BeEfficient.Pomodoro.Core.Model
{
    public class Time
    {
        public Time(TimeSpan remainingTime, TimeSpan initialDuration)
        {
            RemainingTime = remainingTime;
            InitialDuration = initialDuration;
        }

        public TimeSpan RemainingTime { get; }
        public TimeSpan InitialDuration { get; }

        public delegate void UpdateTimeAction(TimeSpan remainingTime, TimeSpan initialDuration);
    }
}
