using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using Xceed.Words.NET;

namespace WorkReportCreator
{
    /// <summary>
    /// Логика взаимодействия для ReportView.xaml
    /// </summary>
    public partial class ReportView : UserControl
    {
        private readonly ReportViewModel _model = new ReportViewModel();
        private readonly ReportsPage _page;
        public ReportView(ReportsPage page)
        {
            InitializeComponent();
            DataContext = _model;
            listBox.SelectionChanged += (sender, e) => listBox.ScrollIntoView(listBox.SelectedItem);
            _page = page;
        }

        private void GenerateReport(object sender, RoutedEventArgs e)
        {
            try
            {
                DocX document = GenerateTitlePage();
                document.SaveAs("./Report.docx");
            }
            catch (IOException)
            {
                MessageBox.Show("Не получилось сохранить отчет!\nВозможно вы не закрыли файл.", "Внимание!", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private DocX GenerateTitlePage()
        {
            Dictionary<string, string> titlePageParams;
            using (var file = new StreamReader("./TitlePageParams.json"))
            {
                titlePageParams = JsonConvert.DeserializeObject<Dictionary<string, string>>(file.ReadToEnd());
            }

            StudentInformation student = _page.StudentInformation;

            titlePageParams.Add("Group", student.Group);
            titlePageParams.Add("StudentFullName", string.Join(" ", student.SecondName, student.FirstName, student.MiddleName));

            string titlePagePath = "./TitlePage.docx";
            DocX doc = DocX.Load(titlePagePath);
            foreach (string key in titlePageParams.Keys)
            {
                doc.ReplaceText($"{{{{{key}}}}}", $"{titlePageParams[key]}");
            }
            return doc;
        }
    }
}
