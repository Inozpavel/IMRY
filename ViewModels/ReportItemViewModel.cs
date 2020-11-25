using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using WorkReportCreator.Models;
using WorkReportCreator.Views;
using WorkReportCreator.Views.CustomConrols;

namespace WorkReportCreator
{
    internal class ReportViewModel : INotifyPropertyChanged
    {
        #region Commands

        public Command AddFileInfo { get; private set; }

        public Command RemoveFileInfo { get; private set; }

        public Command SwapUpFileInfo { get; private set; }

        public Command SwapDownFileInfo { get; private set; }

        public Command ResetItem { get; private set; }

        #endregion

        private int? _selectedItemIndex;

        private Visibility _hintVisibility;

        private Visibility _dynamicTasksVisiblity;

        private string _dynamicTasksStatus;

        private ListBoxItem _selectedItem;

        #region Properties

        /// <summary>
        /// Индекс текущего выбранного элемента
        /// </summary>
        public int? SelectedItemIndex
        {
            get => _selectedItemIndex;
            set
            {
                _selectedItemIndex = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Видимость подсказки для перетаскивания элементов в область
        /// </summary>
        public Visibility HintVisibility
        {
            get => _hintVisibility;
            set
            {
                _hintVisibility = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Видимость блока с заданиями
        /// </summary>
        public Visibility DynamicTasksVisiblity
        {
            get => _dynamicTasksVisiblity;
            set
            {
                _dynamicTasksVisiblity = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Надпись, показывающая, выбрано ли задание или нет
        /// </summary>
        public string DynamicTasksStatus
        {
            get => _dynamicTasksStatus;
            set
            {
                _dynamicTasksStatus = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Текущий выбранный элемент
        /// </summary>
        public ListBoxItem SelectedItem
        {
            get => _selectedItem;
            set
            {
                if (_selectedItem != null)
                    (_selectedItem.Content as FileInformationItem).IsSelected = false;
                _selectedItem = value;
                if (_selectedItem != null)
                    (_selectedItem.Content as FileInformationItem).IsSelected = true;
            }
        }
        /// <summary>
        /// Список заданий
        /// </summary>
        public ObservableCollection<DynamicTaskItem> DynamicTasksArray { get; set; } = new ObservableCollection<DynamicTaskItem>();

        /// <summary>
        /// Список информации о файлах
        /// </summary>
        public ObservableCollection<ListBoxItem> FilesArray { get; set; } = new ObservableCollection<ListBoxItem>();

        #endregion

        public event PropertyChangedEventHandler PropertyChanged;

        /// <param name="DynamicTasks">Список заданий (при наличии)</param>
        public ReportViewModel(List<string> DynamicTasks, ReportModel report = null)
        {
            FilesArray.CollectionChanged += (sender, e) => HintVisibility = FilesArray.Count != 0 ? Visibility.Hidden : Visibility.Visible;

            AddFileInfo = new Command((sender) => AddNewFileInfo(), null);
            RemoveFileInfo = new Command(RemoveSelectedFileInfo, (sender) => SelectedItem != null);
            SwapUpFileInfo = new Command(SwapUpSelectedFileInfo, (sender) => _selectedItem != null && _selectedItemIndex != 0);
            SwapDownFileInfo = new Command(SwapDownSelectedFileInfo, (sender) => _selectedItem != null && _selectedItemIndex + 1 != FilesArray.Count);
            ResetItem = new Command(ShowDialogResetItem, null);

            for (int i = 0; i < DynamicTasks?.Count; i++)
            {
                DynamicTasksArray.Add(new DynamicTaskItem()
                {
                    Text = DynamicTasks[i],
                    IsChecked = report?.SelectedTasksIndices.Contains(i) ?? false,
                });
            }
            foreach (string task in DynamicTasks ?? new List<string>())

                DynamicTasksVisiblity = DynamicTasks.Count > 0 ? DynamicTasksVisiblity = Visibility.Visible : DynamicTasksVisiblity = Visibility.Collapsed;
            DynamicTasksStatus = DynamicTasks.Count == 0 ? "Заданий для выбора нет" : "Выберите, пожалуйста, задание";

            void UpdateTasksStatus(object sender) => DynamicTasksStatus = DynamicTasksArray
                .Any(x => x.IsChecked) ? "Задание выбрано" : "Выберите, пожалуйста, задание";

            foreach (var i in DynamicTasksArray)
                i.CheckedChanged += UpdateTasksStatus;
            if (report != null)
            {
                foreach (var fileItem in report.FilesAndDescriptions)
                    AddNewFileInfo(fileItem.Key, fileItem.Value);
            }
        }

        public void SaveReport(string reportName)
        {
            MainParams mainParams = new MainParams();
            string path = mainParams.SavedReportsPath;
            if (Directory.Exists(path) == false)
                Directory.CreateDirectory(path);

            List<int> indicies = new List<int>();
            for (int i = 0; i < DynamicTasksArray.Count; i++)
            {
                if (DynamicTasksArray[i].IsChecked)
                    indicies.Add(i);
            }

            ReportModel report = new ReportModel()
            {
                WorkNumber = int.Parse(Regex.Match(reportName, @"\d+").Value),
                WorkType = Regex.IsMatch(reportName, "пр|Пр") ? "Practice" : "Laboratory",
                FilesAndDescriptions = FilesArray.Select(x => x.Content as FileInformationItem).ToDictionary(x => x.FilePath, x => x.FileDescription),
                SelectedTasksIndices = indicies,
            };

            string text = JsonConvert.SerializeObject(report, Formatting.Indented);
            string shortName = string.IsNullOrEmpty(mainParams.ShortSubjectName) ? "" : "." + mainParams.ShortSubjectName;
            File.WriteAllText(mainParams.SavedReportsPath + $@"/{reportName}{shortName}.json", text);
        }

        /// <summary>
        /// Добавляет пустой элемент в список файлов
        /// </summary>
        public void AddNewFileInfo(string filePath = "", string fileDescription = "")
        {
            if (SelectedItemIndex != null)
            {
                FilesArray.Insert(SelectedItemIndex + 1 ?? 0, new ListBoxItem()
                {
                    Content = new FileInformationItem() { Number = FilesArray.Count + 1, FilePath = filePath, FileDescription = fileDescription },
                    HorizontalContentAlignment = HorizontalAlignment.Stretch
                });
                SelectedItemIndex += 1;
                UpdateAllNumbers();
            }
            else
            {
                FilesArray.Add(new ListBoxItem()
                {
                    Content = new FileInformationItem() { Number = FilesArray.Count + 1, FilePath = filePath, FileDescription = fileDescription },
                    HorizontalContentAlignment = HorizontalAlignment.Stretch
                });
                SelectedItemIndex = FilesArray.Count - 1;
            }
        }

        /// <summary>
        /// Добавляет новый элемент в список элементов с описанием файлов, с указанным файлом
        /// </summary>
        /// <param name="filePath">Путь до файла</param>
        public void AddNewFileInfoWithFilePath(string filePath)
        {
            if (File.Exists(filePath))
            {
                FilesArray.Add(new ListBoxItem()
                {
                    Content = new FileInformationItem() { Number = FilesArray.Count + 1, FilePath = filePath },
                    HorizontalContentAlignment = HorizontalAlignment.Stretch
                });
            }
        }

        /// <summary>
        /// Удаляет выбранный элемент из списка файлов
        /// </summary>
        public void RemoveSelectedFileInfo(object sender)
        {
            FileInformationItem reportMenuItem = _selectedItem.Content as FileInformationItem;

            if (((Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift)) == false) &&
                (string.IsNullOrEmpty(reportMenuItem.FileName) == false || string.IsNullOrEmpty(reportMenuItem.FileDescription) == false))
            {
                if (MessageBox.Show("В выбранном элементе имеются введеные данные!\nОни удалятся БЕЗ возможности восстановления!\nВы уверены?",
                    "Подтвердите действие", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No) == MessageBoxResult.No)
                    return;
            }

            int number = reportMenuItem.Number - 1;

            FilesArray.Remove(SelectedItem);
            UpdateAllNumbers();

            if (number > 0)
                SelectedItemIndex = number - 1;
        }

        /// <summary>
        /// Перемещает выбранный элемент выше
        /// </summary>
        public void SwapUpSelectedFileInfo(object sender) => SwapAdjacentItemWithSelected(-1);

        /// <summary>
        /// Перемещает выбранный элемент ниже
        /// </summary>
        public void SwapDownSelectedFileInfo(object sender) => SwapAdjacentItemWithSelected(+1);


        /// <summary>
        /// Обменивает ближайший элемент с выбранным
        /// </summary>
        /// <param name="i">1 - элемент снизу, -1 - элемент снизу</param>
        private void SwapAdjacentItemWithSelected(int i)
        {
            int number = (_selectedItem.Content as FileInformationItem).Number - 1;
            SwapArrayItems(number, number + i);
            SelectedItemIndex = number + i;
        }

        /// <summary>
        /// Обменивает два элемента в списке файлов с указанными индексами
        /// </summary>
        /// <param name="firstIndex">Индекс первого элемента</param>
        /// <param name="secondIndex">Индекс второго элемента</param>
        private void SwapArrayItems(int firstIndex, int secondIndex)
        {
            FileInformationItem temp = FilesArray[firstIndex].Content as FileInformationItem;
            FilesArray[firstIndex].Content = FilesArray[secondIndex].Content;
            FilesArray[secondIndex].Content = temp;

            UpdateAllNumbers();
        }

        /// <summary>
        /// Для каждого элемента в списке файлов перепросчитывет индекс
        /// </summary>
        private void UpdateAllNumbers()
        {
            for (int i = 0; i < FilesArray.Count; i++)
                (FilesArray[i].Content as FileInformationItem).Number = i + 1;
        }

        private void ShowDialogResetItem(object sender)
        {
            if (MessageBox.Show("В уверены, что хотите сбросить все?", "Подтвердите действие",
                MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No) == MessageBoxResult.Yes)
            {
                FilesArray.Clear();
                foreach (var task in DynamicTasksArray)
                {
                    task.IsChecked = false;
                }
            }
        }

        /// <summary>
        /// Cоздает отчет для работы
        /// </summary>
        /// <exception cref="Exception"/>
        public void GenerateReport(string reportName)
        {
            List<int> selected = new List<int>();
            for (int i = 0; i < DynamicTasksArray.Count; i++)
            {
                if (DynamicTasksArray[i].IsChecked)
                    selected.Add(i);
            }
            List<FileInformationItem> filesInformation = FilesArray.Select(x => x.Content as FileInformationItem).ToList();
            ReportGenerator reportGenerator = new ReportGenerator(reportName, selected, filesInformation);
            reportGenerator.GenerateReport();
        }

        public void OnPropertyChanged([CallerMemberName] string propertyName = "") => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
