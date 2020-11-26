using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using WorkReportCreator.Models;
using WorkReportCreator.Views;

namespace WorkReportCreator
{
    /// <summary>
    /// Вкладка для просмотра информации о работе
    /// </summary>
    public partial class ReportItem : UserControl
    {
        private enum ReportAction
        {
            Save,
            Generate,
        }

        /// <summary>
        /// Модель данных этого элемента
        /// </summary>
        private readonly ReportViewModel _model;

        /// <param name="window">Окно, на котором расположен элемент</param>
        /// <param name="DynamicTasks">Список заданий (при наличии)</param>
        public ReportItem(List<string> DynamicTasks, ReportModel report)
        {
            InitializeComponent();
            _model = new ReportViewModel(DynamicTasks, report);
            DataContext = _model;
            listBox.SelectionChanged += (sender, e) => listBox.ScrollIntoView(listBox.SelectedItem);
        }

        /// <summary>
        /// Создает отчет для выбранной работы
        /// </summary>
        /// <exception cref="Exception"/>
        public void GenerateReport() => _model.GenerateReport();

        /// <summary>
        /// Сохраняет отчет для выбранной работы
        /// </summary>
        /// <exception cref="Exception"/>
        public void SaveReport() => _model.SaveReport();

        /// <summary>
        /// При использовании Drag & Drop добавляет / дополняет информацию о выбранных файлах с список информации о файлах
        /// </summary>
        private void AddFilesFromDragAndDrop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            MainParams mainParams = new MainParams();
            List<string> permittedExtentions;
            try
            {
                permittedExtentions = JsonConvert.DeserializeObject<List<string>>(
                    File.ReadAllText(mainParams.PermittedDragAndDropExtentionsFilePath));
            }
            catch (Exception)
            {
                MessageBox.Show("Ошибка при чтении данным из файла с расширениями!",
                    "Ошибка!", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            bool shouldAddAll = permittedExtentions.Contains("*");
            bool allFilesIsFilePath = files.All(path => Directory.Exists(path) == false);
            bool allFilesIsDirectoryPath = files.All(path => Directory.Exists(path));

            if (allFilesIsFilePath == false)
            {
                if (allFilesIsDirectoryPath == false)
                {
                    if (MessageBox.Show(
                "Вы выбрали не только файлы, но и папки!\nХотите осуществить поиск файлов в папках рекурсивно?", "Внимание!",
                    MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No) == MessageBoxResult.Yes)
                        RecursiveAddFile(files.ToList(), permittedExtentions, shouldAddAll);
                }
                else
                    RecursiveAddFile(files.ToList(), permittedExtentions, shouldAddAll);
            }
            else
                AddFilePaths(files.ToList(), permittedExtentions, shouldAddAll);

            ChangeImageToTakenIn(this, null);
        }

        /// <summary>
        /// Добавляет все файлы из списка <paramref name="filePaths"/> в список информации о файлах
        /// </summary>
        /// <param name="filePaths">Список путей файлов</param>
        /// <param name="permittedExtentions">Список разрешенных расширений</param>
        private void AddFilePaths(List<string> filePaths, List<string> permittedExtentions, bool shouldAddAll)
        {
            foreach (string filePath in filePaths)
            {
                if (shouldAddAll && _model.FilesArray.Select(x => x.Content as FileInformationItem).Select(x => x.FilePath).ToList().Contains(filePath) == false)
                    _model.AddNewFileInfoWithFilePath(filePath);
                else if ((shouldAddAll || CheckFileExtentionIsPermitted(filePath, permittedExtentions)) &&
                    _model.FilesArray.Select(x => x.Content as FileInformationItem).Select(x => x.FilePath).ToList().Contains(filePath) == false)
                    _model.AddNewFileInfoWithFilePath(filePath);
            }
        }

        /// <summary>
        /// Добавляет все файлы из списка <paramref name="paths"/> в список информации о файлах, если в списке будет папке, будет искать в ней файлы рекурсивно
        /// </summary>
        /// <param name="filePaths">Список путей файлов</param>
        /// <param name="permittedExtentions">Список разрешенных расширений</param>
        private void RecursiveAddFile(List<string> paths, List<string> permittedExtentions, bool shouldAddAll)
        {
            foreach (var name in paths)
            {
                if (Directory.Exists(name))
                {
                    var internalNames = Directory.GetFiles(name, "*.*", SearchOption.AllDirectories).ToList();
                    if (shouldAddAll == false)
                        internalNames = internalNames.Where(x => CheckFileExtentionIsPermitted(x, permittedExtentions)).ToList();
                    AddFilePaths(internalNames, permittedExtentions, shouldAddAll);
                }
                else if (shouldAddAll || CheckFileExtentionIsPermitted(name, permittedExtentions))
                    _model.AddNewFileInfoWithFilePath(name);
            }
        }

        /// <summary>
        /// Проверяет, являет расширение файла допустимым
        /// </summary>
        /// <param name="fileName">Имя файла</param>
        /// <param name="permittedExtentions">Список разрешенных расширений</param>
        /// <returns><paramref name="True"/>, если разрешено, в противном случае <paramref name="false"/></returns>
        private bool CheckFileExtentionIsPermitted(string fileName, List<string> permittedExtentions) => permittedExtentions.Any(ext => Regex.IsMatch(fileName, $@"\.{ext}$"));

        /// <summary>
        /// Изменяет картинку на папку с вытащенными файлами
        /// </summary>
        private void ChangeImageToTakenOut(object sender, DragEventArgs e) => image.Source = new BitmapImage(new Uri("/Images/FolderTakenOut.png", UriKind.Relative));

        /// <summary>
        /// Изменяет картинку на папку со вставленными файлами
        /// </summary>
        private void ChangeImageToTakenIn(object sender, DragEventArgs e) => image.Source = new BitmapImage(new Uri("/Images/FolderTakenIn.png", UriKind.Relative));

        /// <summary>
        /// Создает отчет для работы, показывает все ошибки
        /// </summary>
        private void GenerateReportClicked(object sender, RoutedEventArgs e) => ExecuteWithReport(ReportAction.Generate, "создать", "создан");

        private void SaveReportClicked(object sender, RoutedEventArgs e) => ExecuteWithReport(ReportAction.Save, "сохранить", "сохранен");

        private void ExecuteWithReport(ReportAction action, string actionName, string actionNameInPastSimple)
        {
            MainParams mainParams = new MainParams();
            bool isDone;
            try
            {
                if (action == ReportAction.Generate)
                    GenerateReport();
                else if (action == ReportAction.Save)
                    SaveReport();
                isDone = true;
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message, $"Ошибка! Не получилось {actionName} отчет!", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (isDone && MessageBox.Show("Открыть папку с отчетами?", $"Отчет успешно {actionNameInPastSimple}!",
                MessageBoxButton.YesNo, MessageBoxImage.Information, MessageBoxResult.No) == MessageBoxResult.Yes)
            {
                string path = "";
                if (action == ReportAction.Generate)
                    path = mainParams.ReportsPath;
                else if (action == ReportAction.Save)
                    path = mainParams.SavedReportsPath;
                if (Directory.Exists(path))
                    Process.Start(path.StartsWith(".") || path.StartsWith("/") ? Directory.GetCurrentDirectory() + path : path);
                else
                    MessageBox.Show("Не получилось найти папку с отчетами!", "Ошибка!", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
        }
    }
}
