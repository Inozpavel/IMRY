using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Controls;
using WorkReportCreator.Views;

namespace WorkReportCreator.ViewModels
{
    class ReportsWindowViewModel : INotifyPropertyChanged
    {
        private int? _selectedIndex;

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
        public ReportsWindowViewModel(List<string> laboratoryWorks, List<string> practicalWorks, ReportsWindow reportsWindow)
        {
            FastActions defaultPagesItem = new FastActions();
            defaultPagesItem.ButtonBackClicked += (sender) => ButtonBackClicked?.Invoke(sender);
            defaultPagesItem.ButtonGenerateAllClicked += (sender) => TabItems.Where(item => item.Content is ReportItem)
            .Select(x => x.Content as ReportItem).ToList().ForEach(item => item.GenerateReport(item, null));

            TabItems.Add(new TabItem() { Header = "Быстрые действия", Content = defaultPagesItem });

            var globalParams = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText("./GlobalConfig.json"));
            var dynamicTasks = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, List<string>>>>(File.ReadAllText(globalParams["DynamicTasksFilePath"]));

            foreach (var i in laboratoryWorks)
            {
                TabItems.Add(new TabItem() { Header = $"{i} лаб.", Content = new ReportItem(reportsWindow, dynamicTasks["Laboratories"][i]) });
            }

            foreach (var i in practicalWorks)
            {
                TabItems.Add(new TabItem() { Header = $"{i} пр.", Content = new ReportItem(reportsWindow, dynamicTasks["Practises"][i]) });
            }
            SelectedIndex = 0;
            OnPropertyChanged();
        }

        public void OnPropertyChanged([CallerMemberName] string propertyName = "") => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
