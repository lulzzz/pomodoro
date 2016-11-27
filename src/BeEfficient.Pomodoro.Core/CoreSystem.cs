using System;
using Akka.Actor;
using BeEfficient.Pomodoro.Core.Actors;

namespace BeEfficient.Pomodoro.Core
{
    public class CoreSystem
    {
        private readonly IActorRef _timeCoordinator;
        private readonly IActorRef _notificationActor;

        public CoreSystem()
        {
            var actorSystem = ActorSystem.Create("BeEfficientPomodoro");

            Props notificationActor = Props.Create(() => new NotificationActor(OnUpdateRequested, OnCycleChanged));
            _notificationActor = actorSystem.ActorOf(notificationActor, "notificationActor");

            Props timeCoordinatorActorProps = Props.Create(() => new TimeCoordinatorActor(_notificationActor));
            _timeCoordinator = actorSystem.ActorOf(timeCoordinatorActorProps, "timeCoordinator");
        }

        public void Start()
        {
            _timeCoordinator.Tell(new TimeCoordinatorActor.StartMessage());
        }

        public void Stop()
        {
            _timeCoordinator.Tell(new TimeCoordinatorActor.StopMessage());
        }

        private void OnUpdateRequested(TimeSpan remainingtime, TimeSpan initialduration)
        {
            StateChanged?.Invoke(remainingtime, initialduration);
        }

        private void OnCycleChanged(int cycleNumber, CycleTypes types)
        {
            CycleChanged?.Invoke(cycleNumber, types);
        }

        public event StateChangedHandler StateChanged;

        public event CycleChangedHandler CycleChanged;

        public delegate void StateChangedHandler(TimeSpan remainingtime, TimeSpan initialduration);

        public delegate void CycleChangedHandler(int cycleNumber, CycleTypes type);
    }
}
