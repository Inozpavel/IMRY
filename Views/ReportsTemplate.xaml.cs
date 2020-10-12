using System.Windows;
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
        }
    }
}
