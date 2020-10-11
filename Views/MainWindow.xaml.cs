using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;

namespace WorkReportCreator
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            if (File.Exists("./GlobalConfig.json") == false)
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
                    "TitlePagePath",
                    "TitlePageParametersPath",
                    "DynamicTasksFilePath",
                    "PermittedWorksAndExtentionsPath",
                    "StandartUserDataFileName",
                    "AllReportsFilePath",
                    "CurrentTemplatePath",
                };

                var globalParams = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText("./GlobalConfig.json"));
                if (requiredParams.All(param => globalParams.Keys.Contains(param)) == false)
                {
                    MessageBox.Show("В главном конфигурационном файле отсутствует обязательный параметры!\nБез него нельзя использовать приложение!",
                    "Невозможно запустить приложение!", MessageBoxButton.OK, MessageBoxImage.Error);
                    Application.Current.Shutdown();
                    return;
                }
            }
            catch (Exception)
            {
                MessageBox.Show("Не получилось проверить главый конфигурационный файл!\nБез него нельзя использовать приложение!",
                    "Невозможно запустить приложение!", MessageBoxButton.OK, MessageBoxImage.Error);
                Application.Current.Shutdown();
                return;
            }
            InitializeComponent();
        }

        /// <summary>
        /// Показывает окно с выбором заданий и вводом информации о студенте
        /// </summary>
        private void ShowFormReportsSelect(object sender, RoutedEventArgs e)
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
    }
}
