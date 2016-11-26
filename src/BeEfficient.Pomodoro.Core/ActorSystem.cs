using Akka.Actor;
using BeEfficient.Pomodoro.Core.Actors;

namespace BeEfficient.Pomodoro.Core
{
    public class CoreSystem
    {
        private readonly IActorRef _timeCoordinator;

        public CoreSystem()
        {
            var actorSystem = ActorSystem.Create("BeEfficientPomodoro");

            Props timeCoordinatorActorProps = Props.Create(() => new TimeCoordinatorActor());

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
    }
}
