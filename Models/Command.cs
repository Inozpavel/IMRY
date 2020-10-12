using System;
using System.Windows.Input;

namespace WorkReportCreator
{
    public class Command : ICommand
    {
        /// <summary>
        /// Метод, который выполнится, если команда может запуститься
        /// </summary>
        private readonly Action<object> _execute;

        /// <summary>
        /// Метод, который проверяет, может ли команда выполниться
        /// </summary>
        private readonly Predicate<object> _canExecute;

        /// <param name="execute">Метод, который будет запускаться</param>
        /// <param name="canExecute">Метод, который проверяет, может ли команда запуститься</param>
        public Command(Action<object> execute, Predicate<object> canExecute)
        {
            _execute = execute;
            _canExecute = canExecute;
        }

        /// <summary>
        /// Событие, вызываемое при изменении условия запуска команды
        /// </summary>
        public event EventHandler CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }

        /// <summary>
        /// Проверяет, может ли команда выполниться
        /// </summary>
        /// <param name="sender">Объект, который вызвал команду</param>
        /// <returns><paramref name="True"/>, если может, в противном случае <paramref name="false"/></returns>
        public bool CanExecute(object sender) => _canExecute?.Invoke(sender) ?? true;

        /// <summary>
        /// Метод, который выполнится, если команда может запуститься
        /// </summary>
        /// <param name="sender">Объект, который вызвал команду</param>
        public void Execute(object sender) => _execute?.Invoke(sender);
    }
}
