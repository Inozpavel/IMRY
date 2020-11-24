using System;
using System.Collections.Generic;
using System.Windows;
using WorkReportCreator.Models;
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
        /// Данные студента
        /// </summary>
        public Student Student { get; set; }

        /// <summary>
        /// Окно с информацией о студенте
        /// </summary>
        private readonly SelectionOfWorksWindow _informationPage;

        /// <param name="informationPage">Окно с выбором работ и информацией о студенте</param>
        /// <param name="laboratoryWorks">Список номеров лабораторных работ</param>
        /// <param name="practicalWorks">Список номеров практических работ</param>
        public ReportsWindow(SelectionOfWorksWindow informationPage, List<string> laboratoryWorks, List<string> practicalWorks, Student student, IEnumerable<ReportModel> reports = null)
        {
            InitializeComponent();
            Student = student;
            try
            {
                DataContext = _model = new ReportsWindowViewModel(laboratoryWorks, practicalWorks, this, reports);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Ошибка!", MessageBoxButton.OK, MessageBoxImage.Error);
                throw new Exception();
            }
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
