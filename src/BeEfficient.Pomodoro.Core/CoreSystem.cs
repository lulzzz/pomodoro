using System;
using Akka.Actor;
using BeEfficient.Pomodoro.Core.Actors;
using Debug = System.Diagnostics.Debug;

namespace BeEfficient.Pomodoro.Core
{
    public class CoreSystem
    {
        private readonly IActorRef _timeCoordinator;

        public CoreSystem()
        {
            var actorSystem = ActorSystem.Create("BeEfficientPomodoro");

            Props timeCoordinatorActorProps = Props.Create(() => new TimeCoordinatorActor(OnUpdateRequested));

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

        public event StateChangedHandler StateChanged;

        public delegate void StateChangedHandler(TimeSpan remainingtime, TimeSpan initialduration);
    }
}
