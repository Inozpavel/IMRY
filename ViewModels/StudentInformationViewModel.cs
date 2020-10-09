using Microsoft.Win32;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;

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

        public event PropertyChangedEventHandler PropertyChanged;

        public StudentInformationViewModel()
        {
            SaveStudentInfo = new Command(SaveStudent, null);
            LoadStudentInfo = new Command(LoadStudent, null);

            var globalParams = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText("./GlobalConfig.json"));
            LoadStudent("./" + globalParams["StandartUserDataFileName"] + ".json");
        }

        private void SaveStudent(object sender)
        {
            var globalParams = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText("./GlobalConfig.json"));
            SaveFileDialog dialog = new SaveFileDialog()
            {
                Title = "Сохранение информации о студенте",
                Filter = "JSON файлы (*.json)|*.json|Все файлы (*.*)|*.*",
                DefaultExt = "json",
                FileName = globalParams["StandartUserDataFileName"],
                AddExtension = true,
            };

            if (dialog.ShowDialog() == true)
            {
                File.WriteAllText(dialog.FileName, JsonConvert.SerializeObject(Student, Formatting.Indented));
            }
        }

        private bool LoadStudent(string filePath)
        {
            if (File.Exists(filePath) == false)
                return false;

            Student = JsonConvert.DeserializeObject<StudentInformation>(File.ReadAllText(filePath));
            if (_student.FirstName == null && _student.SecondName == null && _student.MiddleName == null && _student.Group == null)
                return false;

            return true;
        }

        private void LoadStudent(object sender)
        {
            var globalParams = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText("./GlobalConfig.json"));
            OpenFileDialog dialog = new OpenFileDialog()
            {
                Title = "Загрузка информации о студенте",
                Filter = "JSON файлы (*.json)|*.json|Все файлы (*.*)|*.*",
                DefaultExt = "json",
                FileName = globalParams["StandartUserDataFileName"],
            };

            if (dialog.ShowDialog() == true)
            {
                if (LoadStudent(dialog.FileName) == false)
                {
                    MessageBox.Show("Не получилось загрузить данные из файла", "Ошибка!", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                }
            }
        }

        public void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
