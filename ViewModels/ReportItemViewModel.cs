﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
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

        public Command TrySaveImage { get; private set; }

        #endregion

        private string _commonTask;

        private string _saveStatus;

        private int _selectedItemIndex = 0;

        private Visibility _hintVisibility;

        private Visibility _dynamicTasksVisiblity;

        private string _dynamicTasksStatus;

        private ListBoxItem _selectedItem;

        private readonly string _reportName;

        private string _filePath;

        private static readonly object locker = new object();

        #region Properties

        public string CommonTask
        {
            get => _commonTask;
            private set
            {
                _commonTask = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Путь до файла, где сохраняется отчет
        /// </summary>
        public string FilePath
        {
            get => _filePath;
            set
            {
                _filePath = value;
                SaveStatus = string.IsNullOrEmpty(value) ? "Сохранить данные" : "Автосохранение включено";
            }
        }

        /// <summary>
        /// Надпись Сохранить данные / Автосохранение включено
        /// </summary>
        public string SaveStatus
        {
            get => _saveStatus;
            set
            {
                _saveStatus = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Индекс текущего выбранного элемента
        /// </summary>
        public int SelectedItemIndex
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
        public ReportViewModel(string commonTask, List<string> DynamicTasks, ReportModel report)
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
                    Margin = new Thickness(0, 6, 0, 0)
                });
            }
            FilePath = report.FilePath;
            _reportName = report.ReportName;
            CommonTask = commonTask;
            DynamicTasksVisiblity = DynamicTasks.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
            DynamicTasksStatus = DynamicTasks.Count == 0 ? "Заданий для выбора нет" : "Выберите, пожалуйста, задание";

            void UpdateTasksStatus(object sender) => DynamicTasksStatus = DynamicTasksArray
                .Any(x => x.IsChecked) ? "Задание выбрано" : "Выберите, пожалуйста, задание";

            foreach (var i in DynamicTasksArray)
                i.IsCheckedChanged += UpdateTasksStatus;
            if (report != null)
            {
                foreach (var fileItem in report.FilesAndDescriptions)
                    AddNewFileInfo(fileItem.Key, fileItem.Value);
            }

            FilesArray.CollectionChanged += (sender, e) => Autosave();
            DynamicTasksArray.CollectionChanged += (sender, e) => Autosave();
            DynamicTasksArray.ToList().ForEach(x => x.IsCheckedChanged += (sender) => Autosave());
        }

        public void AddImageFromBuffer(BitmapSource imageSource)
        {
            MainParams mainParams = new MainParams();
            string folder = mainParams.SavedReportsPath + "/TempClipboardImages";
            if (Directory.Exists(folder) == false)
                Directory.CreateDirectory(folder);
            try
            {
                PngBitmapEncoder encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(imageSource));

                string reportName = _reportName.TrimEnd('.');
                int imagesCount = Directory.GetFiles(mainParams.SavedReportsPath, $"*{reportName}_*.png").Length;
                string time = DateTime.Now.ToString("ddMMyyyy_HH_mm_ss_fff");
                string path = folder + $"/{reportName}-{imagesCount + 1}_{time}.png";
                using var fileStream = new FileStream(path, FileMode.Create);
                encoder.Save(fileStream);
                AddNewFileInfoWithFilePath(path);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Произошла ошибка!", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }

        /// <summary>
        /// Добавляет пустой элемент в список файлов
        /// </summary>
        public void AddNewFileInfo(string filePath = "", string fileDescription = "")
        {
            FileInformationItem item = CreateFileInformationItem(FilesArray.Count + 1, filePath, fileDescription);

            if (SelectedItemIndex != -1)
            {
                AddFileInformationItem(item, SelectedItemIndex);
                SelectedItemIndex += 1;
                UpdateAllNumbers();
            }
            else
            {
                AddFileInformationItem(item);
                SelectedItemIndex = FilesArray.Count - 1;
            }
        }

        /// <summary>
        /// Добавляет новый элемент в список элементов с описанием файлов, с указанным файлом
        /// </summary>
        /// <param name="filePath">Путь до файла</param>
        public void AddNewFileInfoWithFilePath(string filePath)
        {
            if (string.IsNullOrEmpty(filePath) == false)
            {
                AddFileInformationItem(CreateFileInformationItem(FilesArray.Count + 1, filePath));
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
        /// Cоздает отчет для работы
        /// </summary>
        public void GenerateReport(List<int> selectedIndicies, List<FileInformation> filesInformation)
        {
            ReportGenerator reportGenerator = new ReportGenerator(_reportName, selectedIndicies, filesInformation);
            reportGenerator.GenerateReport();
        }

        /// <summary>
        /// Сохраняет отчет для работы
        /// </summary>
        public void SaveReport(List<int> selectedIndicies, List<FileInformation> filesInformation)
        {
            Dictionary<string, string> filesAndDescriptions = filesInformation.Where(x => string.IsNullOrEmpty(x.FilePath) == false).
                ToDictionary(x => x.FilePath, x => x.FileDescription);

            Task.Run(() =>
            {
                MainParams mainParams = new MainParams();
                if (Directory.Exists(mainParams.SavedReportsPath) == false)
                    Directory.CreateDirectory(mainParams.SavedReportsPath);

                ReportModel report = new ReportModel()
                {
                    WorkNumber = int.Parse(Regex.Match(_reportName, @"\d+").Value),
                    WorkType = Regex.IsMatch(_reportName, "пр|Пр") ? "Practice" : "Laboratory",
                    FilesAndDescriptions = filesAndDescriptions,
                    SelectedTasksIndices = selectedIndicies,
                };
                string text = JsonConvert.SerializeObject(report, Formatting.Indented);
                string shortName = string.IsNullOrEmpty(mainParams.ShortSubjectName) ? "" : "." + mainParams.ShortSubjectName;
                string path = FilePath = mainParams.SavedReportsPath + $@"/{_reportName.TrimEnd('.')}{shortName}.json";
                lock (locker)
                {
                    File.WriteAllText(path, text);
                    FilePath = path;
                }
            });
        }

        public void GetSelectedTasksAndFilesInformation(out List<int> selectedTasks, out List<FileInformation> filesInformation)
        {
            selectedTasks = new List<int>();
            for (int i = 0; i < DynamicTasksArray.Count; i++)
            {
                if (DynamicTasksArray[i].IsChecked)
                    selectedTasks.Add(i);
            }
            filesInformation = FilesArray.Select(x => x.Content as FileInformationItem)
                .Select(x => new FileInformation()
                {
                    FilePath = x.FilePath,
                    FileName = x.FileName,
                    FileDescription = x.FileDescription,
                }).ToList();
        }

        /// <summary>
        /// Проверяет, указан ли файл для сохранения, если файл указан, сохраняет данные
        /// </summary>
        private void Autosave()
        {
            try
            {
                GetSelectedTasksAndFilesInformation(out List<int> selectedTasks, out List<FileInformation> filesInformation);
                SaveReport(selectedTasks, filesInformation);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "При автосохранении работы произошла ошибка!", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Добавляет элемент в список элемента с файлами
        /// </summary>
        /// <param name="item">Сам элемент</param>
        /// <param name="index">Индекс, куда нужно вставить, по умолчанию - в конец</param>
        private void AddFileInformationItem(FileInformationItem item, int? index = null)
        {
            ListBoxItem listBoxItem = new ListBoxItem()
            {
                Content = item,
                HorizontalContentAlignment = HorizontalAlignment.Stretch
            };
            if (index == null)
                FilesArray.Add(listBoxItem);
            else
                FilesArray.Insert(index ?? 0 + 1, listBoxItem);
        }

        /// <summary>
        /// Обменивает ближайший элемент с выбранным
        /// </summary>
        /// <param name="i">1 - элемент снизу, -1 - элемент снизу</param>
        private void SwapAdjacentItemWithSelected(int i)
        {
            int number = (_selectedItem.Content as FileInformationItem).Number - 1;
            SwapArrayItems(number, number + i);
            SelectedItemIndex = number + i;
            Autosave();
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

        /// <summary>
        /// Показывает диалог для подтверждения сброса всего введенного в элемент
        /// </summary>
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
        /// Создает новый элемент с указанными номером, путем и описанием
        /// </summary>
        /// <param name="number">Номер элемента</param>
        /// <param name="filePath">Путь до файла пользователя</param>
        /// <param name="fileDescription">Описание файла</param>
        /// <returns>Элемент с описанием файла</returns>
        private FileInformationItem CreateFileInformationItem(int number, string filePath = "", string fileDescription = "")
        {
            FileInformationItem item = new FileInformationItem()
            {
                Number = number,
                FilePath = filePath,
                FileDescription = fileDescription
            };
            item.PropertyToSaveChanged += (sender, e) => Autosave();
            return item;
        }

        public void OnPropertyChanged([CallerMemberName] string propertyName = "") => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
