using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using BeEfficient.Pomodoro.Commands;
using BeEfficient.Pomodoro.Core;
using PropertyChanged;

namespace BeEfficient.Pomodoro
{
    [ImplementPropertyChanged]
    public class MainViewModel
    {
        private bool _running;
        private readonly CoreSystem _core;

        public string Progress { get; set; }
        public ICommand StartCommand { get; set; }
        public ICommand StopCommand { get; set; }

        public MainViewModel()
        {
            _core = new CoreSystem();

            _core.StateChanged += CoreOnStateChanged;

            _running = false;
            Progress = TimeSpan.FromMinutes(25).ToString();

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
