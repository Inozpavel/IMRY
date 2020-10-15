using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

            try
            {
                List<string> requiredParams = new List<string>()
                {
                    "TitlePageFilePath",
                    "TitlePageParametersFilePath",
                    "DynamicTasksFilePath",
                    "PermittedDragAndDropExtentionsFilePath",
                    "CurrentTemplateFilePath",
                    "StandartUserDataFileName",
                    "AllReportsPath"
                };

                var globalParams = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText("./MainConfig.json"));
                if (requiredParams.All(param => globalParams.Keys.Contains(param)) == false)
                {
                    MessageBox.Show("В главном конфигурационном файле отсутствует обязательный параметры!\nБез него нельзя использовать приложение!",
                    "Невозможно запустить приложение!", MessageBoxButton.OK, MessageBoxImage.Error);
                    Application.Current.Shutdown();
                    return;
                }

                foreach (var param in globalParams.Keys.Where(x => x.Contains("FilePath")))
                {
                    if (File.Exists(globalParams[param]) == false)
                    {
                        MessageBox.Show($"Ошибка в параметре {param},\nфайла {globalParams[param]} не существует!\nОн необходим для работы приложения!\nПроверьть его корректность",
                            "Ошибка!", MessageBoxButton.OK, MessageBoxImage.Error);
                        Application.Current.Shutdown();
                        return;
                    }
                }
            }
            catch (Exception)
            {
                MessageBox.Show("Не получилось проверить главый конфигурационный файл!\nБез него нельзя использовать приложение!",
                    "Невозможно запустить приложение!", MessageBoxButton.OK, MessageBoxImage.Error);
                Application.Current.Shutdown();
                return;
            }
        }

        /// <summary>
        /// Показывает окно с выбором заданий и вводом информации о студенте
        /// </summary>
        private void ShowWindowReportsSelect(object sender, RoutedEventArgs e)
        {
            WorkAndStudentInfoWindow document = new WorkAndStudentInfoWindow();
            Hide();
            document.Show();
        }

        /// <summary>
        /// Показывает диалоговое окно для выбором файла с отчетом, чтобы редактировать его
        /// </summary>
        private void LoadReport(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog()
            {
                Title = "Выборите отчета для редактирования",
                Filter = "Xml файлы (*.xml)|*.xml|Все файлы (*.*)|*.*",
                DefaultExt = "xml",
            };

            if (dialog.ShowDialog() == true)
            {
                string filePath = dialog.FileName;
                MessageBox.Show("В процессе разработки...", "Work in progress!", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void ShowWindowReportTemplate(object sender, RoutedEventArgs e)
        {
            ReportsTemplate reportsTemplate = new ReportsTemplate(this);
            Hide();
            reportsTemplate.Show();
        }

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
