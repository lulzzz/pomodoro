using System;
using System.Windows.Input;

namespace BeEfficient.Pomodoro.Commands
{
    public class RelayCommand : ICommand
    {
        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
        private readonly Action _methodToExecute;
        private readonly Func<bool> _canExecuteEvaluator;

        public RelayCommand(Action methodToExecute)
            : this(methodToExecute, null)
        {
        }

        public RelayCommand(Action methodToExecute, Func<bool> canExecuteEvaluator)
        {
            _methodToExecute = methodToExecute;
            _canExecuteEvaluator = canExecuteEvaluator;
        }

        public bool CanExecute(object parameter)
        {
            if (_canExecuteEvaluator == null)
                return true;

            return _canExecuteEvaluator.Invoke(); 
        }
        public void Execute(object parameter)
        {
            _methodToExecute.Invoke();
        }
    }
}
