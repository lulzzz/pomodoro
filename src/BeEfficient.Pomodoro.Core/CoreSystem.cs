using System;
using System.Reactive.Subjects;
using Akka.Actor;
using BeEfficient.Pomodoro.Core.Actors;
using BeEfficient.Pomodoro.Core.Model;

namespace BeEfficient.Pomodoro.Core
{
    public class CoreSystem
    {
        private readonly IActorRef _timeCoordinator;
        private readonly IActorRef _notificationActor;
        private readonly ActorSystem _actorSystem;

        public CoreSystem()
        {
            _actorSystem = ActorSystem.Create("BeEfficientPomodoro");

            Time = new Subject<Time>();
            Cycle = new Subject<Cycle>();

            Time.UpdateTimeAction updateStateAction = (remainingtime, initialDuration) => Time.OnNext(new Time(remainingtime, initialDuration));
            Cycle.CycleChangedAction cycleChangedAction = (cycleNumber, cycleType) => Cycle.OnNext(new Cycle(cycleNumber, cycleType));

            Props notificationActor = Props.Create(() => new NotificationActor(updateStateAction, cycleChangedAction));
            _notificationActor = _actorSystem.ActorOf(notificationActor, "notificationActor");

            Props timeCoordinatorActorProps = Props.Create(() => new TimeCoordinatorActor(_notificationActor));
            _timeCoordinator = _actorSystem.ActorOf(timeCoordinatorActorProps, "timeCoordinator");
        }

        public void Start()
        {
            _timeCoordinator.Tell(new TimeCoordinatorActor.StartMessage());
        }

        public void Stop()
        {
            _timeCoordinator.Tell(new TimeCoordinatorActor.StopMessage());
        }

        public void ShutDown()
        {
            _actorSystem.Terminate();
        }

        public Subject<Cycle> Cycle { get; }
        public Subject<Time> Time { get; }
    }


}
