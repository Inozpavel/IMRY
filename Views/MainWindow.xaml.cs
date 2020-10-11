using Microsoft.Win32;
using System.Windows;

namespace WorkReportCreator
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Показывает окно с выбором заданий и вводом информации о студенте
        /// </summary>
        private void ShowFormReportsSelect(object sender, RoutedEventArgs e)
        {
            WorkAndStudentInfoWindow document = new WorkAndStudentInfoWindow();
            Hide();
            document.Show();
        }

        /// <summary>
        /// Показывает диалоговое окно для выбором файла с отчетом, чтобы редактировать его
        /// </summary>
        private void LoadReport(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog()
            {
                Title = "Выборите отчета для редактирования",
                Filter = "Xml файлы (*.xml)|*.xml|Все файлы (*.*)|*.*",
                DefaultExt = "xml",
            };

            if (dialog.ShowDialog() == true)
            {
                string filePath = dialog.FileName;
                MessageBox.Show("В процессе разработки...", "Work in progress!", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }
}
