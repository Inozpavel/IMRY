using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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

        public ReportsPageViewModel(List<string> laboratoryWorks, List<string> practicalWorks, ReportsPage reportsPage)
        {
            TabItems.Add(new TabItem() { Header = "Быстрые действия", Content = new DafaultPagesItem() });
            foreach (var i in laboratoryWorks)
            {
                TabItems.Add(new TabItem() { Header = $"{i} лаб.", Content = new ReportView(reportsPage) });
            }

            foreach (var i in practicalWorks)
            {
                TabItems.Add(new TabItem() { Header = $"{i} пр.", Content = new ReportView(reportsPage) });
            }
            SelectedIndex = 0;
            OnPropertyChanged();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
