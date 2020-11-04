using System.ComponentModel;

namespace WorkReportCreator.Models
{
    public class DynamicTask : INotifyPropertyChanged
    {

        public event PropertyChangedEventHandler PropertyChanged;

        private string _description = "";

        /// <summary>
        /// Описание работы
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

        private void OnPropertyChanged(string propertyName = "") => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    }
}
