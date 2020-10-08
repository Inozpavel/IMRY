using Microsoft.Win32;
using Newtonsoft.Json;
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
                Title = "Сохранение информации о студенте",
                Filter = "JSON файлы (*.json)|*.json|Все файлы (*.*)|*.*",
                DefaultExt = "json",
                FileName = "StudentInformation",
                AddExtension = true,
            };

            if (dialog.ShowDialog() == true)
            {
                using (var file = new StreamWriter(dialog.FileName, append: false))
                {
                    file.Write(JsonConvert.SerializeObject(Student, Formatting.Indented));
                }
            }
        }

        private void LoadStudent(object sender)
        {
            OpenFileDialog dialog = new OpenFileDialog()
            {
                Title = "Загрузка информации о студенте",
                Filter = "JSON файлы (*.json)|*.json|Все файлы (*.*)|*.*",
                DefaultExt = "json",
                FileName = "StudentInformation.json",
            };

            if (dialog.ShowDialog() == true)
            {
                using (var file = new StreamReader(dialog.FileName))
                {
                    Student = JsonConvert.DeserializeObject<StudentInformation>(file.ReadToEnd());
                }
            }
        }

        public void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
