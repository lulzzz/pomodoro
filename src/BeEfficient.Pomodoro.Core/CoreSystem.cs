using System;
using Akka.Actor;
using BeEfficient.Pomodoro.Core.Actors;

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

            Props notificationActor = Props.Create(() => new NotificationActor(OnUpdateRequested, OnCycleChanged));
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
