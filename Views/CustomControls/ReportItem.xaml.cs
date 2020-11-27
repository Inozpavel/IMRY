using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
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
        public void GenerateReport(List<int> selectedIndicies, List<FileInformation> filesInformation) => _model.GenerateReport(selectedIndicies, filesInformation);

        /// <summary>
        /// Сохраняет отчет для выбранной работы
        /// </summary>
        /// <exception cref="Exception"/>
        public void SaveReport() => _model.SaveReport();

        public void AddImageFromBuffer(BitmapSource imageSource) => _model.AddImageFromBuffer(imageSource);

        /// <summary>
        /// При использовании Drag & Drop добавляет / дополняет информацию о выбранных файлах с список информации о файлах
        /// </summary>
        private void AddFilesFromDragAndDrop(object sender, DragEventArgs e)
        {
            List<string> existingPaths = _model.FilesArray.Select(x => x.Content as FileInformationItem).Select(x => x.FilePath).ToList();
            List<string> pathsToAdd = ((string[])e.Data.GetData(DataFormats.FileDrop)).ToList();
            AddFiles(existingPaths, pathsToAdd);
        }

        /// <summary>
        /// Добавляет файлы в отчет
        /// </summary>
        /// <param name="existingPaths">Существующие файла в отчете</param>
        /// <param name="pathsToAdd">Файлы, которые надо добавить</param>
        private void AddFiles(List<string> existingPaths, List<string> pathsToAdd)
        {
            List<string> paths = new List<string>();
            Task task = new Task(() =>
            {
                bool allPathsIsFolders = pathsToAdd.All(path => Directory.Exists(path));
                bool allPathsIsFiles = pathsToAdd.All(path => File.Exists(path));
                bool useRecursiveSearch = false;

                if (allPathsIsFiles == allPathsIsFolders)
                {
                    useRecursiveSearch = MessageBox.Show("Вы выбрали не только файлы, но и папки!\nХотите осуществить поиск файлов в папках рекурсивно?",
                   "Внимание!", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No) == MessageBoxResult.Yes;
                }
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


                if (allPathsIsFiles)
                    paths = GetFilesPaths(existingPaths, pathsToAdd, permittedExtentions, shouldAddAll);
                else if (useRecursiveSearch || allPathsIsFolders)
                    paths = RecursiveGetPaths(existingPaths, pathsToAdd, permittedExtentions, shouldAddAll);
                else
                    paths = GetFilesPaths(existingPaths, pathsToAdd, permittedExtentions, shouldAddAll);


            });
            task.ContinueWith((x) =>
            {
                Task.Run(() =>
                {
                    Dispatcher.Invoke(() =>
                    {
                        foreach (string path in paths)
                        {
                            _model.AddNewFileInfoWithFilePath(path);
                        }
                    });
                });
            });
            task.Start();
            ChangeImageToTakenIn(this, null);
        }

        /// <summary>
        /// Добавляет все файлы из списка <paramref name="filePaths"/> в список информации о файлах
        /// </summary>
        /// <param name="filePaths">Список путей файлов</param>
        /// <param name="permittedExtentions">Список разрешенных расширений</param>
        private List<string> GetFilesPaths(List<string> existingPaths, List<string> filePaths, List<string> permittedExtentions, bool shouldAddAll)
        {
            List<string> paths = new List<string>();
            foreach (string filePath in filePaths)
            {
                if (shouldAddAll && existingPaths.Contains(filePath) == false)
                    paths.Add(filePath);
                else if ((shouldAddAll || CheckFileExtentionIsPermitted(filePath, permittedExtentions)) && existingPaths.Contains(filePath) == false)
                    paths.Add(filePath);
            }
            return paths;
        }

        /// <summary>
        /// Добавляет все файлы из списка <paramref name="paths"/> в список информации о файлах, если в списке будет папке, будет искать в ней файлы рекурсивно
        /// </summary>
        /// <param name="filePaths">Список путей файлов</param>
        /// <param name="permittedExtentions">Список разрешенных расширений</param>
        private List<string> RecursiveGetPaths(List<string> existingPaths, List<string> pathsToAdd, List<string> permittedExtentions, bool shouldAddAll)
        {
            List<string> paths = new List<string>();
            foreach (var path in pathsToAdd)
            {
                if (Directory.Exists(path))
                {
                    var internalFilesPaths = Directory.GetFiles(path, "*.*", SearchOption.AllDirectories).ToList();
                    if (shouldAddAll == false)
                        internalFilesPaths = internalFilesPaths.Where(x => CheckFileExtentionIsPermitted(x, permittedExtentions)).ToList();
                    paths = paths.Union(GetFilesPaths(existingPaths, internalFilesPaths, permittedExtentions, shouldAddAll)).ToList();
                }
                else if (shouldAddAll || CheckFileExtentionIsPermitted(path, permittedExtentions))
                    paths.Add(path);
            }
            return paths;
        }

        /// <summary>
        /// Проверяет, являет расширение файла допустимым
        /// </summary>
        /// <param name="fileName">Имя файла</param>
        /// <param name="permittedExtentions">Список разрешенных расширений</param>
        /// <returns><paramref name="True"/>, если разрешено, в противном случае <paramref name="false"/></returns>
        private bool CheckFileExtentionIsPermitted(string fileName, List<string> permittedExtentions) => permittedExtentions.Any(ext => Regex.IsMatch(fileName, $@"\.{ext}$", RegexOptions.IgnoreCase));

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

        public void GetSelectedTasksAndFilesInformation(out List<int> selectedTasks, out List<FileInformation> filesInformation)
        {
            selectedTasks = new List<int>();
            for (int i = 0; i < _model.DynamicTasksArray.Count; i++)
            {
                if (_model.DynamicTasksArray[i].IsChecked)
                    selectedTasks.Add(i);
            }
            filesInformation = _model.FilesArray.Select(x => x.Content as FileInformationItem)
                .Select(x => new FileInformation()
                {
                    FilePath = x.FilePath,
                    FileName = x.FileName,
                    FileDescription = x.FileDescription,
                }).ToList();
        }

        private void ExecuteWithReport(ReportAction action, string actionName, string actionNameInPastSimple)
        {
            MainParams mainParams = new MainParams();

            GetSelectedTasksAndFilesInformation(out List<int> selectedTasks, out List<FileInformation> filesInformation);
            try
            {
                if (action == ReportAction.Generate)
                    GenerateReport(selectedTasks, filesInformation);
                else if (action == ReportAction.Save)
                    SaveReport();
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message, $"Ошибка! Не получилось {actionName} отчет!", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (MessageBox.Show("Открыть папку с отчетами?", $"Отчет успешно {actionNameInPastSimple}!",
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
