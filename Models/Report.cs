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

        private string _conclusions = "";

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
            get => _target;
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
            get => _theoryPart;
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
            get => _commonTask;
            set
            {
                _commonTask = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Выводы для работы
        /// </summary>
        public string Conclusions
        {
            get => _conclusions;
            set
            {
                _conclusions = value;
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
