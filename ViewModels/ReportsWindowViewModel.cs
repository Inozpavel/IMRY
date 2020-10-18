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
using WorkReportCreator.Models;
using WorkReportCreator.Views;

namespace WorkReportCreator.ViewModels
{
    internal class ReportsWindowViewModel : INotifyPropertyChanged
    {
        private int? _selectedIndex;

        /// <summary>
        /// Список всех вкладок
        /// </summary>
        public ObservableCollection<TabItem> TabItems { get; set; } = new ObservableCollection<TabItem>();

        /// <summary>
        /// Идекс текущей выбранной вкладки
        /// </summary>
        public int? SelectedIndex
        {
            get => _selectedIndex;
            set
            {
                _selectedIndex = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Вызывается при нажатии кнопки назад
        /// </summary>
        public event Action<object> ButtonBackClicked;

        public event PropertyChangedEventHandler PropertyChanged;

        /// <param name="laboratoryWorks">Список доступный для выбора лабораторных работ</param>
        /// <param name="practicalWorks">Список доступный для выбора приктических работ</param>
        /// <param name="reportsWindow">Окно, на котором расположен элемент</param>
        public ReportsWindowViewModel(List<string> laboratoryWorks, List<string> practicalWorks, ReportsWindow reportsWindow)
        {
            FastActionsItem fastActionsItem = new FastActionsItem
            {
                IsButtonEnabled = laboratoryWorks.Count > 0 || practicalWorks.Count > 0
            };
            fastActionsItem.ButtonBackClicked += (sender) => ButtonBackClicked?.Invoke(sender);
            fastActionsItem.ButtonGenerateAllClicked += (sender) => TabItems.Where(item => item.Content is ReportItem)
            .Select(x => x.Content as ReportItem).ToList().ForEach(item => item.GenerateReport(item, null));

            TabItems.Add(new TabItem() { Header = "Быстрые действия", Content = fastActionsItem });

            Dictionary<string, Dictionary<string, ReportInformation>> template;
            MainParams mainParams = new MainParams();
            try
            {
                template = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, ReportInformation>>>(File.ReadAllText(mainParams.CurrentTemplateFilePath));
            }
            catch (Exception)
            {
                throw new Exception("Не получилось загрузить данные из шаблона!");
            }

            if (template.Keys.Contains("Laboratories") == false && template.Keys.Contains("Practices") == false)
            {
                throw new Exception("В файле с шаблоном отсутствуют и практические и лабораторные работы!");
            }

            LoadTemplateInfoFromKey(template, reportsWindow, "Practices", "пр.", practicalWorks);
            LoadTemplateInfoFromKey(template, reportsWindow, "Laboratories", "лаб.", laboratoryWorks);

            SelectedIndex = 0;
            OnPropertyChanged();
        }

        private void LoadTemplateInfoFromKey(Dictionary<string, Dictionary<string, ReportInformation>> template, ReportsWindow window, string key, string shortDescription, List<string> selectedWorks)
        {
            if (template.Keys.Contains(key) == false)
                return;


            foreach (string number in template[key].Keys.Where(x => selectedWorks.Contains(x)))
            {
                try
                {
                    TabItems.Add(new TabItem()
                    {
                        Header = $"{number} {shortDescription}",
                        Content = new ReportItem(window, template[key][number].DynamicTasks.Select(x => x.Description).ToList())
                    });
                }
                catch (Exception)
                {
                    MessageBox.Show($"Не получилось загрузить {number} {shortDescription} работу,\nвозможно, ошибка в ключах.",
                        $"Ошибка при загрузке работы!", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        public void OnPropertyChanged([CallerMemberName] string propertyName = "") => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
