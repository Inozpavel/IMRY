using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace WorkReportCreator.Models
{
    class ReportInformation : INotifyPropertyChanged
    {
        private string _name = "";

        private string _target = "";

        private string _theoryPart = "";

        private string _commonTask = "";

        private bool _hasDynamicTask;

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
        /// Есть ли задание для выбора
        /// </summary>
        public bool HasDynamicTask
        {
            get { return _hasDynamicTask; }
            set
            {
                _hasDynamicTask = value;
                OnPropertyChanged();
            }
        }

        #endregion

        public event PropertyChangedEventHandler PropertyChanged;

        public ReportInformation()
        {
            HasDynamicTask = false;
        }

        public void OnPropertyChanged([CallerMemberName] string propertyName = "") => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
