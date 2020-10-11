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
    public partial class ReportView : UserControl
    {
        private readonly ReportViewModel _model;
        private readonly ReportsPage _page;
        public ReportView(ReportsPage page, List<string> DynamicTasks)
        {
            InitializeComponent();
            _model = new ReportViewModel(DynamicTasks);
            DataContext = _model;
            listBox.SelectionChanged += (sender, e) => listBox.ScrollIntoView(listBox.SelectedItem);
            _page = page;
        }

        public void GenerateReport(object sender, RoutedEventArgs e)
        {
            try
            {
                var reportName = (Parent as TabItem).Header.ToString().Trim().TrimEnd(".".ToCharArray());
                DocX document = GenerateTitlePage();
                var globalParameters = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText("./GlobalConfig.json"));
                var reportsFilesPath = globalParameters["AllReportsFilePath"];
                if (Directory.Exists(reportsFilesPath) == false)
                    Directory.CreateDirectory(reportsFilesPath);
                document.SaveAs($"{reportsFilesPath}/Отчет {reportName}.docx");
            }
            catch (IOException)
            {
                MessageBox.Show("Не получилось сохранить отчет!\nВозможно, вы не закрыли файл с титульником.\n", "Внимание!", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private DocX GenerateTitlePage()
        {
            var globalParams = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText("./GlobalConfig.json"));
            var titlePageParams = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText(globalParams["TitlePageParametersPath"]));

            StudentInformation student = _page.StudentInformation;

            titlePageParams.Add("Group", student.Group);
            titlePageParams.Add("StudentFullName", string.Join(" ", student.SecondName, student.FirstName, student.MiddleName));

            DocX doc = DocX.Load(globalParams["TitlePagePath"]);
            foreach (string key in titlePageParams.Keys)
            {
                doc.ReplaceText($"{{{{{key}}}}}", $"{titlePageParams[key]}");
            }
            return doc;
        }

        private void AddAllFiles(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            var globalParams = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText("./GlobalConfig.json"));

            var permittedWorksAndExtentions = JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(
                File.ReadAllText(globalParams["PermittedWorksAndExtentionsPath"]));

            var permittedExtentions = permittedWorksAndExtentions["PermittedFilesExtentions"];
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

        private void AddFilePaths(List<string> filePaths, List<string> permittedExtentions)
        {
            foreach (string filePath in filePaths)
            {
                if (CheckFileExtentionIsPermitted(filePath, permittedExtentions) &&
                    _model.Array.Select(x => x.Content as ReportMenuItem).Select(x => x.CodeFilePath).ToList().Contains(filePath) == false)
                    _model.AddNewFileInfoWithFile(filePath);
            }
        }

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
                    _model.AddNewFileInfoWithFile(name);
                }
            }
        }

        private bool CheckFileExtentionIsPermitted(string fileName, List<string> permittedExtentions) => permittedExtentions.Any(ext => Regex.IsMatch(fileName, $@"\.{ext}$"));
    }
}
