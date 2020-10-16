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
    /// <summary>
    /// Окно с вводом информации о студенте и выбором работ
    /// </summary>
    public partial class WorksAndStudentInfoWindow : Window
    {
        /// <summary>
        /// Модель данных этого элемента
        /// </summary>
        private readonly StudentInformationWindowViewModel _model = new StudentInformationWindowViewModel();

        /// <summary>
        /// От значения зависит, все кнопки с практическими работами будут отмечены / не отмечены
        /// </summary>
        private bool _shouldCheckAllPracticalWorks = true;

        /// <summary>
        /// От значения зависит, все кнопки с лабораторными работами будут отмечены / не отмечены
        /// </summary>
        private bool _shouldCheckAllLaboratoryWork = true;

        /// <summary>
        /// Список всех кнопок для выбора номеров практических работ
        /// </summary>
        private readonly List<ToggleButton> _practicalWorksButtons = new List<ToggleButton>();

        /// <summary>
        /// Список всех кнопок для выбора номеров лабораторных работ
        /// </summary>
        private readonly List<ToggleButton> _laboratoryWorksButtons = new List<ToggleButton>();

        public WorksAndStudentInfoWindow()
        {
            InitializeComponent();
            DataContext = _model;

            var globalParams = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText("./MainConfig.json"));
            var worksTemplatePath = globalParams["CurrentTemplateFilePath"];

            var template = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, Dictionary<string, string>>>>(File.ReadAllText(worksTemplatePath));

            foreach (string type in template.Keys.Distinct())
            {
                foreach (string workNumber in template[type].Keys.Distinct())
                {
                    ToggleButton button = new ToggleButton() { Content = workNumber, Style = stackPanelWithWorks.Resources["NumberButton"] as Style };
                    if (type == "Practices")
                    {
                        _practicalWorksButtons.Add(button);
                        stackPanelPracticalWorks.Children.Add(button);
                    }
                    else if (type == "Laboratories")
                    {
                        _laboratoryWorksButtons.Add(button);
                        stackPanelLaboratoryWorks.Children.Add(button);
                    }
                }
            }
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
        /// Отмечает / снимает отметки со всех кнопок с практическими / лабораторными работами
        /// </summary>
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

        /// <summary>
        /// Показывает форму с выбором заданий (при наличии) и файлов для отчета (при наличии)
        /// </summary>
        private void ShowFormReportsPage(object sender, RoutedEventArgs e)
        {
            List<string> selectedLaboratoryWorks = _laboratoryWorksButtons.Where(x => x.IsChecked ?? false).Select(x => x.Content as string).ToList();
            List<string> selectedPracticalWorks = _practicalWorksButtons.Where(x => x.IsChecked ?? false).Select(x => x.Content as string).ToList();
            ReportsWindow reportsPage = new ReportsWindow(this, selectedLaboratoryWorks, selectedPracticalWorks, _model.Student);

            Hide();
            reportsPage.Show();
        }

        /// <summary>
        /// Завершает работу приложения
        /// </summary>
        private void ExitApplication(object sender, System.ComponentModel.CancelEventArgs e) => Application.Current.Shutdown();
    }
}
