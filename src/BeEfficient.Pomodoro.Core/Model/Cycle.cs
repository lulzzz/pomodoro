using BeEfficient.Pomodoro.Core.Actors;

namespace BeEfficient.Pomodoro.Core.Model
{
    public class Cycle
    {
        public int CycleNumber { get; }
        public CycleTypes Type { get; }

        public Cycle(int cycleNumber, CycleTypes type)
        {
            CycleNumber = cycleNumber;
            Type = type;
        }

        public delegate void CycleChangedAction(int cycleNumber, CycleTypes type);
    }
}