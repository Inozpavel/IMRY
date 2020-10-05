using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace WorkReportCreator
{
    public class StudentInformation : INotifyPropertyChanged
    {
        private string _secondName;
        private string _firstName;
        private string _middleName;
        private string _groupName;

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
        public string GroupName
        {
            get => _groupName;
            set
            {
                _groupName = value.ToUpper();
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
