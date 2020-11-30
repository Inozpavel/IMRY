using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Media.Imaging;
using Xceed.Document.NET;
using Xceed.Words.NET;

namespace WorkReportCreator.Models
{
    internal class ReportGenerator
    {
        private readonly string _reportName;

        private readonly List<int> _selectedWorksNumbers;

        private readonly List<FileInformation> _filesInformation;

        private const string SourcePattern = "source\\s*=\\s*\"[^\"]+\"";
        private const string NamePattern = "name\\s*=\"[^\"]*\"";
        private const string ImagePattern = "{{\\s*image\\s+" + SourcePattern + "(,?\\s*" + NamePattern + ")?\\s*}}";

        public ReportGenerator(string reportName, IEnumerable<int> selectedWorksNumbers, IEnumerable<FileInformation> filesInformation)
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
            DocX document = mainParams.WorkHasTitlePage ? GenerateTitlePage() : DocX.Create("./EmptyDocument.docx");

            AddWorkInformation(document, _reportName);
            AddSelectedTasks(document, _reportName);
            AddUserFiles(document);
            InsertAllImages(document);

            if (Directory.Exists(mainParams.ReportsPath) == false)
                Directory.CreateDirectory(mainParams.ReportsPath);
            document.SaveAs($"{mainParams.ReportsPath}/Отчет {_reportName.TrimEnd('.')}.docx");
        }

        /// <summary>
        /// Создает титульник для отчета
        /// </summary>
        /// <returns><see cref="DocX"/> - Титульник</returns>
        /// <exception cref="Exception"/>
        private DocX GenerateTitlePage()
        {
            MainParams mainParams = new MainParams();

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
                    titlePageParams.Add("UserName", string.Join(" ", student.SecondName, student.FirstName, student.MiddleName));
                }
                else
                {
                    titlePageParams.Add("UserName", string.Join(" ", student.SecondName,
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
                document.ReplaceText("{{WorkTarget}}", $"{task.WorkTarget}");
                document.ReplaceText("{{Conclusions}}", $"{task.Conclusions}");

                InsertTextInParam(document, "TheoryPart", task.TheoryPart);
                InsertTextInParam(document, "CommonTask", task.CommonTask);
            }
            catch (Exception)
            {
                throw new Exception("Не удалось вставить в документ информацию о работе!");
            }
        }

        private void InsertTextInParam(DocX document, string paramName, string text)
        {
            int index = FindParagraphIndexWithParametr(document, paramName); //номер абраза, с которого надо начинать вставлять задания
            if (index == -1)
                return;

            if (_selectedWorksNumbers.Count < 0)
                return;

            MainParams mainParams = new MainParams();
            document.RemoveParagraphAt(index);

            var images = FindImages(text);
            var splittedTask = Regex.Split(text, ImagePattern).Where(x => Regex.IsMatch(x, NamePattern) == false).ToList();
            index--;

            List<(string text, int fontSize, string style, string fontFamily)> paragraphs = new List<(string, int, string, string)>();


            for (int j = 0; j < splittedTask.Count; j++)
            {
                paragraphs.Add(("\t" + splittedTask[j], 14, "normal", "Times New Roman"));

                if (j < splittedTask.Count - 1)
                    paragraphs.Add((images[j].Pattern, 10, "normal", "Times New Roman"));
            }

            InsertParagrapsAfterParagraphIndex(document, paragraphs, index);
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

            document.RemoveParagraphAt(dynamicTasksParagraphIndex); //Удаляем надпись {{DynamicTasks}}
            dynamicTasksParagraphIndex--;

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

            var tasks = template[Regex.IsMatch(reportName, "пр|Пр") ? "Practices" : "Laboratories"][Regex.Match(reportName, @"\d+").Value].
                DynamicTasks.Select(x => x.Description.Trim()).Select(x => Regex.Replace(x, "•", "\t•")).ToList();

            ;

            int number = 1;
            List<(string, int, string, string)> paragraps = new List<(string, int, string, string)>();

            foreach (int i in _selectedWorksNumbers) // Вставка всех работ по абзацам
            {
                var images = FindImages(tasks[i]);
                var splittedTask = Regex.Split(tasks[i], ImagePattern).Where(x => Regex.IsMatch(x, NamePattern) == false).ToList();

                for (int j = 0; j < splittedTask.Count; j++)
                {
                    if (j == 0)
                        paragraps.Add((_selectedWorksNumbers.Count > 1 ? $"\t{number}) " + splittedTask[j] : "\t" + splittedTask[j], 14, "normal", "Times New Roman"));
                    else
                        paragraps.Add(("\t" + splittedTask[j], 14, "normal", "Times New Roman"));

                    if (j < splittedTask.Count - 1)
                        paragraps.Add((images[j].Pattern, 10, "normal", "Times New Roman"));
                }
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

            foreach (FileInformation fileInformation in _filesInformation)
            {
                try
                {
                    BitmapImage image = new BitmapImage(new Uri(fileInformation.FilePath, UriKind.RelativeOrAbsolute));
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
            for (int i = 0; i < paragraphs.Count; i++)
            {
                var images = FindImages(paragraphs[i]);
                if (images.Count > 0)
                {
                    foreach (var imageInfo in images)
                    {
                        try
                        {
                            string imagePath = imageInfo.Source;
                            if (string.IsNullOrEmpty(imagePath) || File.Exists(imagePath) == false)
                                continue;

                            string imageName = imageInfo.Name;
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

                            document.ReplaceText(imageInfo.Pattern, "");
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
            var paragraphs = document.Paragraphs.Cast<Paragraph>().Select(x => x.Text).ToList();
            for (int i = 0; i < paragraphs.Count; i++)
            {
                if (paragraphs[i].Contains("{{" + parametr + "}}"))
                {
                    return i;
                }
            }
            return -1;
        }

        /// <summary>
        /// Ищет все изображения в тексте
        /// </summary>
        /// <param name="text">Текст для поиска</param>
        /// <returns>Список изображений</returns>
        private List<WorkImage> FindImages(string text)
        {
            var patterns = Regex.Matches(text, ImagePattern).Cast<Match>().Select(x => x.Value);

            return patterns.Select(pattern => new WorkImage()
            {
                Pattern = pattern,
                Name = Regex.Match(Regex.Match(pattern, NamePattern).Value, "\".*\"").Value.Trim('"'),
                Source = Regex.Match(Regex.Match(pattern, SourcePattern).Value, "\".+\"").Value.Trim('"').Replace('\\', '/')
            }).ToList();
        }
    }
}
