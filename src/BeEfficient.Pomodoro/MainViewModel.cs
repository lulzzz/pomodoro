using System;
using System.Windows;
using System.Windows.Input;
using BeEfficient.Pomodoro.Commands;
using BeEfficient.Pomodoro.Core;
using BeEfficient.Pomodoro.Core.Actors;
using PropertyChanged;

namespace BeEfficient.Pomodoro
{
    [ImplementPropertyChanged]
    public class MainViewModel
    {
        private bool _running;
        private readonly CoreSystem _core;

        public string Progress { get; set; }
        public int CycleNumber { get; set; }
        public string CycleName { get; set; }

        public ICommand StartCommand { get; set; }
        public ICommand StopCommand { get; set; }

        public MainViewModel()
        {
            _core = new CoreSystem();

            _core.StateChanged += CoreOnStateChanged;
            _core.CycleChanged += CycleChanged;

            _running = false;

            StartCommand = new RelayCommand(Start, CanStart);
            StopCommand  = new RelayCommand(Stop, CanStop);
        }

        private void CoreOnStateChanged(TimeSpan remainingtime, TimeSpan initialduration)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                Progress = remainingtime.ToString();
            });
        }

        private void CycleChanged(int cycleNumber, CycleTypes type)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                CycleNumber = cycleNumber;
                CycleName = GetCycleName(type);

                MessageBox.Show(CycleName, string.Empty, MessageBoxButton.OK, MessageBoxImage.Information);
            });
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
                    return "Długa przerwa";;
            }

            return string.Empty;
        }

        private void Start()
        {
            _core.Start();
            _running = true;
        }

        private void Stop()
        {
            _running = false;
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
