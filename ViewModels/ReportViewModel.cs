using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using WorkReportCreator.Views;

namespace WorkReportCreator
{
    class ReportViewModel : INotifyPropertyChanged
    {
        public Command AddFileInfo { get; private set; }

        public Command RemoveFileInfo { get; private set; }

        public Command SwapUpFileInfo { get; private set; }

        public Command SwapDownFileInfo { get; private set; }

        private int? _selectedFileInfoIndex;
        public int? FileInfoSelectedIndex
        {
            get => _selectedFileInfoIndex;
            set
            {
                _selectedFileInfoIndex = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<ReportMenuItem> Array { get; set; } = new ObservableCollection<ReportMenuItem>();

        private ReportMenuItem _selectedFileInfo;

        public event PropertyChangedEventHandler PropertyChanged;

        public ReportMenuItem SelectedFileInfo
        {
            get => _selectedFileInfo;
            set
            {
                if (_selectedFileInfo != null)
                    _selectedFileInfo.IsSelected = false;

                _selectedFileInfo = value;

                if (value != null)
                    value.IsSelected = true;
            }
        }

        public ReportViewModel()
        {
            Array.Add(new ReportMenuItem() { Number = 1 });
            AddFileInfo = new Command(AddNewFileInfo, null);
            RemoveFileInfo = new Command(RemoveSelectedFileInfo, RemoveSelectedFileInfoCanExecute);
            SwapUpFileInfo = new Command(SwapUpSelectedFileInfo, SwapUpSelectedFileInfoCanExecute);
            SwapDownFileInfo = new Command(SwapDownSelectedFileInfo, SwapDownSelectedFileInfoCanExecute);
        }

        public void AddNewFileInfo(object sender) => Array.Add(new ReportMenuItem() { Number = Array.Count + 1 });

        public void RemoveSelectedFileInfo(object fileInfo) => Array.Remove(SelectedFileInfo);

        public bool RemoveSelectedFileInfoCanExecute(object fileInfo) => SelectedFileInfo != null;

        public void SwapUpSelectedFileInfo(object sender)
        {
            int number = _selectedFileInfo.Number - 1;

            var temp = Array[number - 1];
            Array[number - 1] = Array[number];
            Array[number] = temp;

            int tempNumber = Array[number - 1].Number;
            Array[number - 1].Number = Array[number].Number;
            Array[number].Number = tempNumber;

            FileInfoSelectedIndex = number - 1;

            OnPropertyChanged();
        }

        public bool SwapUpSelectedFileInfoCanExecute(object sender) => _selectedFileInfo != null && _selectedFileInfoIndex != 0;

        public void SwapDownSelectedFileInfo(object sender)
        {
            int number = _selectedFileInfo.Number - 1;

            var temp = Array[number + 1];
            Array[number + 1] = Array[number];
            Array[number] = temp;

            int tempNumber = Array[number + 1].Number;
            Array[number + 1].Number = Array[number].Number;
            Array[number].Number = tempNumber;

            FileInfoSelectedIndex = number + 1;
            OnPropertyChanged();
        }
        public bool SwapDownSelectedFileInfoCanExecute(object sender) => _selectedFileInfo != null && _selectedFileInfoIndex + 1 != Array.Count;

        public void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
