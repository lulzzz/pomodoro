using System;
using Akka.Actor;
using Akka.Event;

namespace BeEfficient.Pomodoro.Core.Actors
{
    public class TimeCoordinatorActor : ReceiveActor
    {
        #region messages
        internal class StartMessage { }
        internal class StopMessage { }
        internal class TimeElapsingMessage
        {
            public TimeSpan RemainingTime { get; }
            public TimeSpan InitialDuration { get; }

            public TimeElapsingMessage(TimeSpan remainingTime, TimeSpan initialDuration)
            {
                RemainingTime = remainingTime;
                InitialDuration = initialDuration;
            }
        }

        internal class TimeElapsedMessage { }
        #endregion

        private readonly IActorRef _timerActor;
        private readonly IActorRef _notificationActor;

        private int _numberOfCycles;
        
        public TimeCoordinatorActor(IActorRef notificationActor)
        {
            _notificationActor = notificationActor;
            var timerActorProps = Props.Create(() => new TimerActor());
            _timerActor = Context.ActorOf(timerActorProps, "timerActor");

            Become(Waiting);
        }

        private void Waiting()
        {
            Receive<StartMessage>(message =>
            {
                Context.GetLogger().Debug("WaitingCoordinator -> Start Message");
                Become(Working);
                HandleStart();
            });
            Receive<StopMessage>(message => { });
        }

        private void Working()
        {
            Receive<StartMessage>(message => { });
            Receive<StopMessage>(message =>
            {
                Context.GetLogger().Debug("WorkingCoordinator -> Stop Message");
                Become(Waiting);
                HandleStop();
            });
            Receive<TimeElapsingMessage>(message =>
            {
                HandleElapsingTime(message);
            });
            Receive<TimeElapsedMessage>(message =>
            {
                HandleElapsedTime();
            });
        }

        private void HandleStart()
        {
            _numberOfCycles = 0;

            var cycleParams = GetCycleParams(_numberOfCycles);

            _timerActor.Tell(new TimerActor.StartCounting(TimeSpan.Zero, cycleParams.EstimatedDuration, TimeSpan.FromSeconds(1)));
            _notificationActor.Tell(new NotificationActor.NotifyCycleChanged(_numberOfCycles, cycleParams.CycleType));
        }

        private void HandleStop()
        {
            _numberOfCycles = 0;

            _timerActor.Tell(new TimerActor.StopCounting());

            _notificationActor.Tell(new NotificationActor.NotifyTimeChanged(TimeSpan.Zero, TimeSpan.Zero));
            _notificationActor.Tell(new NotificationActor.NotifyCycleChanged(_numberOfCycles, CycleTypes.NotWorking));
        }

        private void HandleElapsingTime(TimeElapsingMessage message)
        {
            _notificationActor.Tell(new NotificationActor.NotifyTimeChanged(message.RemainingTime, message.InitialDuration));
        }

        private void HandleElapsedTime()
        {
            _numberOfCycles++;
            
            var cycleParams = GetCycleParams(_numberOfCycles);
            _notificationActor.Tell(new NotificationActor.NotifyCycleChanged(_numberOfCycles, cycleParams.CycleType));
            _timerActor.Tell(new TimerActor.StartCounting(TimeSpan.FromSeconds(1), cycleParams.EstimatedDuration, TimeSpan.FromSeconds(1)));
            _numberOfCycles = cycleParams.NextCycleNumber;
        }

        private static CycleParams GetCycleParams(int cycleNumber)
        {
            int estimatedDuration = 0;
            CycleTypes cycleType = 0;
            int nextCycleNumber = cycleNumber;

            if (cycleNumber < 2)
            {
                estimatedDuration = 25;
                cycleType = CycleTypes.Working;
            }
            else if (cycleNumber == 2)
            {
                estimatedDuration = 5;
                cycleType = CycleTypes.ShortBreak;
            }
            else if (cycleNumber == 3)
            {
                estimatedDuration = 25;
                cycleType = CycleTypes.Working;
            }
            else
            {
                estimatedDuration = 15;
                cycleType = CycleTypes.LongBreak;
                nextCycleNumber = 0;
            }
            
            return new CycleParams
            {
                EstimatedDuration = TimeSpan.FromMinutes(estimatedDuration).Add(TimeSpan.FromSeconds(1)),
                CycleType = cycleType,
                NextCycleNumber = nextCycleNumber
            };
        }

        struct CycleParams
        {
            public int NextCycleNumber { get; set; }
            public TimeSpan EstimatedDuration { get; set; }
            public CycleTypes CycleType { get; set; }
        }
    }
}
