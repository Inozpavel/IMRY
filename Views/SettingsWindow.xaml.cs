using System.Windows;
using WorkReportCreator.Models;
using WorkReportCreator.ViewModels;

namespace WorkReportCreator.Views
{
    public partial class SettingsWindow : Window
    {
        public SettingsWindow()
        {
            InitializeComponent();
            MainParams mainParams = new MainParams();
            DataContext = new SettingsViewModel(mainParams);

            gridTitlePagePath.Opacity = mainParams.WorkHasTitlePage ? 1 : 0;
            gridTitlePageParams.Opacity = mainParams.WorkHasTitlePageParams ? 1 : 0;
        }

        private void CloseWindow(object sender, System.ComponentModel.CancelEventArgs e)
        {
            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();
            Close();
        }
    }
}
