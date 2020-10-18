using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Windows;
using WorkReportCreator.Models;

namespace WorkReportCreator
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            if (File.Exists("./MainConfig.json") == false)
            {
                MessageBox.Show("У вас отсутствует главый конфигурационный файл!\nБез него нельзя использовать приложение!",
                    "Невозможно запустить приложение!", MessageBoxButton.OK, MessageBoxImage.Error);
                Application.Current.Shutdown();
                return;
            }
            MainParams mainParams = new MainParams();
            mainParams.ValidateAllParams();
            textBoxVersion.Text = "Версия " + Assembly.GetExecutingAssembly().GetName().Version.ToString().Substring(2);
        }

        /// <summary>
        /// Показывает окно с выбором заданий и вводом информации о студенте
        /// </summary>
        private void ShowWindowReportsSelect(object sender, RoutedEventArgs e)
        {
            try
            {
                WorksAndStudentInfoWindow document = new WorksAndStudentInfoWindow();
                Hide();
                document.Show();
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message, "Ошибка!", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Показывает диалоговое окно для выбором файла с отчетом, чтобы редактировать его
        /// </summary>
        private void LoadReport(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("В процессе разработки...", "Work in progress!", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        /// <summary>
        /// Показывает окно для редактирования шаблона
        /// </summary>
        private void ShowWindowReportTemplate(object sender, RoutedEventArgs e)
        {
            ReportsTemplate reportsTemplate = new ReportsTemplate(this);
            Hide();
            reportsTemplate.Show();
        }

        /// <summary>
        /// Загружает шаблон работы и показывает окно для редактирования шаблона
        /// </summary>
        private void LoadWindowReportTemplate(object sender, RoutedEventArgs e)
        {
            try
            {
                OpenFileDialog dialog = new OpenFileDialog()
                {
                    Title = "Выберите файл с шаблоном",
                    Filter = "JSON файлы (*.json)|*.json|Все файлы (*.*)|*.*",
                    DefaultExt = "json",
                };
                if (dialog.ShowDialog() == true)
                {
                    var template = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, ReportInformation>>>(File.ReadAllText(dialog.FileName));
                    ReportsTemplate reportsTemplate = new ReportsTemplate(this, template, dialog.FileName);
                    Hide();
                    reportsTemplate.Show();
                }
            }
            catch (Exception)
            {
                MessageBox.Show("Не получилось загрузить шаблон!", "Ошибка при загрузке шаблона", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }
}
