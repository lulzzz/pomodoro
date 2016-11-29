using System;
using Akka.Actor;

namespace BeEfficient.Pomodoro.Core.Actors
{
    public class NotificationActor : ReceiveActor
    {
        #region messages

        public class NotifyTimeChanged
        {
            public TimeSpan RemainingTime { get;}
            public TimeSpan InitialDuration { get; }

            public NotifyTimeChanged(TimeSpan remainingTime, TimeSpan initialDuration)
            {
                RemainingTime = remainingTime;
                InitialDuration = initialDuration;
            }
        }

        public class NotifyCycleChanged
        {
            public int CycleNumber { get; set; }
            public CycleTypes CycleType { get; set; }

            public NotifyCycleChanged(int cycleNumber, CycleTypes cycleType)
            {
                CycleNumber = cycleNumber;
                CycleType = cycleType;
            }
        }

        #endregion messages

        private readonly UpdateTimeAction _onUpdateTimeRequested;
        private readonly CycleChangedAction _onCycleChanged;

        public NotificationActor(UpdateTimeAction onUpdateTimeRequested, CycleChangedAction onCycleChanged)
        {
            _onUpdateTimeRequested = onUpdateTimeRequested;
            _onCycleChanged = onCycleChanged;

            Receive<NotifyTimeChanged>(message =>
            {
                _onUpdateTimeRequested(message.RemainingTime, message.InitialDuration);
            });

            Receive<NotifyCycleChanged>(message =>
            {
                _onCycleChanged(message.CycleNumber, message.CycleType);
            });
        }
    }
}
