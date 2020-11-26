using System.Windows;
using WorkReportCreator.Models;
using WorkReportCreator.ViewModels;

namespace WorkReportCreator.Views
{
    public partial class SettingsWindow : Window
    {
        /// <summary>
        /// Модель данных этого элемента
        /// </summary>
        private readonly SettingsViewModel _model;

        /// <summary>
        /// Главное окно
        /// </summary>
        private readonly MainWindow _mainWindow;

        public SettingsWindow(MainWindow mainWindow)
        {
            InitializeComponent();
            MainParams mainParams = new MainParams();
            DataContext = _model = new SettingsViewModel(mainParams);
            _mainWindow = mainWindow;

            gridTitlePagePath.Opacity = mainParams.WorkHasTitlePage ? 1 : 0;
            gridTitlePageParams.Opacity = mainParams.WorkHasTitlePageParams ? 1 : 0;
        }

        private void ShowMainWindow(object sender, System.ComponentModel.CancelEventArgs e) => _mainWindow.Show();
    }
}
