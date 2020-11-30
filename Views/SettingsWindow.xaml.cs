using System.ComponentModel;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
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

        private void CloseWindow(object sender, CancelEventArgs e)
        {
            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();
        }

        private void ValidateText(object sender, TextChangedEventArgs e)
        {
            string startText = (sender as TextBox).Text;
            string text = Regex.Replace((sender as TextBox).Text, @"[^A-Za-z0-9]", "");
            if (startText == text)
                return;
            (sender as TextBox).Text = text;
            (sender as TextBox).CaretIndex = text.Length;
        }
    }
}
