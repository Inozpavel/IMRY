using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows;
using WorkReportCreator.Models;
using WorkReportCreator.Views;

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
            MainParams.ValidateAllParams();
            textBoxVersion.Text = "Версия " + Assembly.GetExecutingAssembly().GetName().Version.ToString().Substring(2);
        }

        /// <summary>
        /// Загружает шаблон работы и показывает окно для редактирования шаблона
        /// </summary>
        private void LoadReportsTemplateWindow(object sender, RoutedEventArgs e)
        {
            try
            {
                OpenFileDialog dialog = new OpenFileDialog()
                {
                    Title = "Выберите файл с шаблоном",
                    Filter = "JSON файлы (*.template.json)|*.template.json|Все файлы (*.*)|*.*",
                };
                if (dialog.ShowDialog() == true)
                {
                    var template = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, Report>>>(File.ReadAllText(dialog.FileName));
                    ReportsTemplateWindow reportsTemplate = new ReportsTemplateWindow(template, dialog.FileName);
                    reportsTemplate.Show();
                    Close();
                }
            }
            catch (Exception)
            {
                MessageBox.Show("Не получилось загрузить шаблон!", "Ошибка при загрузке шаблона", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        /// <summary>
        /// Открывает папку с приложением
        /// </summary>
        private void OpenApplicationFolder(object sender, RoutedEventArgs e) => Process.Start(Directory.GetCurrentDirectory());

        /// <summary>
        /// Открывает в браузере репозиторий проекта
        /// </summary>
        private void OpenRepositoryInBrowser(object sender, RoutedEventArgs e) => Process.Start("https://github.com/Inozpavel/IMRY");

        /// <summary>
        /// Показывает окно с выбором заданий и вводом информации о студенте
        /// </summary>
        private void ShowWindowReportsSelect(object sender, RoutedEventArgs e)
        {
            try
            {
                SelectionOfWorksWindow window = new SelectionOfWorksWindow();
                window.Show();
                Close();
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message, "Ошибка!", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Показывает окно с настройками
        /// </summary>
        private void ShowSettingsWindow(object sender, RoutedEventArgs e)
        {
            SettingsWindow window = new SettingsWindow();
            window.Show();
            Close();
        }

        /// <summary>
        /// Показывает окно для редактирования шаблона
        /// </summary>
        private void ShowWindowReportsTemplate(object sender, RoutedEventArgs e)
        {
            ReportsTemplateWindow window = new ReportsTemplateWindow();
            window.Show();
            Close();
        }
    }
}
