using System;
using System.Collections.Generic;
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

        /// <summary>
        /// Окно с информацией о студенте
        /// </summary>
        private readonly SelectionOfWorksWindow _informationPage;

        /// <param name="informationPage">Окно с выбором работ и информацией о студенте</param>
        /// <param name="selectedLaboratoryWorks">Список номеров лабораторных работ</param>
        /// <param name="selectedPracticalWorks">Список номеров практических работ</param>
        /// <exception cref="Exception"></exception>
        public ReportsWindow(SelectionOfWorksWindow informationPage, List<string> selectedLaboratoryWorks, List<string> selectedPracticalWorks)
        {
            InitializeComponent();
            DataContext = _model = new ReportsWindowViewModel(selectedLaboratoryWorks, selectedPracticalWorks);
            _model.ButtonBackClicked += ShowWorkAndStudentInformation;
            _informationPage = informationPage;
        }

        /// <summary>
        /// Открывает окно с выбором работ и информацией о студенте
        /// </summary>
        /// <param name="sender"></param>
        private void ShowWorkAndStudentInformation(object sender)
        {
            Close();
            _informationPage.Show();
        }

        /// <summary>
        /// Завершает работу приложения
        /// </summary>
        private void CloseApplicationClicked(object sender, System.ComponentModel.CancelEventArgs e) => _informationPage.Show();
    }
}
