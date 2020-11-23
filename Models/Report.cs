using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace WorkReportCreator.Models
{
    public class Report : INotifyPropertyChanged
    {
        private string _name = "";

        private string _target = "";

        private string _theoryPart = "";

        private string _commonTask = "";

        private ObservableCollection<DynamicTask> _dynamicTasks = new ObservableCollection<DynamicTask>();

        #region Properties

        /// <summary>
        /// Название работы
        /// </summary>
        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Цель работы
        /// </summary>
        public string WorkTarget
        {
            get { return _target; }
            set
            {
                _target = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Теоритическая часть
        /// </summary>
        public string TheoryPart
        {
            get { return _theoryPart; }
            set
            {
                _theoryPart = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Общее описание задачи
        /// </summary>
        public string CommonTask
        {
            get { return _commonTask; }
            set
            {
                _commonTask = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Список заданий для выбора
        /// </summary>
        public ObservableCollection<DynamicTask> DynamicTasks
        {
            get => _dynamicTasks;
            set
            {
                _dynamicTasks = value;
                OnPropertyChanged();
            }
        }

        #endregion

        public event PropertyChangedEventHandler PropertyChanged;

        public Report() => _dynamicTasks.CollectionChanged += (sender, e) => OnPropertyChanged();

        public void OnPropertyChanged([CallerMemberName] string propertyName = "") => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
