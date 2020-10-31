using System.Windows;
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
            DataContext = _model = new SettingsViewModel();
            _mainWindow = mainWindow;

            gridTitlePagePath.Opacity = _model.Params.WorkHasTitlePage ? 1 : 0;
            gridTitlePageParams.Opacity = _model.Params.WorkHasTitlePageParams ? 1 : 0;
        }

        private void ShowMainWindow(object sender, System.ComponentModel.CancelEventArgs e) => _mainWindow.Show();
    }
}
