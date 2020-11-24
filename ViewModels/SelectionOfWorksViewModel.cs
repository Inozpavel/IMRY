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
    internal class SelectionOfWorksViewModel : INotifyPropertyChanged
    {
        private readonly SelectionOfWorksWindow _selectionOfWorksWindow;

        private ResourceDictionary _tiltedButtonDictionary;

        private ResourceDictionary _numberToggleButtonDictionary;

        private Visibility _labaratoriesVisibility;

        private Visibility _practicesVisibility;

        private Visibility _worksSelectVisibility;

        private Student _student;

        private string _saveStatus;

        private readonly List<ReportModel> _reports;

        /// <summary>
        /// От значения зависит, все кнопки с практическими работами будут отмечены / не отмечены
        /// </summary>
        private bool _shouldCheckAllPracticalWorks = false;

        /// <summary>
        /// От значения зависит, все кнопки с лабораторными работами будут отмечены / не отмечены
        /// </summary>
        private bool _shouldCheckAllLaboratoryWorks = false;

        public string SaveStatus
        {
            get => _saveStatus;
            set
            {
                _saveStatus = value;
                OnPropertyChanged();
            }
        }

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
        public Student Student
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
        public SelectionOfWorksViewModel(SelectionOfWorksWindow window)
        {
            MainParams mainParams = new MainParams();
            _selectionOfWorksWindow = window;
            AddBaseFunctional();
            try
            {
                var template = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, Report>>>(File.ReadAllText(mainParams.CurrentTemplateFilePath));
                foreach (string type in template.Keys.Distinct())
                {
                    foreach (string workNumber in template[type].Keys.Distinct())
                    {
                        ToggleButton button = new ToggleButton() { Content = workNumber, Style = _numberToggleButtonDictionary["NumberToggleButton"] as Style };
                        if (type == "Practices")
                            PracticalWorksButtons.Add(button);
                        else if (type == "Laboratories")
                            LaboratoryWorksButtons.Add(button);
                    }
                }
            }
            catch (Exception e)
            {
                MessageBox.Show($"Не удалось загрузить номера работ из шаблона!\n{e.Message}", "Ошибка!", MessageBoxButton.OK, MessageBoxImage.Error);
                throw new Exception();
            }
            UpdateVisibility(mainParams);
        }

        public SelectionOfWorksViewModel(SelectionOfWorksWindow window, IEnumerable<ReportModel> reports)
        {
            MainParams mainParams = new MainParams();
            _selectionOfWorksWindow = window;
            _reports = reports.ToList();
            AddBaseFunctional();
            try
            {
                var template = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, Report>>>(File.ReadAllText(mainParams.CurrentTemplateFilePath));
                foreach (var report in reports)
                {
                    ToggleButton button = new ToggleButton() { Content = report.WorkNumber.ToString(), Style = _numberToggleButtonDictionary["NumberToggleButton"] as Style };

                    if (CheckTypeAndNumber(template, report.WorkType, report.WorkNumber.ToString()))
                    {
                        if (report.WorkType == "Practice")
                            PracticalWorksButtons.Add(button);
                        else if (report.WorkType == "Laboratory")
                            LaboratoryWorksButtons.Add(button);
                    }
                }
            }
            catch (Exception e)
            {
                MessageBox.Show($"Не удалось загрузить работу!\n{e.Message}", "Ошибка!", MessageBoxButton.OK, MessageBoxImage.Error);
                throw new Exception();
            }
            UpdateVisibility(mainParams);
            CheckAllButtons(PracticalWorksButtons, ref _shouldCheckAllPracticalWorks);
            CheckAllButtons(LaboratoryWorksButtons, ref _shouldCheckAllLaboratoryWorks);
        }

        private void AddBaseFunctional()
        {
            _tiltedButtonDictionary = new ResourceDictionary()
            {
                Source = new Uri("/Views/Styles/TiltedButtonStyle.xaml", UriKind.Relative)
            };
            _numberToggleButtonDictionary = new ResourceDictionary()
            {
                Source = new Uri("/Views/Styles/NumberToggleButtonStyle.xaml", UriKind.Relative)
            };

            _student = new Student();
            _student.PropertyChanged += SaveStudentInformation;

            SaveStudentInfo = new Command(ShowDialogSaveStudent, null);
            LoadStudentInfo = new Command(ShowDialogLoadStudent, null);
            CheckAllLaboratoryButtons = new Command((sender) => CheckAllButtons(LaboratoryWorksButtons, ref _shouldCheckAllLaboratoryWorks), null);
            CheckAllPracticalButtons = new Command((sender) => CheckAllButtons(PracticalWorksButtons, ref _shouldCheckAllPracticalWorks), null);
            ShowReportsPage = new Command(LoadReportsPage, null);

            MainParams mainParams = new MainParams();
            LoadStudent(mainParams.UserDataFilePath);
            AddButtonsCheckAll(PracticalWorksButtons);
            AddButtonsCheckAll(LaboratoryWorksButtons);
        }

        private void AddButtonsCheckAll(List<ToggleButton> buttons)
        {
            ToggleButton button = new ToggleButton()
            {
                Content = "Все работы",
                Style = _tiltedButtonDictionary["TiltedButton"] as Style,
                Command = CheckAllPracticalButtons,
                Margin = new Thickness(10),
            };
            buttons.Add(button);
        }

        private void UpdateVisibility(MainParams mainParams)
        {
            PracticesVisibility = PracticalWorksButtons.Count > 1 ? Visibility.Visible : Visibility.Collapsed;
            LaboratoriesVisibility = LaboratoryWorksButtons.Count > 1 ? Visibility.Visible : Visibility.Collapsed;
            WorksSelectVisibility = PracticalWorksButtons.Count > 1 || LaboratoryWorksButtons.Count > 1 ? Visibility.Visible : Visibility.Collapsed;
            SaveStatus = string.IsNullOrEmpty(mainParams.UserDataFilePath) ? "Сохранить информацию о студенте" : "Автосохранение включено";
        }

        private bool CheckTypeAndNumber(Dictionary<string, Dictionary<string, Report>> template, string workType, string workNumber)
        {
            if (workType == "Practice")
            {
                if (template["Practices"].Keys.Contains(workNumber) == false)
                    throw new Exception($"Шаблон для {workNumber} практики не найден!");
                return true;
            }
            else if (workType == "Laboratory")
            {
                if (template["Laboratories"].Keys.Contains(workNumber) == false)
                    throw new Exception($"Шаблон для {workNumber} практики не найден!");
                return true;
            }
            else
                throw new Exception($"Неизвестный тип у {workNumber} работы!({workType})");
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
                ReportsWindow reportsPage = new ReportsWindow(_selectionOfWorksWindow, selectedLaboratoryWorks, selectedPracticalWorks, Student, _reports);

                _selectionOfWorksWindow.Hide();
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

        #region StudentManagement

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
                new MainParams()
                {
                    UserDataFilePath = dialog.FileName
                };
                SaveStudentInformation(this, null);
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
                Student = JsonConvert.DeserializeObject<Student>(File.ReadAllText(filePath));
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
        private void ShowDialogLoadStudent(object sender)
        {
            MainParams mainParams = new MainParams();
            OpenFileDialog dialog = new OpenFileDialog()
            {
                Title = "Загрузка информации о студенте",
                Filter = "JSON файлы (*.json)|*.json|Все файлы (*.*)|*.*",
                DefaultExt = "json",
                FileName = mainParams.UserDataFilePath
            };

            if (dialog.ShowDialog() == true)
            {
                if (LoadStudent(dialog.FileName) == false)
                {
                    MessageBox.Show("Не получилось загрузить данные из файла", "Ошибка!", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    return;
                }
                mainParams.UserDataFilePath = dialog.FileName;
                SaveStatus = "Автосохранение включено";
            }
        }

        private void SaveStudentInformation(object sender, PropertyChangedEventArgs e)
        {
            MainParams mainParams = new MainParams();
            try
            {
                File.WriteAllText(mainParams.UserDataFilePath, JsonConvert.SerializeObject(Student, Formatting.Indented));
                mainParams.UserDataFilePath = mainParams.UserDataFilePath;
                SaveStatus = "Автосохранение включено";
            }
            catch (UnauthorizedAccessException)
            {
                if (MessageBox.Show("У файла установлен атрибут \"Только чтение\"\nНе получилось перезаписать его!\nСнять с него этот атрибут?",
                    "Ошибка", MessageBoxButton.YesNo, MessageBoxImage.Error, MessageBoxResult.No) == MessageBoxResult.Yes)
                {
                    File.SetAttributes(mainParams.UserDataFilePath, FileAttributes.Normal);
                    File.WriteAllText(mainParams.UserDataFilePath, JsonConvert.SerializeObject(Student, Formatting.Indented));
                }
                else
                {
                    MessageBox.Show("Не получилось сохранить информацию!", "Ошибка!", MessageBoxButton.OK, MessageBoxImage.Error);
                    mainParams.UserDataFilePath = "";
                    SaveStatus = "Сохранить информацию о студенте";
                }
            }
            catch (Exception)
            {
            }
        }

        #endregion

        public void OnPropertyChanged([CallerMemberName] string propertyName = "") => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
