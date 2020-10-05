using System;
using System.Windows.Input;

namespace WorkReportCreator
{
    class Command : ICommand
    {
        private readonly Action<object> _execute;
        private readonly Predicate<object> _canExecute;

        public event EventHandler CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }

        public Command(Action<object> execute, Predicate<object> canExecute)
        {
            _execute = execute;
            _canExecute = canExecute;
        }

        public bool CanExecute(object sender) => _canExecute?.Invoke(sender) ?? true;

        public void Execute(object sender) => _execute?.Invoke(sender);
    }
}
