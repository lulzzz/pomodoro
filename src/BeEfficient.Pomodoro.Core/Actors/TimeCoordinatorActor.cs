using Akka.Actor;

namespace BeEfficient.Pomodoro.Core.Actors
{
    public class TimeCoordinatorActor : ReceiveActor
    {
        #region messages
        internal class StartMessage { }
        internal class StopMessage { }
        #endregion

        public TimeCoordinatorActor()
        {
            Become(Waiting);
        }

        private void Waiting()
        {
            Receive<StartMessage>(message =>
            {
               Become(Working); 
            });
            Receive<StopMessage>(message => { });
        }

        private void Working()
        {
            Receive<StartMessage>(message => { });
            Receive<StopMessage>(message =>
            {
                Become(Waiting);
            });
        }
    }
}
