using System;
using Akka.Actor;
using Akka.Event;

namespace BeEfficient.Pomodoro.Core.Actors
{
    public class TimeCoordinatorActor : ReceiveActor
    {
        private readonly UpdateTimeAction _updateTimeAction;

        #region messages
        internal class StartMessage { }
        internal class StopMessage { }
        internal class ElapsedTimeMessage
        {
            public TimeSpan RemainingTime { get; }
            public TimeSpan InitialDuration { get; }

            public ElapsedTimeMessage(TimeSpan remainingTime, TimeSpan initialDuration)
            {
                RemainingTime = remainingTime;
                InitialDuration = initialDuration;
            }
        }
        #endregion

        private readonly IActorRef _timerActor;
        
        public TimeCoordinatorActor(UpdateTimeAction updateTimeAction)
        {
            _updateTimeAction = updateTimeAction;

            var timerActorProps = Props.Create(() => new TimerActor());
            _timerActor = Context.ActorOf(timerActorProps, "timerActor");

            Become(Waiting);
        }

        private void Waiting()
        {
            Receive<StartMessage>(message =>
            {
                Context.GetLogger().Warning("Waiting - Start Message");

                Become(Working);
                _timerActor.Tell(new TimerActor.StartCounting(TimeSpan.FromMinutes(25), TimeSpan.FromSeconds(1)));
            });
            Receive<StopMessage>(message => { });
        }

        private void Working()
        {
            Receive<StartMessage>(message => { });
            Receive<StopMessage>(message =>
            {
                Context.GetLogger().Warning("Working - Stop Message");

                Become(Waiting);
                _timerActor.Tell(new TimerActor.StopCounting());
            });
            Receive<ElapsedTimeMessage>(message =>
            {
                _updateTimeAction(message.RemainingTime, message.InitialDuration);
            });
        }

        public delegate void UpdateTimeAction(TimeSpan remainingTime, TimeSpan initialDuration);
    }
}
