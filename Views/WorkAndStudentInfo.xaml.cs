using Newtonsoft.Json;
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

        private readonly List<ToggleButton> _practicalWorksButtons = new List<ToggleButton>();
        private readonly List<ToggleButton> _laboratoryWorksButtons = new List<ToggleButton>();

        private readonly MainWindow _mainWindow;

        public WorkAndStudentInfo(MainWindow mainWindow)
        {
            InitializeComponent();
            DataContext = _model;
            _mainWindow = mainWindow;
            var globalParams = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText("./GlobalConfig.json"));
            var permittedWorks = JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(File.ReadAllText(globalParams["PermittedWorksAndExtentionsPath"]));
            foreach (string i in permittedWorks["PermittedPracticalWorks"])
            {
                ToggleButton button = new ToggleButton { Content = i, Style = stackPanelWithWorks.Resources["NumberButton"] as Style };
                _practicalWorksButtons.Add(button);
                SPPracticalWorks.Children.Add(button);
            }

            foreach (string i in permittedWorks["PermittedLaboratoryWorks"])
            {
                ToggleButton button = new ToggleButton { Content = i, Style = stackPanelWithWorks.Resources["NumberButton"] as Style };
                _laboratoryWorksButtons.Add(button);
                SPLaboratoryWorks.Children.Add(button);
            }
        }

        private void ShowMainWindow(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Все несохраненные данные будут удалены!\nВы уверены?", "Внимание!",
                           MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No) == MessageBoxResult.Yes)
            {
                Hide();
                _mainWindow.Show();
            }
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

        private void GenerateReport(object sender, RoutedEventArgs e)
        {
            if (_laboratoryWorksButtons.All(x => x.IsChecked == false) && _practicalWorksButtons.All(x => x.IsChecked == false))
            {
                MessageBox.Show("Выберите хотя бы одну работу!", "Ошибка!", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            ReportsPage reportsPage = new ReportsPage(this) { StudentInformation = _model.Student };

            foreach (var i in _laboratoryWorksButtons.Where(x => x.IsChecked ?? false))
            {
                reportsPage.tabControl.Items.Add(new TabItem() { Header = $"{i.Content} лаб.", Content = new ReportView(reportsPage) });
            }

            foreach (var i in _practicalWorksButtons.Where(x => x.IsChecked ?? false))
            {
                reportsPage.tabControl.Items.Add(new TabItem() { Header = $"{i.Content} пр.", Content = new ReportView(reportsPage) });
            }

            Hide();
            reportsPage.Show();
        }
    }
}
