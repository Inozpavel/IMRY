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
using System.Windows.Media;
using WorkReportCreator.Models;

namespace WorkReportCreator.ViewModels
{
    public class SelectionOfWorksViewModel : INotifyPropertyChanged
    {
        #region Fields

        private readonly SelectionOfWorksWindow _selectionOfWorksWindow;

        private readonly Style _tiltedButtonStyle;

        private readonly Style _numberToggleButtonStyle;

        private Visibility _labaratoriesVisibility;

        private Visibility _practicesVisibility;

        private Visibility _worksSelectVisibility;

        private Student _student;

        private string _saveStatus;

        private bool _canShowPageWithReports;

        /// <summary>
        /// От значения зависит, все кнопки с практическими работами будут отмечены / не отмечены
        /// </summary>
        private bool _shouldCheckAllPracticalWorks = false;

        /// <summary>
        /// От значения зависит, все кнопки с лабораторными работами будут отмечены / не отмечены
        /// </summary>
        private bool _shouldCheckAllLaboratoryWorks = false;

        #endregion

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
        /// Текущая информация о пользователе
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

        public bool CanShowPageWithReports
        {
            get => _canShowPageWithReports;
            set
            {
                _canShowPageWithReports = value;
                OnPropertyChanged();
            }
        }
        #endregion

        public event PropertyChangedEventHandler PropertyChanged;

        /// <param name="window">Окно с вводом информации о пользователе и выбором работ</param>
        /// <exception cref="Exception"/>
        public SelectionOfWorksViewModel(SelectionOfWorksWindow window, List<string> selectedPractices = null, List<string> selectedLaboratories = null)
        {
            ResourceDictionary _tiltedButtonDictionary = new ResourceDictionary()
            {
                Source = new Uri("/Views/Styles/TiltedButtonStyle.xaml", UriKind.Relative)
            };
            ResourceDictionary _numberToggleButtonDictionary = new ResourceDictionary()
            {
                Source = new Uri("/Views/Styles/NumberToggleButtonStyle.xaml", UriKind.Relative)
            };
            _tiltedButtonStyle = _tiltedButtonDictionary["TiltedButton"] as Style;
            _numberToggleButtonStyle = _numberToggleButtonDictionary["NumberToggleButton"] as Style;

            _selectionOfWorksWindow = window;
            _student = new Student();
            _student.PropertyChanged += SaveStudentInformation;

            SaveStudentInfo = new Command(ShowDialogSaveStudent, null);
            LoadStudentInfo = new Command(ShowDialogLoadStudent, null);
            CheckAllLaboratoryButtons = new Command(() => CheckAllButtons(LaboratoryWorksButtons, ref _shouldCheckAllLaboratoryWorks), null);
            CheckAllPracticalButtons = new Command(() => CheckAllButtons(PracticalWorksButtons, ref _shouldCheckAllPracticalWorks), null);
            ShowReportsPage = new Command(LoadReportsPage, null);

            MainParams mainParams = new MainParams();
            Student student = LoadStudent(mainParams.UserDataFilePath);
            if (student == null && string.IsNullOrEmpty(mainParams.UserDataFilePath) == false)
                MessageBox.Show("Не получилось загрузить информацию о пользователе!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            Student = student ?? new Student();

            AddButtonCheckAll(PracticalWorksButtons);
            AddButtonCheckAll(LaboratoryWorksButtons);

            try
            {
                var template = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, Report>>>(File.ReadAllText(mainParams.CurrentTemplateFilePath));
                string reportsPath = mainParams.SavedReportsPath;
                List<ReportModel> existingReports = new List<ReportModel>();
                if (Directory.Exists(reportsPath))
                {
                    string[] paths = Directory.GetFiles(reportsPath, $"*.{mainParams.ShortSubjectName}.json");
                    foreach (string path in paths)
                    {
                        try
                        {
                            ReportModel report = JsonConvert.DeserializeObject<ReportModel>(File.ReadAllText(path));
                            if (report != null)
                                existingReports.Add(report);
                        }
                        catch
                        {
                        }
                    }
                }
                foreach (string workType in template.Keys.Distinct())
                {
                    foreach (string workNumber in template[workType].Keys.Distinct())
                    {
                        ToggleButton button = new ToggleButton()
                        {
                            Content = workNumber,
                            Style = _numberToggleButtonStyle,
                        };
                        button.Unchecked += (sender, e) => CanShowPageWithReports = LaboratoryWorksButtons.Skip(1).Any(x => x.IsChecked ?? false) || PracticalWorksButtons.Skip(1).Any(x => x.IsChecked ?? false);
                        button.Checked += (sender, e) => CanShowPageWithReports = LaboratoryWorksButtons.Skip(1).Any(x => x.IsChecked ?? false) || PracticalWorksButtons.Skip(1).Any(x => x.IsChecked ?? false);
                        if (workType == "Practices")
                        {
                            PracticalWorksButtons.Add(StylizeButton(button, existingReports, "Practice", workNumber));
                            button.IsChecked = selectedPractices?.Contains(button.Content as string) ?? false;
                        }
                        else if (workType == "Laboratories")
                        {
                            LaboratoryWorksButtons.Add(StylizeButton(button, existingReports, "Laboratory", workNumber));
                            button.IsChecked = selectedLaboratories?.Contains(button.Content as string) ?? false;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                throw new Exception($"Не удалось загрузить номера работ из шаблона!\n" + e.Message);
            }
            PracticesVisibility = PracticalWorksButtons.Count > 1 ? Visibility.Visible : Visibility.Collapsed;
            LaboratoriesVisibility = LaboratoryWorksButtons.Count > 1 ? Visibility.Visible : Visibility.Collapsed;
            WorksSelectVisibility = PracticalWorksButtons.Count > 1 || LaboratoryWorksButtons.Count > 1 ? Visibility.Visible : Visibility.Collapsed;
            SaveStatus = string.IsNullOrEmpty(mainParams.UserDataFilePath) ? "Сохранить пользователя" : "Автосохранение включено";
        }

        /// <summary>
        /// Проверяет существование работы, стилизует кнопку
        /// </summary>
        /// <param name="button">Сама кнопка</param>
        /// <param name="existingReports">Сохраненные работы работы</param>
        /// <param name="workType">Тип работы</param>
        /// <param name="workNumber">Номер работы</param>
        /// <param name="existingWorkColor">Цвет существующей работы</param>
        /// <param name="notExistingWorkColor">Цвет не существующей работы</param>
        private ToggleButton StylizeButton(ToggleButton button, List<ReportModel> existingReports, string workType, string workNumber)
        {
            bool isExisting = existingReports.Where(x => x.WorkType == workType).FirstOrDefault(x => x.WorkNumber.ToString() == workNumber) != null;
            button.Foreground = GetBrushForWork(isExisting, "Black", "#FFBAC3C3");
            button.BorderBrush = GetBrushForWork(isExisting, "#FF18FF00", "White");
            button.Background = GetBrushForWork(isExisting, "#FFFAA515", "#FF121212");
            if (isExisting)
                button.ToolTip = "Работа уже сохранена";
            return button;
        }

        /// <summary>
        /// Возвращает кисть
        /// </summary>
        /// <returns>Кисть</returns>
        private SolidColorBrush GetBrushForWork(bool isExisting, string existingWorkColor, string notExistingWorkColor)
        {
            if (isExisting)
                return new SolidColorBrush((Color)ColorConverter.ConvertFromString(existingWorkColor));
            else
                return new SolidColorBrush((Color)ColorConverter.ConvertFromString(notExistingWorkColor));
        }

        /// <summary>
        /// Добавляет кнопку для отметки всех работ сразу
        /// </summary>
        /// <param name="buttons">Список, куда нужно добавить кнопку</param>
        private void AddButtonCheckAll(List<ToggleButton> buttons)
        {
            buttons.Add(new ToggleButton()
            {
                Content = "Все работы",
                Style = _tiltedButtonStyle,
                Command = CheckAllPracticalButtons,
                Margin = new Thickness(10),
            });
        }

        /// <summary>
        /// Создает окно с работами и показывает его
        /// </summary>
        private void LoadReportsPage()
        {
            MainParams mainParams = new MainParams();
            if (string.IsNullOrEmpty(mainParams.UserDataFilePath))
            {
                MessageBox.Show("Сначала сохраните или загрузите информацию о себе.", "Не пущу!", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            List<string> selectedLaboratoryWorks = LaboratoryWorksButtons.Where(x => x.IsChecked ?? false).Select(x => x.Content as string).ToList();
            List<string> selectedPracticalWorks = PracticalWorksButtons.Where(x => x.IsChecked ?? false).Select(x => x.Content as string).ToList();
            try
            {
                ReportsWindow reportsPage = new ReportsWindow(selectedLaboratoryWorks, selectedPracticalWorks);
                reportsPage.Show();
                _selectionOfWorksWindow.Hide();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Ошибка!", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Отмечает / снимает отметки со всех кнопок с практическими / лабораторными работами
        /// </summary>
        private void CheckAllButtons(List<ToggleButton> buttons, ref bool isChecked)
        {
            if (buttons.All(x => x.IsChecked ?? false))
                isChecked = false;
            else if (buttons.All(x => x.IsChecked ?? false == false))
                isChecked = true;
            else
                isChecked = !isChecked;
            bool newValue = isChecked;
            buttons.ForEach(x => x.IsChecked = newValue);
        }

        #region StudentManagement

        /// <summary>
        /// Сохраняет информацию о пользователе в выбранном файле
        /// </summary>
        private void ShowDialogSaveStudent()
        {

            SaveFileDialog dialog = new SaveFileDialog()
            {
                Title = "Сохранение информации о пользователе",
                Filter = "JSON файлы c информацией о пользователе (*.user.json)|*.user.json|Все файлы (*.*)|*.*",
                DefaultExt = "user.json",
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
        /// Загружает информацию о пользователе из указанного файла
        /// </summary>
        private Student LoadStudent(string filePath)
        {
            if (File.Exists(filePath) == false)
                return null;
            try
            {
                Student student = JsonConvert.DeserializeObject<Student>(File.ReadAllText(filePath)) ?? new Student();
                student.PropertyChanged += SaveStudentInformation;
                return student;
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Загружает информацию о пользователе из выбранного файла
        /// </summary>
        private void ShowDialogLoadStudent()
        {
            MainParams mainParams = new MainParams();
            OpenFileDialog dialog = new OpenFileDialog()
            {
                Title = "Загрузка информации о пользователе",
                Filter = "JSON файлы c информацией о пользователе (*.user.json)|*.user.json|Все файлы (*.*)|*.*",
            };

            if (dialog.ShowDialog() == true)
            {
                Student student = LoadStudent(dialog.FileName);
                if (student == null)
                {
                    MessageBox.Show("Не получилось загрузить данные из файла", "Ошибка!", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    return;
                }
                Student = student;
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
                    SaveStatus = "Сохранить пользователя";
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
