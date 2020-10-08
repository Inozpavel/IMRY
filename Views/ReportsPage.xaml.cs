using System.Windows;
using System.Windows.Controls;

namespace WorkReportCreator
{
    /// <summary>
    /// Логика взаимодействия для Report.xaml
    /// </summary>
    public partial class ReportsPage : Window
    {
        public TabControl tabControl;
        private WorkAndStudentInfo _informationPage;
        public ReportsPage(WorkAndStudentInfo informationPage)
        {
            InitializeComponent();
            _informationPage = informationPage;
            tabsControl = tabControl;
        }

        private void ShowWorkAndStudentInformation(object sender, RoutedEventArgs e)
        {
            Hide();
            _informationPage.Show();
        }
    }
}
