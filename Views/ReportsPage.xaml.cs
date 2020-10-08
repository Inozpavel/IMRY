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

        public StudentInformation StudentInformation { get; set; }

        private WorkAndStudentInfo _informationPage;

        public ReportsPage(WorkAndStudentInfo informationPage)
        {
            InitializeComponent();
            _informationPage = informationPage;
            tabControl = reportTabControl;
        }

        private void ShowWorkAndStudentInformation(object sender, RoutedEventArgs e)
        {
            Hide();
            _informationPage.Show();
        }
    }
}
