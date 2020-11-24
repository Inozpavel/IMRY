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
        /// Показывает диалоговое окно для выбором файла с отчетом, чтобы редактировать его
        /// </summary>
        private void LoadReport(object sender, RoutedEventArgs e)
        {
            MainParams mainParams = new MainParams();
            string subjectName = mainParams.ShortSubjectName;
            OpenFileDialog dialog = new OpenFileDialog()
            {
                Multiselect = true,
                Title = "Выберите отчеты, которые хотите изменить",
                Filter = $"Отчеты для текущего предмета - {subjectName} (*.{subjectName}.json)|*.{subjectName}.json",
            };
            if (dialog.ShowDialog() == true)
            {
                List<ReportModel> reports = new List<ReportModel>();
                foreach (var path in dialog.FileNames)
                {
                    try
                    {
                        reports.Add(JsonConvert.DeserializeObject<ReportModel>(File.ReadAllText(path)));
                    }
                    catch
                    {
                        MessageBox.Show($"Невозможно распознать сожержимое файла!\nПовторите выбор файлов!\nПуть: {path}");
                        return;
                    }
                }
                SelectionOfWorksWindow window = new SelectionOfWorksWindow(this, reports);
                Hide();
                window.Show();
            }
        }

        /// <summary>
        /// Загружает шаблон работы и показывает окно для редактирования шаблона
        /// </summary>
        private void LoadWindowReportsTemplate(object sender, RoutedEventArgs e)
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
                    var template = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, Report>>>(File.ReadAllText(dialog.FileName));
                    ReportsTemplateWindow reportsTemplate = new ReportsTemplateWindow(this, template, dialog.FileName);
                    Hide();
                    reportsTemplate.Show();
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
        private void ShowWindowReportsSelect(object sender, RoutedEventArgs e) => ShowWindow<SelectionOfWorksWindow>();

        /// <summary>
        /// Показывает окно с настройками
        /// </summary>
        private void ShowSettingsWindow(object sender, RoutedEventArgs e) => ShowWindow<SettingsWindow>();

        /// <summary>
        /// Показывает окно для редактирования шаблона
        /// </summary>
        private void ShowWindowReportsTemplate(object sender, RoutedEventArgs e) => ShowWindow<ReportsTemplateWindow>();

        /// <summary>
        /// Показывет окно выбранного типа
        /// </summary>
        /// <typeparam name="T">Тип окна</typeparam>
        private void ShowWindow<T>() where T : Window
        {
            T window = (T)Activator.CreateInstance(typeof(T), this);
            Hide();
            window.Show();
        }
    }
}
