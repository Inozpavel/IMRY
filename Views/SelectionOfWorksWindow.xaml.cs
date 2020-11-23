﻿using System;
using System.Windows;
using WorkReportCreator.ViewModels.Commands;

namespace WorkReportCreator
{
    /// <summary>
    /// Окно с вводом информации о студенте и выбором работ
    /// </summary>
    public partial class SelectionOfWorksWindow : Window
    {
        /// <summary>
        /// Главное меню
        /// </summary>
        private readonly MainWindow _mainWindow;

        /// <summary>
        /// Модель данных этого элемента
        /// </summary>
        private readonly SelectionOfWorksViewModel _model;

        /// <exception cref="Exception"/>
        public SelectionOfWorksWindow(MainWindow mainWindow)
        {
            InitializeComponent();
            _model = new SelectionOfWorksViewModel(this);
            DataContext = _model;
            _mainWindow = mainWindow;
        }

        /// <summary>
        /// Показывает форму стартового окна
        /// </summary>
        private void ShowFormMainWindow(object sender, RoutedEventArgs e)
        {
            Close();
            _mainWindow.Show();
        }

        /// <summary>
        /// Завершает работу приложения
        /// </summary>
        private void CloseApplicationClicked(object sender, System.ComponentModel.CancelEventArgs e) => _mainWindow.Show();
    }
}