using System;
using Akka.Actor;

namespace BeEfficient.Pomodoro.Core.Actors
{
    public class TimerActor : ReceiveActor
    {
        private ICancelable _cancelRequest;
        private TimeSpan _timeLeft;
        private TimeSpan _interval;
        private TimeSpan _originalDuration;

        #region messages
        public class StartCounting
        {
            public TimeSpan Duration { get; }
            public TimeSpan NotificationInterval { get;}

            public StartCounting(TimeSpan duration, TimeSpan notificationInterval)
            {
                NotificationInterval = notificationInterval;
                Duration = duration;
            }
        }

        public class StopCounting { }

        private class Tick { }
        #endregion messages

        protected override void PreStart()
        {
            Become(Waiting);

            base.PreStart();
        }

        private void Active()
        {
            Receive<StartCounting>(message => { });
            Receive<StopCounting>(message =>
            {
                Become(Waiting);
                HandleStop(message);
            });
            Receive<Tick>(message =>
            {
                HandleTick();
            });
        }

        private void Waiting()
        {
            Receive<StopCounting>(message => { });
            Receive<Tick>(message => { });
            Receive<StartCounting>(message =>
            {
                Become(Active);
                HandleActivation(message);
            });
        }

        private void HandleActivation(StartCounting message)
        {
            _originalDuration = message.Duration;
            _timeLeft = message.Duration;
            _interval = message.NotificationInterval;

            _cancelRequest = Context.System.Scheduler.ScheduleTellRepeatedlyCancelable(TimeSpan.Zero, message.NotificationInterval, Self, new Tick(), Sender);
        }

        private void HandleStop(StopCounting message)
        {
            _cancelRequest.Cancel();
        }

        private void HandleTick()
        {
            _timeLeft -= _interval;

            Sender.Tell(new TimeCoordinatorActor.ElapsedTimeMessage(_timeLeft, _originalDuration));
        }
    }
}
