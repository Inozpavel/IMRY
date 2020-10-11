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
    class ReportsPageViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<TabItem> TabItems { get; set; } = new ObservableCollection<TabItem>();

        private int? _selectedIndex;

        public int? SelectedIndex
        {
            get => _selectedIndex;
            set
            {
                _selectedIndex = value;
                OnPropertyChanged();
            }
        }
        public event Action<object> ButtonBackClicked;

        public event PropertyChangedEventHandler PropertyChanged;

        public ReportsPageViewModel(List<string> laboratoryWorks, List<string> practicalWorks, ReportsPage reportsPage)
        {
            DefaultPagesItem defaultPagesItem = new DefaultPagesItem();
            defaultPagesItem.ButtonBackClicked += (sender) => ButtonBackClicked?.Invoke(sender);
            defaultPagesItem.ButtonGenerateAllClicked += (sender) => TabItems.Where(item => item.Content is ReportView)
            .Select(x => x.Content as ReportView).ToList().ForEach(item => item.GenerateReport(item, null));

            TabItems.Add(new TabItem() { Header = "Быстрые действия", Content = defaultPagesItem });

            var globalParams = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText("./GlobalConfig.json"));
            var dynamicTasks = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, List<string>>>>(File.ReadAllText(globalParams["DynamicTasksFilePath"]));
            foreach (var i in laboratoryWorks)
            {
                TabItems.Add(new TabItem() { Header = $"{i} лаб.", Content = new ReportView(reportsPage, dynamicTasks["Laboratories"][i]) });
            }

            foreach (var i in practicalWorks)
            {
                TabItems.Add(new TabItem() { Header = $"{i} пр.", Content = new ReportView(reportsPage, dynamicTasks["Practises"][i]) });
            }
            SelectedIndex = 0;
            OnPropertyChanged();
        }

        public void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
