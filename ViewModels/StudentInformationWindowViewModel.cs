using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls.Primitives;
using WorkReportCreator.Models;

namespace WorkReportCreator.ViewModels.Commands
{
    internal class StudentInformationWindowViewModel : INotifyPropertyChanged
    {
        private readonly WorksAndStudentInfoWindow _worksAndStudentInfoWindow;

        private Visibility _labaratoriesVisibility;

        private Visibility _practicesVisibility;

        private Visibility _worksSelectVisibility;

        private StudentInformation _student;

        private string _saveStatus;

        public string SaveStatus
        {
            get => _saveStatus;
            set
            {
                _saveStatus = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// От значения зависит, все кнопки с практическими работами будут отмечены / не отмечены
        /// </summary>
        private bool _shouldCheckAllPracticalWorks = false;

        /// <summary>
        /// От значения зависит, все кнопки с лабораторными работами будут отмечены / не отмечены
        /// </summary>
        private bool _shouldCheckAllLaboratoryWork = false;

        #region Commands

        public Command SaveStudentInfo { get; private set; }

        public Command LoadStudentInfo { get; private set; }

        public Command CheckAllPracticalButtons { get; private set; }

        public Command CheckAllLaboratoryButtons { get; private set; }

        public Command ShowReportsPage { get; private set; }

        #endregion

        #region Properties

        /// <summary>
        /// Видимость блока с выбором практических работ
        /// </summary>
        public Visibility PracticesVisibility
        {
            get => _practicesVisibility;
            set
            {
                _practicesVisibility = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Видимость блока с выбором лабораторных работ
        /// </summary>
        public Visibility LaboratoriesVisibility
        {
            get => _labaratoriesVisibility;
            set
            {
                _labaratoriesVisibility = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Видимость всего блока с выбором работ
        /// </summary>
        public Visibility WorksSelectVisibility
        {
            get => _worksSelectVisibility;
            set
            {
                _worksSelectVisibility = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Список всех кнопок для выбора номеров практических работ
        /// </summary>
        public List<ToggleButton> PracticalWorksButtons { get; set; } = new List<ToggleButton>();

        /// <summary>
        /// Список всех кнопок для выбора номеров лабораторных работ
        /// </summary>
        public List<ToggleButton> LaboratoryWorksButtons { get; set; } = new List<ToggleButton>();

        /// <summary>
        /// Текущая информация о студенте
        /// </summary>
        public StudentInformation Student
        {
            get => _student;
            set
            {
                _student = value;
                OnPropertyChanged();
            }
        }

        #endregion

        public event PropertyChangedEventHandler PropertyChanged;

        /// <param name="window">Окно с вводом информации о студенте и выбором работ</param>
        /// <exception cref="Exception"/>
        public StudentInformationWindowViewModel(WorksAndStudentInfoWindow window)
        {
            _student = new StudentInformation();
            _student.PropertyChanged += SaveStudentInformation;
            _worksAndStudentInfoWindow = window;
            SaveStudentInfo = new Command(ShowDialogSaveStudent, null);
            LoadStudentInfo = new Command(LoadStudent, null);
            CheckAllLaboratoryButtons = new Command((sender) => CheckAllButtons(LaboratoryWorksButtons, ref _shouldCheckAllLaboratoryWork), null);
            CheckAllPracticalButtons = new Command((sender) => CheckAllButtons(PracticalWorksButtons, ref _shouldCheckAllPracticalWorks), null);
            ShowReportsPage = new Command(LoadReportsPage, null);

            MainParams mainParams = new MainParams();
            LoadStudent(mainParams.UserDataFileName);

            ResourceDictionary dictionaryWithTiltedButton = new ResourceDictionary() { Source = new Uri("/Views/Styles/TiltedButtonStyle.xaml", UriKind.Relative) };
            ResourceDictionary dictionaryWithNumberToggleButton = new ResourceDictionary() { Source = new Uri("/Views/Styles/NumberToggleButtonStyle.xaml", UriKind.Relative) };

            ToggleButton checkAllPracticalButton = new ToggleButton() { Content = "Все работы", Style = dictionaryWithTiltedButton["TiltedButton"] as Style, Command = CheckAllPracticalButtons, Margin = new Thickness(10) };
            ToggleButton checkAllLaboratoriesButton = new ToggleButton() { Content = "Все работы", Style = dictionaryWithTiltedButton["TiltedButton"] as Style, Command = CheckAllLaboratoryButtons, Margin = new Thickness(10) };

            PracticalWorksButtons.Add(checkAllPracticalButton);
            LaboratoryWorksButtons.Add(checkAllLaboratoriesButton);

            try
            {
                var template = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, ReportInformation>>>(File.ReadAllText(mainParams.CurrentTemplateFilePath));
                foreach (string type in template.Keys.Distinct())
                {
                    foreach (string workNumber in template[type].Keys.Distinct())
                    {
                        ToggleButton button = new ToggleButton() { Content = workNumber, Style = dictionaryWithNumberToggleButton["NumberToggleButton"] as Style };
                        if (type == "Practices")
                        {
                            PracticalWorksButtons.Add(button);
                        }
                        else if (type == "Laboratories")
                        {
                            LaboratoryWorksButtons.Add(button);
                        }
                    }
                }
            }
            catch (Exception)
            {
                MessageBox.Show("Не удалось загрузить номера работ из шаблона!\nПроверьте коррекность пути!", "Ошибка!", MessageBoxButton.OK, MessageBoxImage.Error);
                throw new Exception();
            }
            PracticesVisibility = PracticalWorksButtons.Count > 1 ? Visibility.Visible : Visibility.Collapsed;
            LaboratoriesVisibility = LaboratoryWorksButtons.Count > 1 ? Visibility.Visible : Visibility.Collapsed;
            WorksSelectVisibility = PracticalWorksButtons.Count > 1 || LaboratoryWorksButtons.Count > 1 ? Visibility.Visible : Visibility.Collapsed;
            SaveStatus = string.IsNullOrEmpty(mainParams.UserDataFileName) ?  "Сохранить информацию о студенте": "Автосохранение включено";
        }

        private void SaveStudentInformation(object sender, PropertyChangedEventArgs e)
        {
            MainParams mainParams = new MainParams();
            if (File.Exists(mainParams.UserDataFileName))
            {
                try
                {
                    File.WriteAllText(mainParams.UserDataFileName, JsonConvert.SerializeObject(Student, Formatting.Indented));
                    mainParams.UserDataFileName = mainParams.UserDataFileName;
                }
                catch (UnauthorizedAccessException)
                {
                    if (MessageBox.Show("У файла установлен атрибут \"Только чтение\"\nНе получилось перезаписать его!\nСнять с него этот атрибут?",
                        "Ошибка", MessageBoxButton.YesNo, MessageBoxImage.Error, MessageBoxResult.No) == MessageBoxResult.Yes)
                    {
                        File.SetAttributes(mainParams.UserDataFileName, FileAttributes.Normal);
                        File.WriteAllText(mainParams.UserDataFileName, JsonConvert.SerializeObject(Student, Formatting.Indented));
                    }
                    else
                    {
                        MessageBox.Show("Не получилось сохранить информацию!", "Ошибка!", MessageBoxButton.OK, MessageBoxImage.Error);
                        mainParams.UserDataFileName = "";
                        SaveStatus = "Сохранить информацию о студенте";
                    }
                }
            }
        }

        /// <summary>
        /// Создает окно с работами и показывает его
        /// </summary>
        private void LoadReportsPage(object sender)
        {
            List<string> selectedLaboratoryWorks = LaboratoryWorksButtons.Where(x => x.IsChecked ?? false).Select(x => x.Content as string).ToList();
            List<string> selectedPracticalWorks = PracticalWorksButtons.Where(x => x.IsChecked ?? false).Select(x => x.Content as string).ToList();
            try
            {
                ReportsWindow reportsPage = new ReportsWindow(_worksAndStudentInfoWindow, selectedLaboratoryWorks, selectedPracticalWorks, Student);
                _worksAndStudentInfoWindow.Hide();
                reportsPage.Show();
            }
            catch (Exception)
            {
            }
        }

        /// <summary>
        /// Отмечает / снимает отметки со всех кнопок с практическими / лабораторными работами
        /// </summary>
        private void CheckAllButtons(List<ToggleButton> buttons, ref bool oldValue)
        {
            bool newValue = !oldValue;
            buttons.ForEach(x => x.IsChecked = newValue);
            oldValue = newValue;
        }

        /// <summary>
        /// Сохраняет информацию о студенте в выбранном файле
        /// </summary>
        private void ShowDialogSaveStudent(object sender)
        {

            SaveFileDialog dialog = new SaveFileDialog()
            {
                Title = "Сохранение информации о студенте",
                Filter = "JSON файлы (*.json)|*.json|Все файлы (*.*)|*.*",
                DefaultExt = "json",
                AddExtension = true,
            };

            if (dialog.ShowDialog() == true)
            {
                MainParams mainParams = new MainParams
                {
                    UserDataFileName = dialog.FileName
                };
            }
        }

        /// <summary>
        /// Загружает информацию о студенте из указанного файла
        /// </summary>
        private bool LoadStudent(string filePath)
        {
            if (File.Exists(filePath) == false)
                return false;
            try
            {
                Student = JsonConvert.DeserializeObject<StudentInformation>(File.ReadAllText(filePath));
                Student.PropertyChanged += SaveStudentInformation;
                if (_student.FirstName == null && _student.SecondName == null && _student.MiddleName == null && _student.Group == null)
                    return false;
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Загружает информацию о студенте из выбранного файла
        /// </summary>
        private void LoadStudent(object sender)
        {
            MainParams mainParams = new MainParams();
            OpenFileDialog dialog = new OpenFileDialog()
            {
                Title = "Загрузка информации о студенте",
                Filter = "JSON файлы (*.json)|*.json|Все файлы (*.*)|*.*",
                DefaultExt = "json",
                FileName = mainParams.UserDataFileName
            };

            if (dialog.ShowDialog() == true)
            {
                if (LoadStudent(dialog.FileName) == false)
                {
                    MessageBox.Show("Не получилось загрузить данные из файла", "Ошибка!", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    return;
                }
                mainParams.UserDataFileName = dialog.FileName;
                SaveStatus = "Автосохранение включено";
            }
        }

        public void OnPropertyChanged([CallerMemberName] string propertyName = "") => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
