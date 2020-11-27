using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Windows;
using WorkReportCreator.ViewModels;

namespace WorkReportCreator
{
    /// <summary>
    /// Окно с владками для каждой работы
    /// </summary>
    public partial class ReportsWindow : Window
    {
        /// <summary>
        /// Модель данных этого элемента
        /// </summary>
        private readonly ReportsWindowViewModel _model;

        /// <param name="informationPage">Окно с выбором работ и информацией о студенте</param>
        /// <param name="selectedLaboratoryWorks">Список номеров лабораторных работ</param>
        /// <param name="selectedPracticalWorks">Список номеров практических работ</param>
        /// <exception cref="Exception"></exception>
        public ReportsWindow(List<string> selectedLaboratoryWorks, List<string> selectedPracticalWorks)
        {
            InitializeComponent();
            DataContext = _model = new ReportsWindowViewModel(selectedLaboratoryWorks, selectedPracticalWorks);
            _model.ButtonBackClicked += CloseWindow;
        }

        /// <summary>
        /// Открывает окно с выбором работ и информацией о студенте
        /// </summary>
        /// <param name="sender"></param>
        private void CloseWindow(object sender) => Close();

        /// <summary>
        /// Завершает работу приложения
        /// </summary>
        private void CloseWindow(object sender, System.ComponentModel.CancelEventArgs e)
        {
            GetSelectedTasks(out List<string> selectedPractices, out List<string> selectedLaboratories);
            SelectionOfWorksWindow window = new SelectionOfWorksWindow(selectedPractices, selectedLaboratories);
            window.Show();
        }

        private void GetSelectedTasks(out List<string> selectedPractices, out List<string> selectedLaboratories)
        {
            var items = _model.TabItems;
            selectedPractices = new List<string>();
            selectedLaboratories = new List<string>();
            for (int i = 1; i < items.Count; i++)
            {
                string name = items[i].Header as string;
                string number = Regex.Match(name, @"\d+").Value;
                if (Regex.IsMatch(name, "пр|ПР"))
                    selectedPractices.Add(number);
                else
                    selectedLaboratories.Add(number);
            }
        }
    }
}
