using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using WorkReportCreator.Models;
using WorkReportCreator.Views;
using WorkReportCreator.Views.CustomConrols;
using Xceed.Document.NET;
using Xceed.Words.NET;

namespace WorkReportCreator
{
    internal class ReportViewModel : INotifyPropertyChanged
    {
        #region Commands

        public Command AddFileInfo { get; private set; }

        public Command RemoveFileInfo { get; private set; }

        public Command SwapUpFileInfo { get; private set; }

        public Command SwapDownFileInfo { get; private set; }

        #endregion

        private int? _selectedItemIndex;

        private Visibility _hintVisibility;

        private Visibility _dynamicTasksVisiblity;

        private string _dynamicTasksStatus;

        private ListBoxItem _selectedItem;

        #region Properties

        /// <summary>
        /// Индекс текущего выбранного элемента
        /// </summary>
        public int? SelectedItemIndex
        {
            get => _selectedItemIndex;
            set
            {
                _selectedItemIndex = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Видимость подсказки для перетаскивания элементов в область
        /// </summary>
        public Visibility HintVisibility
        {
            get => _hintVisibility;
            set
            {
                _hintVisibility = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Видимость блока с заданиями
        /// </summary>
        public Visibility DynamicTasksVisiblity
        {
            get => _dynamicTasksVisiblity;
            set
            {
                _dynamicTasksVisiblity = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Надпись, показывающая, выбрано ли задание или нет
        /// </summary>
        public string DynamicTasksStatus
        {
            get => _dynamicTasksStatus;
            set
            {
                _dynamicTasksStatus = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Текущий выбранный элемент
        /// </summary>
        public ListBoxItem SelectedItem
        {
            get => _selectedItem;
            set
            {
                if (_selectedItem != null)
                    (_selectedItem.Content as FileInformationItem).IsSelected = false;
                _selectedItem = value;
                if (_selectedItem != null)
                    (_selectedItem.Content as FileInformationItem).IsSelected = true;
            }
        }
        /// <summary>
        /// Список заданий
        /// </summary>
        public ObservableCollection<DynamicTaskItem> DynamicTasksArray { get; set; } = new ObservableCollection<DynamicTaskItem>();

        /// <summary>
        /// Список информации о файлах
        /// </summary>
        public ObservableCollection<ListBoxItem> FilesArray { get; set; } = new ObservableCollection<ListBoxItem>();

        #endregion

        public event PropertyChangedEventHandler PropertyChanged;

        /// <param name="DynamicTasks">Список заданий (при наличии)</param>
        public ReportViewModel(List<string> DynamicTasks)
        {
            FilesArray.CollectionChanged += (sender, e) => HintVisibility = FilesArray.Count != 0 ? Visibility.Hidden : Visibility.Visible;

            AddFileInfo = new Command(AddNewFileInfo, null);
            RemoveFileInfo = new Command(RemoveSelectedFileInfo, RemoveSelectedFileInfoCanExecute);
            SwapUpFileInfo = new Command(SwapUpSelectedFileInfo, SwapUpSelectedFileInfoCanExecute);
            SwapDownFileInfo = new Command(SwapDownSelectedFileInfo, SwapDownSelectedFileInfoCanExecute);

            foreach (string task in DynamicTasks ?? new List<string>())
                DynamicTasksArray.Add(new DynamicTaskItem() { Text = task, });

            DynamicTasksVisiblity = DynamicTasks.Count > 0 ? DynamicTasksVisiblity = Visibility.Visible : DynamicTasksVisiblity = Visibility.Collapsed;
            DynamicTasksStatus = DynamicTasks.Count == 0 ? "Заданий для выбора нет" : "Выберите, пожалуйста, задание";

            void UpdateTasksStatus(object sender) => DynamicTasksStatus = DynamicTasksArray
                .Any(x => x.IsChecked) ? "Задание выбрано" : "Выберите, пожалуйста, задание";

            foreach (var i in DynamicTasksArray)
                i.CheckedChanged += UpdateTasksStatus;
        }

        /// <summary>
        /// Добавляет пустой элемент в список файлов
        /// </summary>
        public void AddNewFileInfo(object sender)
        {
            if (SelectedItemIndex != null)
            {
                FilesArray.Insert(SelectedItemIndex + 1 ?? 0, new ListBoxItem()
                {
                    Content = new FileInformationItem() { Number = FilesArray.Count + 1 },
                    HorizontalContentAlignment = HorizontalAlignment.Stretch
                });
                SelectedItemIndex += 1;
                UpdateAllNumbers();
            }
            else
            {
                FilesArray.Add(new ListBoxItem()
                {
                    Content = new FileInformationItem() { Number = FilesArray.Count + 1 },
                    HorizontalContentAlignment = HorizontalAlignment.Stretch
                });
                SelectedItemIndex = FilesArray.Count - 1;
            }
        }

        /// <summary>
        /// Добавляет новый элемент в список элементов с описанием файлов, с указанным файлом
        /// </summary>
        /// <param name="filePath">Путь до файла</param>
        public void AddNewFileInfoWithFilePath(string filePath)
        {
            if (File.Exists(filePath))
            {
                FilesArray.Add(new ListBoxItem()
                {
                    Content = new FileInformationItem() { Number = FilesArray.Count + 1, FilePath = filePath },
                    HorizontalContentAlignment = HorizontalAlignment.Stretch
                });
            }
        }

        /// <summary>
        /// Удаляет выбранный элемент из списка файлов
        /// </summary>
        public void RemoveSelectedFileInfo(object sender)
        {
            FileInformationItem reportMenuItem = _selectedItem.Content as FileInformationItem;

            if (((Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift)) == false) &&
                (string.IsNullOrEmpty(reportMenuItem.FileName) == false || string.IsNullOrEmpty(reportMenuItem.FileDescription) == false))
            {
                if (MessageBox.Show("В выбранном элементе имеются введеные данные!\nОни удалятся БЕЗ возможности восстановления!\nВы уверены?",
                    "Подтвердите действие", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No) == MessageBoxResult.No)
                    return;
            }

            int number = reportMenuItem.Number - 1;

            FilesArray.Remove(SelectedItem);
            UpdateAllNumbers();

            if (number > 0)
                SelectedItemIndex = number - 1;
        }

        /// <summary>
        /// Проверяет, можно ли удалить выбранный элемент
        /// </summary>
        /// <returns><paramref name="True"/> если может удалить, в противное случает <paramref name="false"/></returns>
        public bool RemoveSelectedFileInfoCanExecute(object fileInfo) => SelectedItem != null;

        /// <summary>
        /// Перемещает выбранный элемент выше
        /// </summary>
        public void SwapUpSelectedFileInfo(object sender) => SwapAdjacentItemWithSelected(-1);

        /// <summary>
        /// Проверяет, можно ли переместить выше выбраннный элемент
        /// </summary>
        /// <returns><paramref name="True"/> если может переместить, в противное случает <paramref name="false"/></returns>
        public bool SwapUpSelectedFileInfoCanExecute(object sender) => _selectedItem != null && _selectedItemIndex != 0;

        /// <summary>
        /// Перемещает выбранный элемент ниже
        /// </summary>
        public void SwapDownSelectedFileInfo(object sender) => SwapAdjacentItemWithSelected(+1);


        /// <summary>
        /// Обменивает ближайший элемент с выбранным
        /// </summary>
        /// <param name="i">1 - элемент снизу, -1 - элемент снизу</param>
        private void SwapAdjacentItemWithSelected(int i)
        {
            int number = (_selectedItem.Content as FileInformationItem).Number - 1;
            SwapArrayItems(number, number + i);
            SelectedItemIndex = number + i;
        }

        /// <summary>
        /// Проверяет, можно ли переместить ниже выбранный элемент
        /// </summary>
        /// <returns><paramref name="True"/> если может переместить, в противное случает <paramref name="false"/></returns>
        public bool SwapDownSelectedFileInfoCanExecute(object sender) => _selectedItem != null && _selectedItemIndex + 1 != FilesArray.Count;

        /// <summary>
        /// Обменивает два элемента в списке файлов с указанными индексами
        /// </summary>
        /// <param name="firstIndex">Индекс первого элемента</param>
        /// <param name="secondIndex">Индекс второго элемента</param>
        private void SwapArrayItems(int firstIndex, int secondIndex)
        {
            FileInformationItem temp = FilesArray[firstIndex].Content as FileInformationItem;
            FilesArray[firstIndex].Content = FilesArray[secondIndex].Content;
            FilesArray[secondIndex].Content = temp;

            UpdateAllNumbers();
        }

        /// <summary>
        /// Для каждого элемента в списке файлов перепросчитывет индекс
        /// </summary>
        private void UpdateAllNumbers()
        {
            for (int i = 0; i < FilesArray.Count; i++)
                (FilesArray[i].Content as FileInformationItem).Number = i + 1;
        }

        #region ReportGeneration

        /// <summary>
        /// Cоздает отчет для работы
        /// </summary>
        /// <exception cref="Exception"/>
        public void GenerateReport(string reportName)
        {
            try
            {
                MainParams mainParams = new MainParams();

                DocX document;
                if (mainParams.WorkHasTitlePage)
                    document = GenerateTitlePage();
                else
                    document = DocX.Create("./Configs/EmptyDocument.docs");

                document = AddWorkInformation(document, reportName);
                document = AddSelectedTasks(document, reportName);
                document = AddUserFiles(document);

                document = InsertAllImages(document);


                if (Directory.Exists(mainParams.AllReportsPath) == false)
                    Directory.CreateDirectory(mainParams.AllReportsPath);
                document.SaveAs($"{mainParams.AllReportsPath}/Отчет {reportName}.docx");
            }
            catch (IOException e)
            {
                throw new Exception(e.Message);
            }
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

            StudentInformation student;
            try
            {
                student = JsonConvert.DeserializeObject<StudentInformation>(File.ReadAllText(mainParams.UserDataFilePath));
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
        private DocX AddWorkInformation(DocX document, string reportName)
        {
            MainParams mainParams = new MainParams();
            try
            {
                var template = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, ReportInformation>>>(File.ReadAllText(mainParams.CurrentTemplateFilePath));
                ReportInformation task = template[Regex.IsMatch(reportName, "пр|Пр") ? "Practices" : "Laboratories"][Regex.Match(reportName, @"\d+").Value];
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
            return document;
        }

        /// <summary>
        /// Вставляет в отчет выбранные пользователем работы вместо {{DynamicTasks}}
        /// <param name="document">Документ</param>
        /// <param name="reportName">Название отчета</param>
        /// <returns>Документ со вставленной информацией о работе</returns>
        /// <exception cref="Exception"/>
        private DocX AddSelectedTasks(DocX document, string reportName)
        {
            int dynamicTasksParagraphIndex = FindParagraphIndexWithParametr(document, "DynamicTasks"); //номер абраза, с котоого надо начинать вставлять задания

            if (dynamicTasksParagraphIndex == -1) //Задания вставлять не нужно
                return document;

            List<int> selectedWorksNumbers = new List<int>();
            for (int i = 0; i < DynamicTasksArray.Count; i++)
            {
                if (DynamicTasksArray[i].IsChecked)
                    selectedWorksNumbers.Add(i);
            }

            if (selectedWorksNumbers.Count < 0) // Заданий нет
                return document;

            MainParams mainParams = new MainParams();
            Dictionary<string, Dictionary<string, ReportInformation>> template;
            try
            {
                template = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, ReportInformation>>>(File.ReadAllText(mainParams.CurrentTemplateFilePath));
            }
            catch (Exception)
            {
                MessageBox.Show("Не удалось вставить в документ выбранные задания!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return document;
            }

            var tasks = template[Regex.IsMatch(reportName, "пр|Пр") ? "Practices" : "Laboratories"][Regex.Match(reportName, @"\d+").Value].DynamicTasks.Select(x => x.Description.Trim()).ToList();
            tasks = tasks.Select(x => Regex.Replace(x, "•", "\t•")).ToList();

            document.RemoveParagraphAt(dynamicTasksParagraphIndex); //Удаляем надпись {{DynamicTasks}}
            dynamicTasksParagraphIndex--;

            int number = 1;
            List<(string, int, string)> paragraps = new List<(string, int, string)>();

            foreach (int i in selectedWorksNumbers) // Вставка всех работ по абзацам
            {
                paragraps.Add((selectedWorksNumbers.Count > 1 ? $"\t{number}) " + tasks[i] : "\t" + tasks[i], 14, "normal"));
                number++;
            }

            InsertParagrapsAfterParagraphIndex(document, paragraps, dynamicTasksParagraphIndex, "Times New Roman");

            return document;
        }

        /// <summary>
        /// Вставляет в документ указанные абзацы после указанного
        /// </summary>
        /// <param name="document">Документ, в который будет вставка</param>
        /// <param name="paragraphs">Абзацы</param>
        /// <param name="paragraphIndex">Индекс параграфа</param>
        /// <param name="FontFamily">Шрифт</param>
        private void InsertParagrapsAfterParagraphIndex(DocX document, List<(string text, int fontSize, string style)> paragraphs, int paragraphIndex, string FontFamily)
        {
            for (int i = 0; i < paragraphs.Count; i++)
            {
                Paragraph paragraph = document.InsertParagraph(paragraphs[i].text).FontSize(paragraphs[i].fontSize).Font(FontFamily);

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
        private DocX AddUserFiles(DocX document)
        {
            int userFilesParagraphIndex = FindParagraphIndexWithParametr(document, "UserFiles");
            if (userFilesParagraphIndex == -1)
                return document;

            document.RemoveParagraphAt(userFilesParagraphIndex); //Удаляем надпись {{UserFiles}}
            userFilesParagraphIndex--;

            var selectedFiles = FilesArray.Select(x => x.Content as FileInformationItem).ToList();
            if (selectedFiles.Count > 0 == false)
                return document;

            List<(string text, int fontSize, string style)> paragraphs = new List<(string, int, string)>();


            foreach (FileInformationItem fileInformation in selectedFiles)
            {
                try
                {
                    BitmapImage image = new BitmapImage(new Uri(fileInformation.FilePath));
                    string name = string.IsNullOrEmpty(fileInformation.FileDescription) ? "" : ",name=\"" + fileInformation.FileDescription + "\"";
                    paragraphs.Add(("{{image source=\"" + fileInformation.FilePath + "\"" + name + "}}", 10, "normal"));
                }
                catch (Exception)
                {
                    paragraphs.Add((fileInformation.FileName, 16, "bold"));

                    if (string.IsNullOrEmpty(fileInformation.FileDescription) == false)
                        document.InsertParagraph(fileInformation.FileDescription).Font("Times New Roman").FontSize(14);
                    paragraphs.Add((string.Join("\n", File.ReadAllLines(fileInformation.FilePath)), 10, "normal"));
                }
                paragraphs.Add(("", 10, "normal"));
            }
            InsertParagrapsAfterParagraphIndex(document, paragraphs, userFilesParagraphIndex, "Consolas");
            return document;
        }


        /// <summary>
        /// Заменяет все {{image}} в которых указан путь на картинки
        /// </summary>
        /// <param name="doc">Документ</param>
        /// <returns>Документ со вставленными картинками</returns>
        private DocX InsertAllImages(DocX document)
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
            return document;
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

        #endregion

        public void OnPropertyChanged([CallerMemberName] string propertyName = "") => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
