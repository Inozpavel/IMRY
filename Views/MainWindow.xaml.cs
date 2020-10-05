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

        private void ShowDialogReportSelect(object sender, RoutedEventArgs e)
        {
            WorkAndStudentInfo document = new WorkAndStudentInfo(this);
            Hide();
            document.Show();
        }

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
