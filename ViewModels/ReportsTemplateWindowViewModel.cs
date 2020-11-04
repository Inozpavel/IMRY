using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using WorkReportCreator.Models;
using WorkReportCreator.Views;

namespace WorkReportCreator.ViewModels
{
    internal class ReportsTemplateWindowViewModel : INotifyPropertyChanged
    {
        #region Commands

        public Command AddWork { get; private set; }

        public Command RemoveWork { get; private set; }

        public Command ChangeWorkType { get; private set; }

        public Command ChooseFile { get; private set; }

        public Command SwapUpElement { get; private set; }

        public Command SwapDownElement { get; private set; }

        public Command AddDescription { get; private set; }

        public Command SwapUpDescription { get; private set; }

        public Command SwapDownDescription { get; private set; }

        public Command RemoveDescription { get; private set; }

        public Command FormatFields { get; private set; }

        #endregion

        private ReportInformation _currentInformation;

        private Visibility _reportInformationVisibility;

        private ObservableCollection<RadioButton> _worksButtons = new ObservableCollection<RadioButton>();

        private int? _selecteDescriptionIndex = null;

        private string _filePath;

        private string _saveStatus = "Сохранить в файл";

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
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Текущее выбранное описание практической работы
        /// </summary>
        public ReportInformation PractisesCurrentInformation { get; set; }

        /// <summary>
        /// Текущее выбранное описание лабораторной работы
        /// </summary>
        public ReportInformation LaboratoriesCurrentInformation { get; set; }

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
        /// Индекс текущего выбранного задания для выбора
        /// </summary>
        public int? SelectedDescriptionIndex
        {
            get => _selecteDescriptionIndex;
            set
            {
                _selecteDescriptionIndex = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Путь до файла, в который все сохраняется
        /// </summary>
        public string FilePath
        {
            get => _filePath;
            set
            {
                _filePath = value;
                SaveStatus = string.IsNullOrEmpty(value) ? "Сохранить в файл" : "Автосохранение включено";
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Надпись на кнопке для сохранения
        /// </summary>
        public string SaveStatus
        {
            get => _saveStatus;
            set
            {
                _saveStatus = value;
                OnPropertyChanged();
            }
        }

        #endregion

        public event PropertyChangedEventHandler PropertyChanged;

        public ReportsTemplateWindowViewModel()
        {
            AddBaseFunctional();
            AddAutoSaveForWorksCollections();
        }

        /// <param name="template">Шаблон</param>
        /// <param name="filePath">Путь до файла с шаблоном</param>
        /// <exception cref="ArgumentException"/>
        public ReportsTemplateWindowViewModel(Dictionary<string, Dictionary<string, ReportInformation>> template, string filePath)
        {
            AddBaseFunctional();
            foreach (string workType in template.Keys)
            {
                if (workType != "Practices" && workType != "Laboratories")
                    throw new ArgumentException("Некорректный ключ!");

                foreach (string number in template[workType].Keys)
                {
                    RadioButton radioButton = GenerateNewItem(int.Parse(number));
                    ReportInformation reportInformation = template[workType][number];
                    reportInformation.PropertyChanged += SaveAllInformation;
                    foreach (DynamicTask task in reportInformation.DynamicTasks)
                        task.PropertyChanged += (sender, e) => SaveAllInformation(this, null);
                    if (workType == "Practices")
                    {
                        PractisesWorks.Add(radioButton, reportInformation);
                        PractisesWorksButtons.Add(radioButton);
                    }
                    else
                    {
                        LaboratoriesWorks.Add(radioButton, reportInformation);
                        LaboratoriesWorksButtons.Add(radioButton);
                    }
                }
            }
            FilePath = filePath;
            AddAutoSaveForWorksCollections();
        }

        /// <summary>
        /// Создает команды, подписывет коллекцию WorksButtons на обновление видимости описания работы при ее изменении
        /// </summary>
        private void AddBaseFunctional()
        {
            AddWork = new Command(AddNewWork, (sender) => IsLaboratoriesChecked || IsPracticesChecked);
            RemoveWork = new Command(RemoveSelectedWork, RemoveSelectedWorkCanExecute);
            ChangeWorkType = new Command(ChangeWorksList, null);
            ChooseFile = new Command(ChooseFilePath, null);
            SwapUpElement = new Command(SwapUpSelectedItem, SwapUpCanExecute);
            SwapDownElement = new Command(SwapDownSelectedItem, SwapDownCanExecute);

            FormatFields = new Command(FormatAllFields, (sender) => CurrentInformation != null);

            AddDescription = new Command(AddNewDescription, null);
            RemoveDescription = new Command(RemoveSelectedDescription, (sender) => SelectedDescriptionIndex != null);

            SwapUpDescription = new Command((sender) => CurrentInformation.DynamicTasks.Move(SelectedDescriptionIndex ?? 0, SelectedDescriptionIndex - 1 ?? 0),
                (sender) => SelectedDescriptionIndex != null && SelectedDescriptionIndex > 0);

            SwapDownDescription = new Command((sender) => CurrentInformation.DynamicTasks.Move(SelectedDescriptionIndex ?? 0, SelectedDescriptionIndex + 1 ?? 0),
                (sender) => SelectedDescriptionIndex != null && SelectedDescriptionIndex < CurrentInformation.DynamicTasks.Count - 1);

            WorksButtons.CollectionChanged += (sender, e) => ReportInformationVisibility = WorksButtons.Any(x => x.IsChecked ?? false) ? Visibility.Visible : Visibility.Collapsed;

            ReportInformationVisibility = Visibility.Collapsed;
        }

        /// <summary>
        /// Подписывает коллекции PractisesWorksButtons и LaboratoriesWorksButtons на автосохранении информации при изменении
        /// </summary>
        private void AddAutoSaveForWorksCollections()
        {
            PractisesWorksButtons.CollectionChanged += (sender, e) => SaveAllInformation(this, null);
            LaboratoriesWorksButtons.CollectionChanged += (sender, e) => SaveAllInformation(this, null);
        }

        private void FormatAllFields(object sender)
        {
            CurrentInformation.Name = FormatText(CurrentInformation.Name, isNameOfWork: true);
            CurrentInformation.WorkTarget = FormatText(CurrentInformation.WorkTarget);
            CurrentInformation.TheoryPart = FormatText(CurrentInformation.TheoryPart);
            CurrentInformation.CommonTask = FormatText(CurrentInformation.CommonTask);
            for (int i = 0; i < CurrentInformation.DynamicTasks.Count; i++)
                CurrentInformation.DynamicTasks[i].Description = FormatText(CurrentInformation.DynamicTasks[i].Description);
        }

        public string FormatText(string textToFormat, bool isNameOfWork = false)
        {
            if (textToFormat.Length == 0)
                return textToFormat;
            string formattedText = textToFormat.Trim();
            if (isNameOfWork == false & ".!?".Contains(formattedText.Last()) == false)
                formattedText += ".";
            else if (isNameOfWork)
            {
                while (formattedText.Length > 0 && ".!?".Contains(formattedText[formattedText.Length - 1]))
                    formattedText = formattedText.Substring(0, formattedText.Length - 1);

                formattedText = formattedText.ToUpper();
            }
            return formattedText;
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
            RadioButton radioButton;
            ReportInformation reportInformation;
            if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
                radioButton = GenerateNewItem(WorksButtons.Count + 1);
            else
            {
                InputWorkNumberBox box = new InputWorkNumberBox(WorksButtons.Select(x => x.Content.ToString()).ToList());
                box.ShowDialog();
                if (box.ResultNumber == null)
                    return;
                radioButton = GenerateNewItem(box.ResultNumber ?? WorksButtons.Count + 1);

            }
            reportInformation = new ReportInformation();
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
            if (Keyboard.IsKeyDown(Key.LeftShift) == false && Keyboard.IsKeyDown(Key.RightShift) == false &&
                (string.IsNullOrEmpty(CurrentInformation.Name) && string.IsNullOrEmpty(CurrentInformation.WorkTarget) &&
                string.IsNullOrEmpty(CurrentInformation.CommonTask) && string.IsNullOrEmpty(CurrentInformation.TheoryPart)) == false)
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
                UpdateCurrentInformation(sender);
            };
            return radioButton;
        }

        private void UpdateCurrentInformation(object sender)
        {
            if (IsPracticesChecked)
                CurrentInformation = PractisesCurrentInformation = Works[sender as RadioButton];
            else if (IsLaboratoriesChecked)
                CurrentInformation = LaboratoriesCurrentInformation = Works[sender as RadioButton];
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
        private void SwapUpSelectedItem(object sender) => SwapAdjacentItemWithSelected(-1);

        /// <summary>
        /// Перемещает выбранны элемент вниз
        /// </summary>
        private void SwapDownSelectedItem(object sender) => SwapAdjacentItemWithSelected(+1);

        /// <summary>
        /// Обменивает ближайший элемент с выбранным
        /// </summary>
        /// <param name="i">1 - элемент снизу, -1 - элемент снизу</param>
        private void SwapAdjacentItemWithSelected(int i)
        {
            int index = GetSelectedIndex();
            WorksButtons.Move(index, index + i);
            WorksButtons[index + i].IsChecked = true;
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
        /// Добавляет новое описание работы в список, подписывает его на автообновление
        /// </summary>
        private void AddNewDescription(object sender)
        {
            DynamicTask task = new DynamicTask();
            task.PropertyChanged += (s, e) => SaveAllInformation(this, null);
            CurrentInformation.DynamicTasks.Add(task);
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

        /// <summary>
        /// Удаляет выбранное задание из работы из списка
        /// </summary>
        private void RemoveSelectedDescription(object sender)
        {
            int index = SelectedDescriptionIndex ?? 0;
            string text = CurrentInformation.DynamicTasks[index].Description;
            if (string.IsNullOrEmpty(text) == false && Keyboard.IsKeyDown(Key.LeftShift) == false && Keyboard.IsKeyDown(Key.RightShift) == false)
            {
                if (MessageBox.Show("В выбранно элементу имеются введенные данные!\nПри удалении вы потеряет их БЕЗВОЗВРАТНО!\nВы уверены?", "Подтвердите действие",
                    MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No) == MessageBoxResult.No)
                {
                    return;
                }
            }
            CurrentInformation.DynamicTasks.RemoveAt(index);
            if (index > 0)
                SelectedDescriptionIndex = index - 1;
            else
                SelectedDescriptionIndex = null;
        }

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
                practisesinfo[button.Content.ToString()] = PractisesWorks[button];

            Dictionary<string, ReportInformation> laboratoriesInfo = new Dictionary<string, ReportInformation>();
            foreach (var button in LaboratoriesWorksButtons)
                laboratoriesInfo[button.Content.ToString()] = LaboratoriesWorks[button];

            template["Practices"] = practisesinfo;
            template["Laboratories"] = laboratoriesInfo;
            try
            {
                File.WriteAllText(FilePath, Regex.Replace(JsonConvert.SerializeObject(template, Formatting.Indented), @"\\r", ""));
            }
            catch (UnauthorizedAccessException)
            {
                if (MessageBox.Show("У файла установлен атрибут \"Только чтение\"\nНе получилось перезаписать его!\nСнять с него этот атрибут?",
                    "Ошибка", MessageBoxButton.YesNo, MessageBoxImage.Error, MessageBoxResult.No) == MessageBoxResult.Yes)
                {
                    File.SetAttributes(FilePath, FileAttributes.Normal);
                    File.WriteAllText(FilePath, Regex.Replace(JsonConvert.SerializeObject(template, Formatting.Indented), @"\\r", ""));
                }
                else
                    FilePath = "";
            }
        }

        public void OnPropertyChanged([CallerMemberName] string propertyName = "") => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
