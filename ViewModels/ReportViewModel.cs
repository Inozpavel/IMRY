using System;
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
    class ReportViewModel : INotifyPropertyChanged
    {
        public Command AddFileInfo { get; private set; }

        public Command RemoveFileInfo { get; private set; }

        public Command SwapUpFileInfo { get; private set; }

        public Command SwapDownFileInfo { get; private set; }

        private int? _selectedFileInfoIndex;
        public int? SelectedFileInfoIndex
        {
            get => _selectedFileInfoIndex;
            set
            {
                _selectedFileInfoIndex = value;
                OnPropertyChanged();
            }
        }

        private Visibility _hintVisibility;

        public Visibility HintVisibility
        {
            get => _hintVisibility;
            set
            {
                _hintVisibility = value;
                OnPropertyChanged();
            }
        }

        private string _dynamicTasksStatus;

        public string DynamicTasksStatus
        {
            get => _dynamicTasksStatus;
            set
            {
                _dynamicTasksStatus = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<DynamicTaskItem> DynamicTasksArray { get; set; } = new ObservableCollection<DynamicTaskItem>();

        public ObservableCollection<ListBoxItem> Array { get; set; } = new ObservableCollection<ListBoxItem>();

        private ListBoxItem _selectedFileInfo;

        public ListBoxItem SelectedFileInfo
        {
            get => _selectedFileInfo;
            set
            {
                if (_selectedFileInfo != null)
                    (_selectedFileInfo.Content as ReportMenuItem).MarkAsNotSelected();
                _selectedFileInfo = value;
                if (_selectedFileInfo != null)
                    (_selectedFileInfo.Content as ReportMenuItem).MarkAsSelected();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public ReportViewModel()
        {
            Array.CollectionChanged += (sender, e) => HintVisibility = Array.Count != 0 ? Visibility.Hidden : Visibility.Visible;
            AddFileInfo = new Command(AddNewFileInfo, null);
            RemoveFileInfo = new Command(RemoveSelectedFileInfo, RemoveSelectedFileInfoCanExecute);
            SwapUpFileInfo = new Command(SwapUpSelectedFileInfo, SwapUpSelectedFileInfoCanExecute);
            SwapDownFileInfo = new Command(SwapDownSelectedFileInfo, SwapDownSelectedFileInfoCanExecute);
            DynamicTasksStatus = "Выберите, пожалуйста, задание";

            DynamicTasksArray.Add(new DynamicTaskItem() { Text = "первое задание", });
            DynamicTasksArray.Add(new DynamicTaskItem() { Text = "второе задание" });
            DynamicTasksArray.Add(new DynamicTaskItem() { Text = "третье задание" });
            DynamicTasksArray.Add(new DynamicTaskItem() { Text = "четвертое задание" });

            void UpdateTasksStatus(object sender) => DynamicTasksStatus = DynamicTasksArray
                .Any(x => x.IsChecked ?? false) ? "Задание выбрано" : "Выберите, пожалуйста, задание";

            foreach (var i in DynamicTasksArray)
                i.CheckedChanged += UpdateTasksStatus;
        }

        public void AddNewFileInfoWithFile(string filePath)
        {
            if (Regex.IsMatch(filePath, @"(\w+\.[\w]+)+$"))
            {
                Array.Add(new ListBoxItem()
                {
                    Content = new ReportMenuItem() { Number = Array.Count + 1, CodeFilePath = filePath },
                    HorizontalContentAlignment = HorizontalAlignment.Stretch
                });
            }
        }

        public void AddNewFileInfo(object sender)
        {
            if (SelectedFileInfoIndex != null)
            {
                Array.Insert(SelectedFileInfoIndex + 1 ?? 0, new ListBoxItem()
                {
                    Content = new ReportMenuItem() { Number = Array.Count + 1 },
                    HorizontalContentAlignment = HorizontalAlignment.Stretch
                });
                SelectedFileInfoIndex += 1;
                UpdateAllNumbers();
            }
            else
            {
                Array.Add(new ListBoxItem()
                {
                    Content = new ReportMenuItem() { Number = Array.Count + 1 },
                    HorizontalContentAlignment = HorizontalAlignment.Stretch
                });
                SelectedFileInfoIndex = Array.Count - 1;
            }
        }

        public void RemoveSelectedFileInfo(object fileInfo)
        {
            ReportMenuItem reportMenuItem = _selectedFileInfo.Content as ReportMenuItem;
            if (string.IsNullOrEmpty(reportMenuItem.FileName) == false || string.IsNullOrEmpty(reportMenuItem.CodeFileDescription) == false)
            {
                if (MessageBox.Show("В выбранном элементе имеются введеные данные!\nОни удалятся БЕЗ возможности восстановления!\nВы уверены?",
                    "Подтвердите действие", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No) == MessageBoxResult.No)
                    return;
            }

            int number = reportMenuItem.Number - 1;

            Array.Remove(SelectedFileInfo);
            UpdateAllNumbers();

            if (number > 0)
                SelectedFileInfoIndex = number - 1;
        }

        public bool RemoveSelectedFileInfoCanExecute(object fileInfo) => SelectedFileInfo != null;

        public void SwapUpSelectedFileInfo(object sender)
        {
            int number = (_selectedFileInfo.Content as ReportMenuItem).Number - 1;
            SwapArrayItems(number, number - 1);
            SelectedFileInfoIndex = number - 1;
            OnPropertyChanged();
        }

        public bool SwapUpSelectedFileInfoCanExecute(object sender) => _selectedFileInfo != null && _selectedFileInfoIndex != 0;

        public void SwapDownSelectedFileInfo(object sender)
        {
            int number = (_selectedFileInfo.Content as ReportMenuItem).Number - 1;
            SwapArrayItems(number, number + 1);
            SelectedFileInfoIndex = number + 1;
            OnPropertyChanged();
        }

        public bool SwapDownSelectedFileInfoCanExecute(object sender) => _selectedFileInfo != null && _selectedFileInfoIndex + 1 != Array.Count;

        public void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void SwapArrayItems(int firstIndex, int secondIndex)
        {
            ReportMenuItem temp = Array[firstIndex].Content as ReportMenuItem;
            Array[firstIndex].Content = Array[secondIndex].Content;
            Array[secondIndex].Content = temp;

            UpdateAllNumbers();
        }

        private void UpdateAllNumbers()
        {
            for (int i = 0; i < Array.Count; i++)
                (Array[i].Content as ReportMenuItem).Number = i + 1;
        }
    }
}
