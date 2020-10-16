using System.Collections.Generic;
using System.Windows;
using WorkReportCreator.Models;
using WorkReportCreator.ViewModels;

namespace WorkReportCreator
{
    /// <summary>
    /// Окно с редактированием информации в шаблоне
    /// </summary>
    public partial class ReportsTemplate : Window
    {
        private readonly ReportsTemplateWindowViewModel _model = new ReportsTemplateWindowViewModel();

        private readonly MainWindow _mainWindow;

        public ReportsTemplate(MainWindow mainWindow)
        {
            InitializeComponent();
            DataContext = _model = new ReportsTemplateWindowViewModel();
            _mainWindow = mainWindow;
        }

        public ReportsTemplate(MainWindow mainWindow, Dictionary<string, Dictionary<string, ReportInformation>> template, string filePath)
        {
            InitializeComponent();
            DataContext = _model = new ReportsTemplateWindowViewModel(template, filePath);
            _mainWindow = mainWindow;
        }

        private void ShowMainWindow(object sender, System.ComponentModel.CancelEventArgs e)
        {
            bool shouldClose = false;
            if (string.IsNullOrEmpty(_model.FilePath))
            {
                shouldClose = MessageBox.Show("Данные не сохранены!\nПри выходе они будет уничножены!\nВы уверены?", "Внимание!",
                           MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No) == MessageBoxResult.Yes;
            }

            if (shouldClose)
                _mainWindow.Show();
            else
            {
                e.Cancel = true;
                return;
            }
        }
    }
}
