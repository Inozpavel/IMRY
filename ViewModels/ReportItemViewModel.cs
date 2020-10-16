using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
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

        public Command HideDynamicTasks { get; private set; }

        public Command ShowDynamicTasks { get; private set; }

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
        public ReportViewModel(List<string> DynamicTasks)
        {
            FilesArray.CollectionChanged += (sender, e) => HintVisibility = FilesArray.Count != 0 ? Visibility.Hidden : Visibility.Visible;

            AddFileInfo = new Command(AddNewFileInfo, null);
            RemoveFileInfo = new Command(RemoveSelectedFileInfo, RemoveSelectedFileInfoCanExecute);
            SwapUpFileInfo = new Command(SwapUpSelectedFileInfo, SwapUpSelectedFileInfoCanExecute);
            SwapDownFileInfo = new Command(SwapDownSelectedFileInfo, SwapDownSelectedFileInfoCanExecute);
            ShowDynamicTasks = new Command((sender) => DynamicTasksVisiblity = Visibility.Visible, null);
            HideDynamicTasks = new Command((sender) => DynamicTasksVisiblity = Visibility.Collapsed, null);

            foreach (string task in DynamicTasks ?? new List<string>())
            {
                DynamicTasksArray.Add(new DynamicTaskItem() { Text = task, });
            }
            DynamicTasksStatus = "Выберите, пожалуйста, задание";

            void UpdateTasksStatus(object sender) => DynamicTasksStatus = DynamicTasksArray
                .Any(x => x.IsChecked) ? "Задание выбрано" : "Выберите, пожалуйста, задание";

            foreach (var i in DynamicTasksArray)
                i.CheckedChanged += UpdateTasksStatus;
        }

        /// <summary>
        /// Добавляет пустой элемент в список файлов
        /// </summary>
        public void AddNewFileInfo(object sender)
        {
            if (SelectedItemIndex != null)
            {
                FilesArray.Insert(SelectedItemIndex + 1 ?? 0, new ListBoxItem()
                {
                    Content = new FileInformationItem() { Number = FilesArray.Count + 1 },
                    HorizontalContentAlignment = HorizontalAlignment.Stretch
                });
                SelectedItemIndex += 1;
                UpdateAllNumbers();
            }
            else
            {
                FilesArray.Add(new ListBoxItem()
                {
                    Content = new FileInformationItem() { Number = FilesArray.Count + 1 },
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
            if (Regex.IsMatch(filePath, @"(\w+\.[\w]+)+$"))
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
            if (string.IsNullOrEmpty(reportMenuItem.FileName) == false || string.IsNullOrEmpty(reportMenuItem.FileDescription) == false)
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
        /// Проверяет, можно ли удалить выбранный элемент
        /// </summary>
        /// <returns><paramref name="True"/> если может удалить, в противное случает <paramref name="false"/></returns>
        public bool RemoveSelectedFileInfoCanExecute(object fileInfo) => SelectedItem != null;

        /// <summary>
        /// Перемещает выбранный элемент выше
        /// </summary>
        public void SwapUpSelectedFileInfo(object sender)
        {
            int number = (_selectedItem.Content as FileInformationItem).Number - 1;
            SwapArrayItems(number, number - 1);
            SelectedItemIndex = number - 1;
            OnPropertyChanged();
        }

        /// <summary>
        /// Проверяет, можно ли переместить выше выбраннный элемент
        /// </summary>
        /// <returns><paramref name="True"/> если может переместить, в противное случает <paramref name="false"/></returns>
        public bool SwapUpSelectedFileInfoCanExecute(object sender) => _selectedItem != null && _selectedItemIndex != 0;

        /// <summary>
        /// Перемещает выбранный элемент ниже
        /// </summary>
        public void SwapDownSelectedFileInfo(object sender)
        {
            int number = (_selectedItem.Content as FileInformationItem).Number - 1;
            SwapArrayItems(number, number + 1);
            SelectedItemIndex = number + 1;
            OnPropertyChanged();
        }

        /// <summary>
        /// Проверяет, можно ли переместить ниже выбранный элемент
        /// </summary>
        /// <returns><paramref name="True"/> если может переместить, в противное случает <paramref name="false"/></returns>
        public bool SwapDownSelectedFileInfoCanExecute(object sender) => _selectedItem != null && _selectedItemIndex + 1 != FilesArray.Count;

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

        public void OnPropertyChanged([CallerMemberName] string propertyName = "") => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
