using System.Collections.ObjectModel;

namespace WorkReportCreator
{
    class ReportViewModel
    {
        public Command AddFileInfo { get; private set; }

        public Command RemoveFileInfo { get; private set; }

        public ObservableCollection<ReportFileInfo> Array { get; set; } = new ObservableCollection<ReportFileInfo>();

        public ReportFileInfo SelectedFileInfo { get; set; }

        public ReportViewModel()
        {
            AddFileInfo = new Command(AddNewFileInfo, null);
            RemoveFileInfo = new Command(RemoveSelectedFileInfo, RemoveSelectedFileInfoCanExecute);
        }

        public void AddNewFileInfo(object parameter) => Array.Add(new ReportFileInfo());

        public void RemoveSelectedFileInfo(object fileInfo) => Array.Remove(SelectedFileInfo);

        public bool RemoveSelectedFileInfoCanExecute(object fileInfo) => SelectedFileInfo != null;
    }
}
