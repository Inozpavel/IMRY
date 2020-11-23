using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace WorkReportCreator
{
    public class Student : INotifyPropertyChanged
    {
        private string _secondName = "";

        private string _firstName = "";

        private string _middleName = "";

        private string _group = "";

        private bool _useFullName = false;

        /// <summary>
        /// Фамилия студента
        /// </summary>
        public string SecondName
        {
            get => _secondName;
            set
            {
                _secondName = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Имя студента
        /// </summary>
        public string FirstName
        {
            get => _firstName;
            set
            {
                _firstName = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Отчество студента
        /// </summary>
        public string MiddleName
        {
            get => _middleName;
            set
            {
                _middleName = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Группа студента
        /// </summary>
        public string Group
        {
            get => _group;
            set
            {
                _group = value.ToUpper();
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Использовать полное имя или фамилию + инициалы
        /// </summary>
        public bool UseFullName
        {
            get => _useFullName;
            set
            {
                _useFullName = value;
                OnPropertyChanged();
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged([CallerMemberName] string propertyName = "") => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
