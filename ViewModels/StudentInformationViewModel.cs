using Microsoft.Win32;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Xml.Serialization;

namespace WorkReportCreator.ViewModels.Commands
{
    class StudentInformationViewModel : INotifyPropertyChanged
    {
        private StudentInformation _student = new StudentInformation();

        public StudentInformation Student
        {
            get => _student;
            set
            {
                _student = value;
                OnPropertyChanged();
            }
        }

        public Command SaveStudentInfo { get; private set; }

        public Command LoadStudentInfo { get; private set; }

        private readonly XmlSerializer _xmlSerializer = new XmlSerializer(typeof(StudentInformation));

        public event PropertyChangedEventHandler PropertyChanged;

        public StudentInformationViewModel()
        {
            SaveStudentInfo = new Command(SaveStudent, null);
            LoadStudentInfo = new Command(LoadStudent, null);
        }

        private void SaveStudent(object sender)
        {
            SaveFileDialog dialog = new SaveFileDialog()
            {
                Title = "Загрузка информации о студенте",
                Filter = "Xml файлы(*.xml)|*.xml|Все файлы (*.*)|*.*",
                DefaultExt = "xml",
                FileName = "StudentInformation"
            };

            if (dialog.ShowDialog() == true)
            {
                _xmlSerializer.Serialize(new FileStream(dialog.FileName, FileMode.Create), Student);
            }
        }

        private void LoadStudent(object sender)
        {
            OpenFileDialog dialog = new OpenFileDialog()
            {
                Title = "Сохранение инфорации о студенте",
                Filter = "Xml файлы(*.xml)|*.xml|Все файлы (*.*)|*.*",
                DefaultExt = "xml",
            };

            if (dialog.ShowDialog() == true)
            {
                Student = _xmlSerializer.Deserialize(new FileStream(dialog.FileName, FileMode.Open)) as StudentInformation ?? new StudentInformation();
            }
        }

        public void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
