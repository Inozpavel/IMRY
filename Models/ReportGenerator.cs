using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Media.Imaging;
using WorkReportCreator.Views;
using Xceed.Document.NET;
using Xceed.Words.NET;

namespace WorkReportCreator.Models
{
    internal class ReportGenerator
    {
        private readonly string _reportName;
        private readonly List<int> _selectedWorksNumbers;
        private readonly List<FileInformationItem> _filesInformation;

        public ReportGenerator(string reportName, IEnumerable<int> selectedWorksNumbers, IEnumerable<FileInformationItem> filesInformation)
        {
            _reportName = reportName;
            _selectedWorksNumbers = selectedWorksNumbers.ToList();
            _filesInformation = filesInformation.ToList();
        }

        /// <summary>
        /// Cоздает отчет для работы
        /// </summary>
        /// <exception cref="Exception"/>
        public void GenerateReport()
        {
            MainParams mainParams = new MainParams();
            DocX document = mainParams.WorkHasTitlePage ? GenerateTitlePage() : DocX.Create("./Configs/EmptyDocument.docs");

            AddWorkInformation(document, _reportName);
            AddSelectedTasks(document, _reportName);
            AddUserFiles(document);
            InsertAllImages(document);

            if (Directory.Exists(mainParams.ReportsPath) == false)
                Directory.CreateDirectory(mainParams.ReportsPath);
            document.SaveAs($"{mainParams.ReportsPath}/Отчет {_reportName}.docx");
        }

        /// <summary>
        /// Создает титульник для отчета
        /// </summary>
        /// <returns><see cref="DocX"/> - Титульник</returns>
        /// <exception cref="Exception"/>
        private DocX GenerateTitlePage()
        {
            MainParams mainParams = new MainParams();
            if (mainParams.WorkHasTitlePage == false)
                throw new Exception("Вызвано создание титульника, хотя его быть не должно!");

            if (mainParams.WorkHasTitlePageParams && File.Exists(mainParams.WorkTitlePageParamsFilePath) == false)
                throw new Exception("Файл с параметрами для титульной страницы отсутствует!");

            Student student;
            try
            {
                student = JsonConvert.DeserializeObject<Student>(File.ReadAllText(mainParams.UserDataFilePath));
            }
            catch (Exception)
            {
                throw new Exception("Не получилось загрузить информацию о студенте!");
            }

            if (File.Exists(mainParams.WorkTitlePageFilePath) == false)
                throw new Exception("Файл с титульной страницей отсутствует!");

            DocX doc = DocX.Load(mainParams.WorkTitlePageFilePath);
            if (mainParams.WorkHasTitlePageParams)
            {
                Dictionary<string, string> titlePageParams = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText(mainParams.WorkTitlePageParamsFilePath));
                titlePageParams.Add("Group", student.Group);
                if (student.UseFullName)
                {
                    titlePageParams.Add("StudentFullName", string.Join(" ", student.SecondName,
                       student.FirstName, student.MiddleName));
                }
                else
                {
                    titlePageParams.Add("StudentFullName", string.Join(" ", student.SecondName,
                       student.FirstName.Substring(0, 1).ToUpper() + ".", student.MiddleName.Substring(0, 1).ToUpper() + "."));
                }

                foreach (string key in titlePageParams.Keys)
                    doc.ReplaceText("{{" + key + "}}", titlePageParams[key]);
            }

            return doc;
        }

        /// <summary>
        /// Земеняет информацию о работе в {{ }} на данные
        /// </summary>
        /// <param name="document">Документ</param>
        /// <param name="reportName">Название отчета</param>
        /// <returns>Документ со вставленной информацией о работе</returns>
        /// <exception cref="Exception"/>
        private void AddWorkInformation(DocX document, string reportName)
        {
            MainParams mainParams = new MainParams();
            try
            {
                var template = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, Report>>>(File.ReadAllText(mainParams.CurrentTemplateFilePath));
                Report task = template[Regex.IsMatch(reportName, "пр|Пр") ? "Practices" : "Laboratories"][Regex.Match(reportName, @"\d+").Value];
                document.ReplaceText("{{WorkType}}", $"{(Regex.IsMatch(reportName, "пр|Пр") ? "Практическая работа" : "Лабораторная работа")}");
                document.ReplaceText("{{WorkNumber}}", $"{Regex.Match(reportName, @"\d+").Value}");
                document.ReplaceText("{{WorkName}}", $"{task.Name}");
                document.ReplaceText("{{WorkTheoryPart}}", $"{task.TheoryPart}");
                document.ReplaceText("{{WorkTarget}}", $"{task.WorkTarget}");
                document.ReplaceText("{{CommonTask}}", $"{task.CommonTask}");
            }
            catch (Exception)
            {
                throw new Exception("Не удалось вставить в документ информацию о работе!");
            }
        }

        /// <summary>
        /// Вставляет в отчет выбранные пользователем работы вместо {{DynamicTasks}}
        /// <param name="document">Документ</param>
        /// <param name="reportName">Название отчета</param>
        /// <returns>Документ со вставленной информацией о работе</returns>
        /// <exception cref="Exception"/>
        private void AddSelectedTasks(DocX document, string reportName)
        {
            int dynamicTasksParagraphIndex = FindParagraphIndexWithParametr(document, "DynamicTasks"); //номер абраза, с котоого надо начинать вставлять задания

            if (dynamicTasksParagraphIndex == -1) //Задания вставлять не нужно
                return;


            if (_selectedWorksNumbers.Count < 0) // Заданий нет
                return;

            MainParams mainParams = new MainParams();
            Dictionary<string, Dictionary<string, Report>> template;
            try
            {
                template = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, Report>>>(File.ReadAllText(mainParams.CurrentTemplateFilePath));
            }
            catch (Exception)
            {
                MessageBox.Show("Не удалось вставить в документ выбранные задания!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var tasks = template[Regex.IsMatch(reportName, "пр|Пр") ? "Practices" : "Laboratories"][Regex.Match(reportName, @"\d+").Value].DynamicTasks.Select(x => x.Description.Trim()).ToList();
            tasks = tasks.Select(x => Regex.Replace(x, "•", "\t•")).ToList();

            document.RemoveParagraphAt(dynamicTasksParagraphIndex); //Удаляем надпись {{DynamicTasks}}
            dynamicTasksParagraphIndex--;

            int number = 1;
            List<(string, int, string, string)> paragraps = new List<(string, int, string, string)>();

            foreach (int i in _selectedWorksNumbers) // Вставка всех работ по абзацам
            {
                paragraps.Add((_selectedWorksNumbers.Count > 1 ? $"\t{number}) " + tasks[i] : "\t" + tasks[i], 14, "normal", "Times New Roman"));
                number++;
            }

            InsertParagrapsAfterParagraphIndex(document, paragraps, dynamicTasksParagraphIndex);
        }

        /// <summary>
        /// Вставляет в документ указанные абзацы после указанного
        /// </summary>
        /// <param name="document">Документ, в который будет вставка</param>
        /// <param name="paragraphs">Абзацы</param>
        /// <param name="paragraphIndex">Индекс параграфа</param>
        /// <param name="FontFamily">Шрифт</param>
        private void InsertParagrapsAfterParagraphIndex(DocX document, List<(string text, int fontSize, string style, string fontFamily)> paragraphs, int paragraphIndex)
        {
            for (int i = 0; i < paragraphs.Count; i++)
            {
                Paragraph paragraph = document.InsertParagraph(paragraphs[i].text).FontSize(paragraphs[i].fontSize).Font(paragraphs[i].fontFamily);

                if (paragraphs[i].style == "bold")
                    paragraph = paragraph.Bold();

                document.Paragraphs[paragraphIndex].InsertParagraphAfterSelf(paragraph);
                document.RemoveParagraphAt(document.Paragraphs.Count - 1); // пакет зачем-то вставляет еще один в конец
                paragraphIndex++;
            }
        }

        /// <summary>
        /// Вставляет в отчет добавленные пользователем файлы в конец отчета
        /// </summary>
        /// <param name="document">Документ</param>
        /// <param name="reportName">Название отчета</param>
        /// <returns>Документ со вставленной информацией о работе</returns>
        /// <exception cref="Exception"/>
        private void AddUserFiles(DocX document)
        {
            int userFilesParagraphIndex = FindParagraphIndexWithParametr(document, "UserFiles");
            if (userFilesParagraphIndex == -1)
                return;

            document.RemoveParagraphAt(userFilesParagraphIndex); //Удаляем надпись {{UserFiles}}
            userFilesParagraphIndex--;

            if (_filesInformation.Count == 0)
                return;

            List<(string text, int fontSize, string style, string fontFamily)> paragraphs = new List<(string, int, string, string)>();


            foreach (FileInformationItem fileInformation in _filesInformation)
            {
                try
                {
                    BitmapImage image = new BitmapImage(new Uri(fileInformation.FilePath));
                    string name = string.IsNullOrEmpty(fileInformation.FileDescription) ? "" : ",name=\"" + fileInformation.FileDescription + "\"";
                    paragraphs.Add(("{{image source=\"" + fileInformation.FilePath + "\"" + name + "}}", 10, "normal", "Times New Roman"));
                }
                catch (Exception)
                {
                    if (string.IsNullOrEmpty(fileInformation.FileDescription) == false)
                        paragraphs.Add((fileInformation.FileDescription, 14, "normal", "Times New Roman"));

                    paragraphs.Add((fileInformation.FileName.Split('\\').Last(), 16, "bold", "Tahoma"));
                    paragraphs.Add((string.Join("\n", File.ReadAllLines(fileInformation.FilePath)), 10, "normal", "Consolas"));
                }
                paragraphs.Add(("", 10, "normal", "Consolas"));
            }
            InsertParagrapsAfterParagraphIndex(document, paragraphs, userFilesParagraphIndex);
            return;
        }


        /// <summary>
        /// Заменяет все {{image}} в которых указан путь на картинки
        /// </summary>
        /// <param name="doc">Документ</param>
        /// <returns>Документ со вставленными картинками</returns>
        private void InsertAllImages(DocX document)
        {
            List<string> paragraphs = document.Paragraphs.Cast<Paragraph>().Select(x => x.Text).ToList();// После вставки всех работ список изменился
            int imagesCount = 0;
            string sourcePattern = "source\\s*=\\s*\"[^\"]+\"";
            string namePattern = "name\\s*=\"[^\"]*\"";
            for (int i = 0; i < paragraphs.Count; i++)
            {

                if (Regex.IsMatch(paragraphs[i], "{{\\s*image\\s+" + sourcePattern + "(,?\\s*" + namePattern + ")?\\s*}}"))
                {
                    var matches = Regex.Matches(paragraphs[i], "{{\\s*image\\s+" + sourcePattern + "(,?\\s*" + namePattern + ")?\\s*}}").Cast<Match>().Select(x => x.Value).ToList();
                    foreach (string image in matches)
                    {
                        try
                        {
                            string imagePath = Regex.Match(Regex.Match(image, sourcePattern).Value, "\".+\"").Value.Trim('"');
                            imagePath = imagePath.Replace('\\', '/');
                            if (string.IsNullOrEmpty(imagePath) || File.Exists(imagePath) == false)
                                continue;

                            string imageName = Regex.Match(Regex.Match(image, namePattern).Value, "\".*\"").Value.Trim('"');
                            Paragraph paragraph = document.InsertParagraph();

                            document.RemoveParagraphAt(document.Paragraphs.Count - 1);

                            var insertedImage = document.AddImage(imagePath).CreatePicture();

                            float maxWidth = document.PageWidth - 150;
                            if (insertedImage.Width > maxWidth)
                            {
                                float aspectRatio = insertedImage.Height / insertedImage.Width;
                                insertedImage.Width = maxWidth;
                                insertedImage.Height = maxWidth * aspectRatio;
                            }

                            paragraph.AppendPicture(insertedImage);
                            paragraph.AppendLine("Рис. " + (imagesCount + 1) + " " + (string.IsNullOrEmpty(imageName) ? "" : imageName)).FontSize(12).Font("Times New Roman");

                            paragraph.Alignment = Alignment.center;
                            document.Paragraphs[i + imagesCount].InsertParagraphAfterSelf(paragraph);

                            document.ReplaceText(image, "");
                            imagesCount++;
                        }
                        catch (Exception)
                        {
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Ищет во всех абзацах документа указанный параметр в {{ }}
        /// </summary>
        /// <param name="document">Документ, в котором будет поиск</param>
        /// <param name="parametr">Название параметра</param>
        /// <returns>Индекс параметра, если он найден, иначе -1</returns>
        private int FindParagraphIndexWithParametr(DocX document, string parametr)
        {
            int index = -1;
            var paragraphs = document.Paragraphs.Cast<Paragraph>().Select(x => x.Text).ToList();
            for (int i = 0; i < paragraphs.Count; i++)
            {
                if (paragraphs[i].Contains("{{" + parametr + "}}"))
                {
                    index = i;
                    break;
                }
            }
            return index;
        }
    }
}
