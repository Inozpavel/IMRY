using System.Collections.Generic;
using System.Windows;
using WorkReportCreator.Models;
using WorkReportCreator.ViewModels;

namespace WorkReportCreator
{
    /// <summary>
    /// Логика взаимодействия для ReportsTemplate.xaml
    /// </summary>
    public partial class ReportsTemplate : Window
    {
        private readonly ReportsTemplateWindowViewModel _model = new ReportsTemplateWindowViewModel();

        private MainWindow _mainWindow;

        public ReportsTemplate(MainWindow mainWindow)
        {
            InitializeComponent();
            _model = new ReportsTemplateWindowViewModel();
            DataContext = _model;
            _mainWindow = mainWindow;
        }

        public ReportsTemplate(MainWindow mainWindow, Dictionary<string, Dictionary<string, ReportInformation>> template, string filePath)
        {
            InitializeComponent();
            _model = new ReportsTemplateWindowViewModel(template, filePath);
            DataContext = _model;
            _mainWindow = mainWindow;
        }

        private void ShowMainWindow(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (string.IsNullOrEmpty(_model.FilePath))
            {
                if (MessageBox.Show("Данные не сохранены!\nПри выходе они будет уничножены!\nВы уверены?", "Внимание!",
                           MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No) == MessageBoxResult.Yes)
                {
                    Hide();
                    _mainWindow.Show();

                }
                else
                {
                    e.Cancel = true;
                    return;
                }
            }
            Hide();
            _mainWindow.Show();
        }
    }
}
