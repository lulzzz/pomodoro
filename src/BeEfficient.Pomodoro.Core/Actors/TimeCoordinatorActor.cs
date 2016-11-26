﻿using System;
using Akka.Actor;
using Akka.Event;

namespace BeEfficient.Pomodoro.Core.Actors
{
    public class TimeCoordinatorActor : ReceiveActor
    {
        #region messages
        internal class StartMessage { }
        internal class StopMessage { }
        #endregion

        private readonly IActorRef _timerActor;
        
        public TimeCoordinatorActor()
        {
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
                _timerActor.Tell(new TimerActor.StartCounting(TimeSpan.FromMilliseconds(25), TimeSpan.FromSeconds(1)));
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
        }
    }
}