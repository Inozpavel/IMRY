using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using WorkReportCreator.ViewModels.Commands;

namespace WorkReportCreator
{
    public partial class WorkAndStudentInfo : Window
    {
        private readonly StudentInformationViewModel _model = new StudentInformationViewModel();

        private bool _shouldCheckAllLaboratoryWork = true;
        private bool _shouldCheckAllPracticalWorks = true;

        private readonly List<int> _permittedPracticalWorks = new List<int>() { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 12, 16 };
        private readonly List<int> _permittedLaboratoryWorks = new List<int>() { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 16, };

        private readonly List<ToggleButton> _practicalWorksButtons = new List<ToggleButton>();
        private readonly List<ToggleButton> _laboratoryWorksButtons = new List<ToggleButton>();

        private readonly MainWindow _mainWindow;
        public WorkAndStudentInfo(MainWindow mainWindow)
        {
            InitializeComponent();
            DataContext = _model;
            _mainWindow = mainWindow;
            foreach (int i in _permittedPracticalWorks)
            {
                ToggleButton button = new ToggleButton { Content = i.ToString(), Style = SPWorks.Resources["NumberButton"] as Style };
                _practicalWorksButtons.Add(button);
                SPPracticalWorks.Children.Add(button);
            }

            foreach (int i in _permittedLaboratoryWorks)
            {
                ToggleButton button = new ToggleButton { Content = i.ToString(), Style = SPWorks.Resources["NumberButton"] as Style };
                _laboratoryWorksButtons.Add(button);
                SPLaboratoryWorks.Children.Add(button);
            }
        }

        private void GenerateReport(object sender, RoutedEventArgs e)
        {
            if (_laboratoryWorksButtons.All(x => x.IsChecked == false) && _practicalWorksButtons.All(x => x.IsChecked == false))
            {
                MessageBox.Show("Выберите хотя бы одну работу!", "Ошибка!", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            ReportsPage report = new ReportsPage(this);

            foreach (var i in _laboratoryWorksButtons.Where(x => x.IsChecked ?? false))
            {
                TabItem tabItem = new TabItem() { Header = $"{i.Content} лаб. ", Content = new ReportView() };
                report.tabControl.Items.Add(tabItem);
                if (Directory.Exists("./Reports") == false)
                    Directory.CreateDirectory("./Reports");

                string documentPath = "./Reports/Lab_" + i.Content + ".docx";

                //DocX doc = DocX.Create(documentPath);
                //doc.InsertParagraph("123");
                //doc.SaveAs(new FileStream(documentPath, FileMode.Create));
            }

            foreach (var i in _practicalWorksButtons.Where(x => x.IsChecked ?? false))
            {
                report.tabControl.Items.Add(new TabItem() { Header = $"{i.Content} лаб. ", Content = new ReportView() });
            }
            Hide();
            report.Show();
        }

        private void ShowMainWindow(object sender, RoutedEventArgs e)
        {
            Hide();
            _mainWindow.Show();
        }

        private void CheckAllWorks(object sender, RoutedEventArgs e)
        {
            string parentName = ((sender as Button).Parent as StackPanel).Name;
            if (parentName.Contains("Practical"))
            {
                _practicalWorksButtons.ForEach(x => x.IsChecked = _shouldCheckAllPracticalWorks);
                _shouldCheckAllPracticalWorks = !_shouldCheckAllPracticalWorks;
            }
            else if (parentName.Contains("Laboratory"))
            {
                _laboratoryWorksButtons.ForEach(x => x.IsChecked = _shouldCheckAllLaboratoryWork);
                _shouldCheckAllLaboratoryWork = !_shouldCheckAllLaboratoryWork;
            }
        }
    }
}
