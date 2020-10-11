using System.Collections.Generic;
using System.Windows;
using WorkReportCreator.ViewModels;

namespace WorkReportCreator
{
    /// <summary>
    /// Логика взаимодействия для Report.xaml
    /// </summary>
    public partial class ReportsPage : Window
    {
        private ReportsPageViewModel _model;

        public StudentInformation StudentInformation { get; set; }

        private WorkAndStudentInfo _informationPage;

        public ReportsPage(WorkAndStudentInfo informationPage, List<string> laboratoryWorks, List<string> practicalWorks)
        {
            InitializeComponent();
            _model = new ReportsPageViewModel(laboratoryWorks, practicalWorks, this);
            _model.ButtonBackClicked += ShowWorkAndStudentInformation;
            _informationPage = informationPage;
            DataContext = _model;
        }

        private void ShowWorkAndStudentInformation(object sender)
        {
            Hide();
            _informationPage.Show();
        }
    }
}
