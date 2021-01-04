using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using WorkReportCreator.Models;
using WorkReportCreator.Views;

namespace WorkReportCreator.ViewModels
{
    internal class ReportsWindowViewModel : INotifyPropertyChanged
    {
        public Command AddImage { get; private set; }

        private int? _selectedIndex;

        private readonly Style _tabItemStyle;

        private enum ReportAction
        {
            Save,
            Generate,
        }

        /// <summary>
        /// Список всех вкладок
        /// </summary>
        public ObservableCollection<TabItem> TabItems { get; set; } = new ObservableCollection<TabItem>();

        /// <summary>
        /// Идекс текущей выбранной вкладки
        /// </summary>
        public int? SelectedIndex
        {
            get => _selectedIndex;
            set
            {
                _selectedIndex = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Вызывается при нажатии кнопки назад
        /// </summary>
        public event Action<object> ButtonBackClicked;

        public event PropertyChangedEventHandler PropertyChanged;

        /// <param name="laboratoryWorks">Список доступный для выбора лабораторных работ</param>
        /// <param name="practicalWorks">Список доступный для выбора приктических работ</param>
        /// <param name="reportsWindow">Окно, на котором расположен элемент</param>
        /// <exception cref="Exception"/>
        public ReportsWindowViewModel(List<string> laboratoryWorks, List<string> practicalWorks)
        {
            FastActionsItem fastActionsItem = new FastActionsItem
            {
                IsButtonsEnabled = laboratoryWorks.Count > 0 || practicalWorks.Count > 0
            };

            fastActionsItem.ButtonGenerateAllClicked += GenerateAllReports;
            fastActionsItem.ButtonSaveAllClicked += SaveAllReports;
            fastActionsItem.ButtonBackClicked += (sender) => ButtonBackClicked?.Invoke(sender);
            AddImage = new Command(TryAddImage, null);
            ResourceDictionary _tabItemStyleDictionary = new ResourceDictionary()
            {
                Source = new Uri("./Views/Styles/CommonTabItemStyle.xaml",
                UriKind.Relative)
            };
            _tabItemStyle = _tabItemStyleDictionary["CommonTabItemStyle"] as Style;

            TabItems.Add(new TabItem()
            {
                Header = "Быстрые действия",
                Content = fastActionsItem,
                Style = _tabItemStyle,
            });

            Dictionary<string, Dictionary<string, Report>> template;
            MainParams mainParams = new MainParams();
            try
            {
                template = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, Report>>>(File.ReadAllText(mainParams.CurrentTemplateFilePath));
                if (template == null)
                    throw new Exception();
            }
            catch (Exception)
            {
                throw new Exception("Не получилось загрузить данные из шаблона!");
            }

            if (template.Keys.Contains("Laboratories") == false && template.Keys.Contains("Practices") == false)
                throw new Exception("В файле с шаблоном отсутствуют и практические и лабораторные работы!");

            List<ReportModel> reports = new List<ReportModel>(); ;
            string reportsPath = mainParams.SavedReportsPath;
            if (Directory.Exists(reportsPath))
            {
                string[] paths = Directory.GetFiles(reportsPath, $"*.{mainParams.ShortSubjectName}.json");
                foreach (string path in paths)
                {
                    try
                    {
                        ReportModel report = JsonConvert.DeserializeObject<ReportModel>(File.ReadAllText(path));
                        if (report != null)
                        {
                            report.FilePath = path;
                            reports.Add(report);
                        }
                    }
                    catch
                    {
                        MessageBox.Show("Не получилось подгрузить данные из файла!\nФайл: " + path, "Внимание!", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    }
                }
            }
            LoadTemplateInfoFromKey(template, "Practices", "пр.", practicalWorks, _tabItemStyle, reports?.Where(x => x.WorkType == "Practice"));
            LoadTemplateInfoFromKey(template, "Laboratories", "лаб.", laboratoryWorks, _tabItemStyle, reports?.Where(x => x.WorkType == "Laboratory"));

            SelectedIndex = 0;
            OnPropertyChanged();
        }

        private void GenerateAllReports(object obj) => ExecuteWithAllReports(ReportAction.Generate, "созданы", "создании");

        private void SaveAllReports(object obj) => ExecuteWithAllReports(ReportAction.Save, "сохранены", "сохранении");

        /// <summary>
        /// Добавляет новые вкладки для всех выбранных работ
        /// </summary>
        /// <param name="template">Шаблон для работ</param>
        /// <param name="window">Окно со всеми выбранными работами</param>
        /// <param name="workType">Тип работы</param>
        /// <param name="shortDescription">Аббревеатура типа работы</param>
        /// <param name="selectedWorks">Выбранные пользователем работы</param>
        /// <param name="style">Стиль для TabItem</param>
        private void LoadTemplateInfoFromKey(Dictionary<string, Dictionary<string, Report>> template,
            string workType, string shortDescription, List<string> selectedWorks, Style style, IEnumerable<ReportModel> reports)
        {
            if (template.Keys.Contains(workType) == false)
                return;

            foreach (string number in template[workType].Keys.Where(x => selectedWorks.Contains(x)))
            {
                try
                {
                    List<string> dynamicTasks = template[workType][number].DynamicTasks.Select(x => x.Description).Where(x => string.IsNullOrEmpty(x) == false).ToList();
                    string task = template[workType][number].CommonTask;
                    string commonTask = string.IsNullOrEmpty(task) ? "Общего задания нет." : task;
                    ReportModel report = reports?.FirstOrDefault(x => x.WorkNumber.ToString() == number) ?? new ReportModel();
                    string reportName = $"{number} {shortDescription}";
                    report.ReportName = reportName;
                    TabItems.Add(new TabItem()
                    {
                        Header = reportName,
                        Content = new ReportItem(commonTask, dynamicTasks, report),
                        Style = style,
                    });
                }
                catch (Exception)
                {
                    MessageBox.Show($"Не получилось загрузить {number} {shortDescription} работу!",
                        $"Ошибка при загрузке работы!", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void ExecuteWithAllReports(ReportAction action, string actionName, string actionNameDuringAction)
        {
            List<int> failedPracticesReports = new List<int>();
            List<int> failedLaboratoriesReports = new List<int>();

            List<int> workNumbers = new List<int>();
            List<ReportItem> reportItems = new List<ReportItem>();
            List<string> reportNames = new List<string>();
            for (int i = 1; i < TabItems.Count; i++)
            {
                workNumbers.Add(int.Parse(Regex.Match(TabItems[i].Header as string, @"\d+").Value));
                reportItems.Add(TabItems[i].Content as ReportItem);
                reportNames.Add(TabItems[i].Header as string);
            }
            int completedCount = 0;

            for (int i = 0; i < reportItems.Count; i++)
            {
                ReportItem item = reportItems[i];
                string reportName = reportNames[i];
                int workNumber = workNumbers[i];
                item.GetSelectedTasksAndFilesInformation(out List<int> selectedTasks, out List<FileInformation> filesInformation);
                Task.Run(() =>
                {
                    try
                    {
                        if (action == ReportAction.Generate)
                            item.GenerateReport(selectedTasks, filesInformation);
                        else if (action == ReportAction.Save)
                            item.SaveReport(selectedTasks, filesInformation);
                    }
                    catch (Exception)
                    {
                        List<int> list = Regex.IsMatch(reportName, "пр|Пр") ? failedPracticesReports : failedLaboratoriesReports;
                        list.Add(workNumber);
                    }
                    finally
                    {
                        completedCount++;
                    }
                });
            };
            Task.Run(() =>
            {
                do
                {
                }
                while (completedCount != reportItems.Count);

                if (failedLaboratoriesReports.Count == 0 && failedPracticesReports.Count == 0)
                {
                    if (MessageBox.Show($"Все отчеты успешно {actionName}!\nОткрыть папку с отчетами?", "Поздравляю!",
                    MessageBoxButton.YesNo, MessageBoxImage.Information, MessageBoxResult.No) == MessageBoxResult.Yes)
                    {
                        MainParams mainParams = new MainParams();
                        string path = action == ReportAction.Generate ? mainParams.ReportsPath : mainParams.SavedReportsPath;
                        if (Directory.Exists(path))
                            Process.Start(path.StartsWith(".") || path.StartsWith("/") ? Directory.GetCurrentDirectory() + path : path);
                        else
                            MessageBox.Show("Не получилось найти папку с отчетами!", "Ошибка!", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    }
                }
                else if (failedPracticesReports.Count == 0 && failedLaboratoriesReports.Count > 0)
                {
                    MessageBox.Show($"При {actionNameDuringAction} отчетов для лабораторных работ произошли ошибки, номера работ:\n" +
                        string.Join(" ", failedLaboratoriesReports.Take(10)) + (failedLaboratoriesReports.Count > 10 ? "..." : ""),
                        "Внимание!", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                }
                else if (failedPracticesReports.Count > 0 && failedLaboratoriesReports.Count == 0)
                {
                    MessageBox.Show($"При {actionNameDuringAction} отчетов для практических работ произошли ошибки, номера работ:\n" +
                        string.Join(" ", failedPracticesReports.Take(10)) + (failedPracticesReports.Count > 10 ? "..." : ""),
                        "Внимание!", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                }
                else
                {
                    MessageBox.Show($"При {actionNameDuringAction} отчетов произошли ошибки!\n" +
                        $"Практические работы:\n{string.Join(" ", failedPracticesReports.Take(10)) + (failedPracticesReports.Count > 10 ? "..." : "")}" +
                        $"\nЛабораторные работы:\n{string.Join(" ", failedLaboratoriesReports.Take(10)) + (failedLaboratoriesReports.Count > 10 ? "..." : "")}",
                        "Ошибка!", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            });

        }

        /// <summary>
        /// Пробует добавить картинку из буфера в работу
        /// </summary>
        private void TryAddImage()
        {
            if (Clipboard.ContainsImage() == false || SelectedIndex == null || SelectedIndex < 1)
                return;
            (TabItems[SelectedIndex ?? 0].Content as ReportItem).AddImageFromBuffer(Clipboard.GetImage());
        }

        public void OnPropertyChanged([CallerMemberName] string propertyName = "") => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
