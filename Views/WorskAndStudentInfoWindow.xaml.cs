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
        /// Модель данных этого элемента
        /// </summary>
        private readonly StudentInformationWindowViewModel _model;

        public WorksAndStudentInfoWindow()
        {
            InitializeComponent();
            _model = new StudentInformationWindowViewModel(this);
            DataContext = _model;
        }

        /// <summary>
        /// Показывает форму стартового окна
        /// </summary>
        private void ShowFormMainWindow(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Все несохраненные данные будут удалены!\nВы уверены?", "Внимание!",
                           MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No) == MessageBoxResult.Yes)
            {
                MainWindow mainWindow = new MainWindow();
                Hide();
                mainWindow.Show();
            }
        }


        /// <summary>
        /// Показывает форму с выбором заданий (при наличии) и файлов для отчета (при наличии)
        /// </summary>
        private void ShowFormReportsPage(object sender, RoutedEventArgs e)
        {

        }

        /// <summary>
        /// Завершает работу приложения
        /// </summary>
        private void ExitApplication(object sender, System.ComponentModel.CancelEventArgs e) => Application.Current.Shutdown();
    }
}
