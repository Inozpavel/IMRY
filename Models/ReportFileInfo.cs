using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace WorkReportCreator
{
    class ReportFileInfo : INotifyPropertyChanged
    {
        private string _description;
        private string _filePath;

        /// <summary>
        /// Описание файла с кодом
        /// </summary>
        public string Description
        {
            get => _description;
            set
            {
                _description = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Путь до файла с кодом
        /// </summary>
        public string FilePath
        {
            get => _filePath;
            set
            {
                _filePath = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
