using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using WorkReportCreator.Views;
using Xceed.Words.NET;

namespace WorkReportCreator
{
    /// <summary>
    /// Вкладка для просмотра информации о работе
    /// </summary>
    public partial class ReportItem : UserControl
    {
        /// <summary>
        /// Модель данных этого элемента
        /// </summary>
        private readonly ReportViewModel _model;

        /// <summary>
        /// Окно, на котором расположен элемент
        /// </summary>
        private readonly ReportsWindow _page;

        /// <param name="page">Окно, на котором расположен элемент</param>
        /// <param name="DynamicTasks">Список заданий (при наличии)</param>
        public ReportItem(ReportsWindow page, List<string> DynamicTasks)
        {
            InitializeComponent();
            _model = new ReportViewModel(DynamicTasks);
            DataContext = _model;
            _page = page;
            listBox.SelectionChanged += (sender, e) => listBox.ScrollIntoView(listBox.SelectedItem);
        }

        /// <summary>
        /// Cоздает отчет для работы
        /// </summary>
        public void GenerateReport(object sender, RoutedEventArgs e)
        {
            try
            {
                var reportName = (Parent as TabItem).Header.ToString().Trim().TrimEnd(".".ToCharArray());
                DocX document = GenerateTitlePage();
                var globalParameters = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText("./MainConfig.json"));
                var reportsFilesPath = globalParameters["AllReportsPath"];
                if (Directory.Exists(reportsFilesPath) == false)
                    Directory.CreateDirectory(reportsFilesPath);
                document.SaveAs($"{reportsFilesPath}/Отчет {reportName}.docx");
            }
            catch (IOException)
            {
                MessageBox.Show("Не получилось сохранить отчет!\nВозможно, вы не закрыли файл с титульником.\n", "Внимание!", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Создает титульник для отчета
        /// </summary>
        /// <returns><see cref="DocX"/> - Титульник</returns>
        private DocX GenerateTitlePage()
        {
            var globalParams = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText("./MainConfig.json"));
            var titlePageParams = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText(globalParams["TitlePageParametersFilePath"]));

            StudentInformation student = _page.Student;
            titlePageParams.Add("Group", student.Group);
            titlePageParams.Add("StudentFullName", string.Join(" ", student.SecondName, student.FirstName, student.MiddleName));

            DocX doc = DocX.Load(globalParams["TitlePageFilePath"]);
            foreach (string key in titlePageParams.Keys)
            {
                doc.ReplaceText($"{{{{{key}}}}}", $"{titlePageParams[key]}");
            }
            return doc;
        }

        /// <summary>
        /// При использовании Drag & Drop добавляет / дополняет информацию о выбранных файлах с список информации о файлах
        /// </summary>
        private void AddAllFiles(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            var globalParams = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText("./MainConfig.json"));

            List<string> permittedExtentions = JsonConvert.DeserializeObject<List<string>>(
                File.ReadAllText(globalParams["PermittedDragAndDropExtentionsFilePath"]));
            bool allFilesIsFilePath = files.All(path => Directory.Exists(path) == false);
            bool allFilesIsDirectoryPath = files.All(path => Directory.Exists(path));

            if (allFilesIsFilePath == false)
            {
                if (allFilesIsDirectoryPath == false)
                {
                    if (MessageBox.Show(
                "Вы выбрали не только файлы, но и папки!\nХотите осуществить поиск файлов в папках рекурсивно?", "Внимание!",
                    MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No) == MessageBoxResult.Yes)
                    {
                        RecursiveAddFile(files.ToList(), permittedExtentions);
                    }
                }
                else
                {
                    RecursiveAddFile(files.ToList(), permittedExtentions);
                }
            }
            else
            {
                AddFilePaths(files.ToList(), permittedExtentions);
            }
        }

        /// <summary>
        /// Добавляет все файлы из списка <paramref name="filePaths"/> в список информации о файлах
        /// </summary>
        /// <param name="filePaths">Список путей файлов</param>
        /// <param name="permittedExtentions">Список разрешенных расширений</param>
        private void AddFilePaths(List<string> filePaths, List<string> permittedExtentions)
        {
            foreach (string filePath in filePaths)
            {
                if (CheckFileExtentionIsPermitted(filePath, permittedExtentions) &&
                    _model.FilesArray.Select(x => x.Content as FileInformationItem).Select(x => x.FilePath).ToList().Contains(filePath) == false)
                    _model.AddNewFileInfoWithFilePath(filePath);
            }
        }

        /// <summary>
        /// Добавляет все файлы из списка <paramref name="paths"/> в список информации о файлах, если в списке будет папке, будет искать в ней файлы рекурсивно
        /// </summary>
        /// <param name="filePaths">Список путей файлов</param>
        /// <param name="permittedExtentions">Список разрешенных расширений</param>
        private void RecursiveAddFile(List<string> paths, List<string> permittedExtentions)
        {
            foreach (var name in paths)
            {
                if (Directory.Exists(name))
                {
                    var internalNames = Directory.GetFiles(name, "*.*", SearchOption.AllDirectories)
                         .Where(x => CheckFileExtentionIsPermitted(x, permittedExtentions)).ToList();
                    AddFilePaths(internalNames, permittedExtentions);
                }
                else if (CheckFileExtentionIsPermitted(name, permittedExtentions))
                {
                    _model.AddNewFileInfoWithFilePath(name);
                }
            }
        }

        /// <summary>
        /// Проверяет, являет расширение файла допустимым
        /// </summary>
        /// <param name="fileName">Имя файла</param>
        /// <param name="permittedExtentions">Список разрешенных расширений</param>
        /// <returns><paramref name="True"/>, если разрешено, в противном случае <paramref name="false"/></returns>
        private bool CheckFileExtentionIsPermitted(string fileName, List<string> permittedExtentions) => permittedExtentions.Any(ext => Regex.IsMatch(fileName, $@"\.{ext}$"));
    }
}
