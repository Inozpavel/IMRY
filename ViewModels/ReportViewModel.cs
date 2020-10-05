using System.Collections.ObjectModel;
using WorkReportCreator.Views;

namespace WorkReportCreator
{
    class ReportViewModel
    {
        public Command AddFileInfo { get; private set; }

        public Command RemoveFileInfo { get; private set; }

        public ObservableCollection<ReportMenuItem> Array { get; set; } = new ObservableCollection<ReportMenuItem>();

        public ReportMenuItem SelectedFileInfo { get; set; }

        public ReportViewModel()
        {
            Array.Add(new ReportMenuItem());
            AddFileInfo = new Command(AddNewFileInfo, null);
            RemoveFileInfo = new Command(RemoveSelectedFileInfo, RemoveSelectedFileInfoCanExecute);
        }

        public void AddNewFileInfo(object parameter) => Array.Add(new ReportMenuItem());

        public void RemoveSelectedFileInfo(object fileInfo) => Array.Remove(SelectedFileInfo);

        public bool RemoveSelectedFileInfoCanExecute(object fileInfo) => SelectedFileInfo != null;
    }
}
