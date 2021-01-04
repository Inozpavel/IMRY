using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using WorkReportCreator.ViewModels;

namespace WorkReportCreator
{
    /// <summary>
    /// Окно с вводом информации о студенте и выбором работ
    /// </summary>
    public partial class SelectionOfWorksWindow : Window
    {
        /// <exception cref="Exception"/>
        public SelectionOfWorksWindow()
        {
            InitializeComponent();
            DataContext = new SelectionOfWorksViewModel(this);
        }

        public SelectionOfWorksWindow(List<string> selectedPractices, List<string> selectedLaboratories)
        {
            InitializeComponent();
            DataContext = new SelectionOfWorksViewModel(this, selectedPractices, selectedLaboratories);
        }

        /// <summary>
        /// Показывает форму стартового окна
        /// </summary>
        private void ShowFormMainWindow(object sender, RoutedEventArgs e) => Close();

        /// <summary>
        /// Завершает работу приложения
        /// </summary>
        private void CloseWindow(object sender, CancelEventArgs e)
        {
            MainWindow window = new MainWindow();
            window.Show();
        }
    }
}
