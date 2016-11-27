using System;
using Akka.Actor;
using Akka.Event;

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
            public TimeSpan Wait { get; }
            public TimeSpan NotificationInterval { get; }

            public StartCounting(TimeSpan wait, TimeSpan duration, TimeSpan notificationInterval)
            {
                Wait = wait;
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
            Receive<StartCounting>(message => { Context.GetLogger().Warning("Already started"); });
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
            Receive<StopCounting>(message => { Context.GetLogger().Warning("Waiting already"); });
            Receive<Tick>(message => { Context.GetLogger().Warning("Currently waiting"); });
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

            _cancelRequest = Context.System.Scheduler.ScheduleTellRepeatedlyCancelable(message.Wait, message.NotificationInterval, Self, new Tick(), Sender);
        }

        private void HandleStop(StopCounting message)
        {
            _cancelRequest.Cancel();
        }

        private void HandleTick()
        {
            _timeLeft -= _interval;

            Sender.Tell(new TimeCoordinatorActor.TimeElapsingMessage(_timeLeft, _originalDuration));

            if (_timeLeft <= TimeSpan.Zero)
            {
                Self.Tell(new StopCounting());
                Sender.Tell(new TimeCoordinatorActor.TimeElapsedMessage());
            }
        }
    }
}
