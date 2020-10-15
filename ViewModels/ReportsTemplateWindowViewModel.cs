﻿using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using WorkReportCreator.Models;
using WorkReportCreator.Views;

namespace WorkReportCreator.ViewModels
{
    class ReportsTemplateWindowViewModel : INotifyPropertyChanged
    {
        public Command AddWork { get; private set; }

        public Command RemoveWork { get; private set; }

        public Command ChangeWorkType { get; private set; }

        public Command ChooseFile { get; private set; }

        public Command SwapUpElement { get; private set; }

        public Command SwapDownElement { get; private set; }

        private ReportInformation _currentInformation;

        private ReportInformation _practisesCurrentInformation;

        private ReportInformation _laboratoriesCurrentInformation;

        private Visibility _reportInformationVisibility;

        public string FilePath { get; private set; }

        private ObservableCollection<RadioButton> _worksButtons = new ObservableCollection<RadioButton>();

        private bool _hasNotDynamictask = true;

        #region Properties

        /// <summary>
        /// Текущий набор работ для выбора
        /// </summary>
        public ObservableCollection<RadioButton> WorksButtons
        {
            get => _worksButtons;
            set
            {
                _worksButtons = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Текущий список описаний работ
        /// </summary>
        public Dictionary<RadioButton, ReportInformation> Works { get; set; }

        /// <summary>
        /// Список кнопок с номерами доступных лабораторных работ 
        /// </summary>
        public ObservableCollection<RadioButton> LaboratoriesWorksButtons { get; set; } = new ObservableCollection<RadioButton>();

        /// <summary>
        /// Список кнопок с номерами доступных практических работ 
        /// </summary>
        public ObservableCollection<RadioButton> PractisesWorksButtons { get; set; } = new ObservableCollection<RadioButton>();

        /// <summary>
        /// Список описаний лабораторных работ
        /// </summary>
        public Dictionary<RadioButton, ReportInformation> LaboratoriesWorks { get; set; } = new Dictionary<RadioButton, ReportInformation>();

        /// <summary>
        /// Список описаний практических работ
        /// </summary>
        public Dictionary<RadioButton, ReportInformation> PractisesWorks { get; set; } = new Dictionary<RadioButton, ReportInformation>();

        /// <summary>
        /// Показывает, выбран ли пункт "Лабораторные работы"
        /// </summary>
        public bool IsLaboratoriesChecked { get; set; }

        /// <summary>
        /// Показывает, выбран ли пункт "Практические работы"
        /// </summary>
        public bool IsPracticesChecked { get; set; }

        /// <summary>
        /// Текущее выбранное описание работы
        /// </summary>
        public ReportInformation CurrentInformation
        {
            get => _currentInformation;
            set
            {
                _currentInformation = value;
                SwapUpElement.CanExecute(this);
                SwapDownElement.CanExecute(this);
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Текущее выбранное описание практической работы
        /// </summary>
        public ReportInformation PractisesCurrentInformation
        {
            get => _practisesCurrentInformation;
            set
            {
                _practisesCurrentInformation = value;
                if (IsPracticesChecked)
                    CurrentInformation = PractisesCurrentInformation;
                else if (IsLaboratoriesChecked)
                    CurrentInformation = LaboratoriesCurrentInformation;
            }
        }

        /// <summary>
        /// Текущее выбранное описание лабораторной работы
        /// </summary>
        public ReportInformation LaboratoriesCurrentInformation
        {
            get => _laboratoriesCurrentInformation;
            set
            {
                _laboratoriesCurrentInformation = value;
                if (IsPracticesChecked)
                    CurrentInformation = PractisesCurrentInformation;
                else if (IsLaboratoriesChecked)
                    CurrentInformation = LaboratoriesCurrentInformation;
            }
        }

        /// <summary>
        /// Видимость описания работы
        /// </summary>
        public Visibility ReportInformationVisibility
        {
            get => _reportInformationVisibility;
            set
            {
                _reportInformationVisibility = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Отмечена ли кнопка "Не имеются"
        /// </summary>
        public bool HasNotDynamictask
        {
            get { return _hasNotDynamictask; }
            set
            {
                _hasNotDynamictask = value;
                OnPropertyChanged();
            }
        }

        #endregion

        public event PropertyChangedEventHandler PropertyChanged;

        private void AddBaseFunctional()
        {
            AddWork = new Command(AddNewWork, (sender) => IsLaboratoriesChecked || IsPracticesChecked);
            RemoveWork = new Command(RemoveSelectedWork, RemoveSelectedWorkCanExecute);
            ChangeWorkType = new Command(ChangeWorksList, null);
            ChooseFile = new Command(ChooseFilePath, null);
            SwapUpElement = new Command(SwapUpSelectedItem, SwapUpCanExecute);
            SwapDownElement = new Command(SwapDownSelectedItem, SwapDownCanExecute);
            WorksButtons.CollectionChanged += (sender, e) => ReportInformationVisibility = WorksButtons.Any(x => x.IsChecked ?? false) ? Visibility.Visible : Visibility.Collapsed;

            ReportInformationVisibility = Visibility.Collapsed;
        }

        public ReportsTemplateWindowViewModel()
        {
            AddBaseFunctional();
            PractisesWorksButtons.CollectionChanged += (sender, e) => SaveAllInformation(this, null);
            LaboratoriesWorksButtons.CollectionChanged += (sender, e) => SaveAllInformation(this, null);
        }

        public ReportsTemplateWindowViewModel(Dictionary<string, Dictionary<string, ReportInformation>> template, string filePath)
        {
            AddBaseFunctional();
            FilePath = filePath;
            foreach (string number in template["Practices"].Keys)
            {
                if (number.All(x => char.IsDigit(x)))
                {
                    RadioButton radioButton = GenerateNewItem(int.Parse(number));
                    ReportInformation reportInformation = template["Practices"][number];
                    reportInformation.PropertyChanged += SaveAllInformation;
                    PractisesWorks.Add(radioButton, reportInformation);
                    PractisesWorksButtons.Add(radioButton);
                }
                else
                    throw new Exception();
            }
            foreach (string number in template["Laboratories"].Keys)
            {
                if (number.All(x => char.IsDigit(x)))
                {

                    RadioButton radioButton = GenerateNewItem(int.Parse(number));
                    ReportInformation reportInformation = template["Laboratories"][number];
                    reportInformation.PropertyChanged += SaveAllInformation;
                    LaboratoriesWorks.Add(radioButton, reportInformation);
                    LaboratoriesWorksButtons.Add(radioButton);
                }
                else
                    throw new Exception();
            }
            PractisesWorksButtons.CollectionChanged += (sender, e) => SaveAllInformation(this, null);
            LaboratoriesWorksButtons.CollectionChanged += (sender, e) => SaveAllInformation(this, null);
        }

        /// <summary>
        /// Переключает видимые номера работ
        /// </summary>
        private void ChangeWorksList(object sender)
        {
            if (IsPracticesChecked)
            {
                CurrentInformation = PractisesCurrentInformation;
                ReportInformationVisibility = CurrentInformation != null && PractisesWorksButtons.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
                WorksButtons = PractisesWorksButtons;
                Works = PractisesWorks;
            }
            else if (IsLaboratoriesChecked)
            {
                CurrentInformation = LaboratoriesCurrentInformation;
                ReportInformationVisibility = CurrentInformation != null && LaboratoriesWorksButtons.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
                WorksButtons = LaboratoriesWorksButtons;
                Works = LaboratoriesWorks;
            }
        }

        /// <summary>
        /// Добавляет новую пустую работу в список работ
        /// </summary>
        private void AddNewWork(object sender)
        {
            InputWorkNumberBox box = new InputWorkNumberBox(WorksButtons.Select(x => x.Content.ToString()).ToList());

            if ((Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift)) == false)
            {
                box.ShowDialog();
            }

            RadioButton radioButton = GenerateNewItem(box.ResultNumber ?? WorksButtons.Count + 1);
            ReportInformation reportInformation = new ReportInformation();
            reportInformation.PropertyChanged += SaveAllInformation;
            Works.Add(radioButton, reportInformation);
            WorksButtons.Add(radioButton);
        }

        /// <summary>
        /// Удаляет выбранную работу из списка, если в ней есть данные, попросит подтвердить действие
        /// </summary>
        private void RemoveSelectedWork(object sender)
        {
            int index = GetSelectedIndex();
            RadioButton selectedButton = WorksButtons[index];
            if ((string.IsNullOrEmpty(CurrentInformation.Name) && string.IsNullOrEmpty(CurrentInformation.WorkTarget) && string.IsNullOrEmpty(CurrentInformation.CommonTask) && string.IsNullOrEmpty(CurrentInformation.TheoryPart)) == false)
            {
                if (MessageBox.Show("В выбранно элементу имеются введенные данные!\nПри удалении вы потеряет их БЕЗВОЗВРАТНО!\nВы уверены?", "Подтвердите действие",
                    MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No) == MessageBoxResult.No)
                {
                    return;
                }
            }

            Works.Remove(selectedButton);
            WorksButtons.Remove(selectedButton);
            if (index > 0)
                WorksButtons[index - 1].IsChecked = true;
            else if (index == 0) //При удалении самого верхнего элемента
            {
                ReportInformationVisibility = Visibility.Collapsed;
                if (IsPracticesChecked)
                {
                    CurrentInformation = PractisesCurrentInformation = null;
                }
                else if (IsLaboratoriesChecked)
                {
                    CurrentInformation = LaboratoriesCurrentInformation = null;
                }
            }
        }

        /// <summary>
        /// Проверяет, можно ли сейчас удалить элемент
        /// </summary>
        /// <returns><paramref name="True"/>, если возможно, в противном случае <paramref name="false"/></returns>
        private bool RemoveSelectedWorkCanExecute(object sender) => WorksButtons.Any(x => x.IsChecked ?? false);

        private RadioButton GenerateNewItem(int number)
        {
            ResourceDictionary resourceDictionary = new ResourceDictionary() { Source = new Uri("./Views/Styles/DigitRadioButtonStyle.xaml", UriKind.Relative) };
            RadioButton radioButton = new RadioButton() { Content = number, Style = resourceDictionary["RadioButtonDigit"] as Style, GroupName = "WorkNumber" };
            radioButton.Checked += (sender, e) =>
            {
                ReportInformationVisibility = Visibility.Visible;
                if (IsPracticesChecked)
                    PractisesCurrentInformation = Works[sender as RadioButton];
                else if (IsLaboratoriesChecked)
                    LaboratoriesCurrentInformation = Works[sender as RadioButton];
            };
            return radioButton;
        }

        /// <summary>
        /// Показывает диалоговое окно для выбора файла для сохранения шаблона
        /// </summary>
        private void ChooseFilePath(object sender)
        {
            SaveFileDialog dialog = new SaveFileDialog()
            {
                Title = "Выберите файл для сохранения шаблона",
                Filter = "JSON файлы (*.json)|*.json|Все файлы (*.*)|*.*",
                DefaultExt = "json",
                AddExtension = true,
            };

            if (dialog.ShowDialog() == true)
            {
                FilePath = dialog.FileName;
                SaveAllInformation(this, null);
            }
        }

        /// <summary>
        /// Перемещает выбранны элемент вверх
        /// </summary>
        private void SwapUpSelectedItem(object sender)
        {
            int index = GetSelectedIndex();
            WorksButtons.Move(index, index - 1);
            WorksButtons[index - 1].IsChecked = true;
        }

        /// <summary>
        /// Перемещает выбранны элемент вниз
        /// </summary>
        private void SwapDownSelectedItem(object sender)
        {
            int index = GetSelectedIndex();
            WorksButtons.Move(index, index + 1);
            WorksButtons[index + 1].IsChecked = true;
        }

        /// <summary>
        /// Проверяет, можно ли переместить элемент вверх
        /// </summary>
        /// <returns><paramref name="True"/> если может удалить, в противное случает <paramref name="false"/></returns>
        private bool SwapUpCanExecute(object sender) => WorksButtons.Any(x => x.IsChecked ?? false) && GetSelectedIndex() > 0;

        /// <summary>
        /// Проверяет, можно ли переместить элемент вниз
        /// </summary>
        /// <returns><paramref name="True"/> если может удалить, в противное случает <paramref name="false"/></returns>
        private bool SwapDownCanExecute(object sender) => WorksButtons.Any(x => x.IsChecked ?? false) && GetSelectedIndex() < Works.Count - 1;

        /// <summary>
        /// Сохраняет всю введенную информацию в файл
        /// </summary>
        private void SaveAllInformation(object sender, PropertyChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(FilePath))
                return;
            Dictionary<string, Dictionary<string, ReportInformation>> template = new Dictionary<string, Dictionary<string, ReportInformation>>();

            Dictionary<string, ReportInformation> practisesinfo = new Dictionary<string, ReportInformation>();
            foreach (var button in PractisesWorksButtons)
            {
                practisesinfo[button.Content.ToString()] = PractisesWorks[button];
            }

            Dictionary<string, ReportInformation> laboratoriesInfo = new Dictionary<string, ReportInformation>();
            foreach (var button in LaboratoriesWorksButtons)
            {
                laboratoriesInfo[button.Content.ToString()] = LaboratoriesWorks[button];
            }

            template["Practices"] = practisesinfo;
            template["Laboratories"] = laboratoriesInfo;
            File.WriteAllText(FilePath, JsonConvert.SerializeObject(template, Formatting.Indented));
        }

        /// <summary>
        /// Ищет индекс выбранного элемента
        /// </summary>
        /// <returns>Индекс элемента</returns>
        private int GetSelectedIndex()
        {
            int index = -1;
            for (int i = 0; i < WorksButtons.Count; i++)
            {
                if (WorksButtons[i].IsChecked ?? false)
                {
                    index = i;
                    break;
                }
            }
            return index;
        }

        public void OnPropertyChanged([CallerMemberName] string propertyName = "") => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
