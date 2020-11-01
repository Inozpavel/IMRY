using System;
using System.Windows;
using WorkReportCreator.ViewModels.Commands;

namespace WorkReportCreator
{
    /// <summary>
    /// Окно с вводом информации о студенте и выбором работ
    /// </summary>
    public partial class WorksAndStudentInfoWindow : Window
    {
        /// <summary>
        /// Главное меню
        /// </summary>
        private readonly MainWindow _mainWindow;

        /// <summary>
        /// Модель данных этого элемента
        /// </summary>
        private readonly StudentInformationWindowViewModel _model;

        /// <exception cref="Exception"/>
        public WorksAndStudentInfoWindow(MainWindow mainWindow)
        {
            InitializeComponent();
            _model = new StudentInformationWindowViewModel(this);
            DataContext = _model;
            _mainWindow = mainWindow;
        }

        /// <summary>
        /// Показывает форму стартового окна
        /// </summary>
        private void ShowFormMainWindow(object sender, RoutedEventArgs e)
        {
            Close();
            _mainWindow.Show();
        }

        /// <summary>
        /// Завершает работу приложения
        /// </summary>
        private void CloseApplicationClicked(object sender, System.ComponentModel.CancelEventArgs e) => _mainWindow.Show();
    }
}
