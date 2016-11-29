using System;
using System.Reactive.Linq;
using System.Windows.Input;
using System.Windows.Shell;
using BeEfficient.Pomodoro.Commands;
using BeEfficient.Pomodoro.Core;
using BeEfficient.Pomodoro.Core.Actors;
using PropertyChanged;

namespace BeEfficient.Pomodoro
{
    [ImplementPropertyChanged]
    public class MainViewModel
    {
        private readonly Action _bringToFront;
        private readonly CoreSystem _core;

        private bool _running;
        public string Progress { get; set; }
        public int CycleNumber { get; set; }
        public string CycleName { get; set; }
        public double ProgressPercent { get; set; }
        public TaskbarItemProgressState ProgressState { get; set; }

        public ICommand StartCommand { get; set; }
        public ICommand StopCommand { get; set; }
        public ICommand CloseWindowCommand { get; set; }

        public MainViewModel(Action bringToFront)
        {
            _bringToFront = bringToFront;
            _core = new CoreSystem();

            _running = false;

            StartCommand = new RelayCommand(Start, CanStart);
            StopCommand = new RelayCommand(Stop, CanStop);
            CloseWindowCommand = new RelayCommand(Close);

            _core.State
                .Select(state => new
                {
                    RemainingTime = state.RemainingTime.ToString(),
                    ProgressPercent = ComputerPercentage(state.InitialDuration, state.RemainingTime)
                })
                .ObserveOnDispatcher()
                .Subscribe(state =>
                {
                    Progress = state.RemainingTime;
                    ProgressPercent = state.ProgressPercent;
                });

            _core.Cycle
                .Select(cycle => new
                {
                    CycleNumber = cycle.CycleNumber,
                    CycleName = GetCycleName(cycle.Type),
                    ProgressState = GetProgressState(cycle.Type),
                    Type = cycle.Type
                })
                .ObserveOnDispatcher()
                .Subscribe(cycle =>
                {
                    CycleName = cycle.CycleName;
                    CycleNumber = cycle.CycleNumber;
                    ProgressState = cycle.ProgressState;
                    _running = cycle.Type != CycleTypes.NotWorking;

                    _bringToFront();
                    System.Media.SystemSounds.Beep.Play();
                });
        }

        private static double ComputerPercentage(TimeSpan initialDuration, TimeSpan remainingTime)
        {
            return (initialDuration.TotalSeconds - remainingTime.TotalSeconds) / initialDuration.TotalSeconds;
        }

        private void Close()
        {
            _core.ShutDown();
        }

        private TaskbarItemProgressState GetProgressState(CycleTypes type)
        {
            switch (type)
            {
                case CycleTypes.NotWorking:
                    return TaskbarItemProgressState.None;
                case CycleTypes.Working:
                    return TaskbarItemProgressState.Error;
                case CycleTypes.ShortBreak:
                case CycleTypes.LongBreak:
                    return TaskbarItemProgressState.Normal;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }

        private string GetCycleName(CycleTypes type)
        {
            switch (type)
            {
                case CycleTypes.NotWorking:
                    return "Nie pracuję";
                case CycleTypes.Working:
                    return "Pracuję";
                case CycleTypes.ShortBreak:
                    return "Krótka przerwa";
                case CycleTypes.LongBreak:
                    return "Długa przerwa"; ;
            }

            return string.Empty;
        }

        private void Start()
        {
            _core.Start();
        }

        private void Stop()
        {
            _core.Stop();
        }

        private bool CanStart()
        {
            return _running == false;
        }

        private bool CanStop()
        {
            return _running;
        }
    }
}
