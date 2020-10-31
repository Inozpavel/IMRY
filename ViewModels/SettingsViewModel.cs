using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Forms;
using WorkReportCreator.Models;

namespace WorkReportCreator.ViewModels
{
    internal class SettingsViewModel
    {
        public Command ChooseTitlePageFile { get; private set; }

        public Command ChooseTitlePageParamsFile { get; private set; }

        public Command ChooseCurrentTemplateFile { get; private set; }

        public Command ChoosePermittedExtentionsFile { get; private set; }

        public Command ChooseFolderForReports { get; private set; }

        public MainParams Params { get; set; }

        private OpenFileDialog _dialog;

        public SettingsViewModel()
        {
            Params = new MainParams();
            ChooseTitlePageFile = new Command((x) => SelectTitlePageFile(), null);
            ChooseTitlePageParamsFile = new Command((x) => SelectTitlePageParamsFile(), null);
            ChooseCurrentTemplateFile = new Command((x) => SelectCurrentTemplateFile(), null);
            ChoosePermittedExtentionsFile = new Command((x) => SelectPermittedExtentionFile(), null);
            ChooseFolderForReports = new Command((x) => SelectFolderForReports(), null);
        }

        /// <summary>
        /// Показывает диалог для выбора раположения титульной страницы, проверяет корректность данных, обноляет свойство WorkTitlePageFilePath
        /// </summary>
        private void SelectTitlePageFile()
        {
            OpenFileDialog dialog = BuildDialog("Выберите файл с титульной страницей", "Doc и Docx файлы(*.doc ,*.docx)|*.doc; *.docx");
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                Params.WorkTitlePageFilePath = dialog.FileName;
            }
        }

        /// <summary>
        /// Показывает диалог для выбора расположения параметров для титульной страницы, проверяет корректность данных, обноляет свойство WorkTitlePageParamsFilePath
        /// </summary>
        private void SelectTitlePageParamsFile()
        {
            if (ShowDialogAndCheckPathIsCorrect<Dictionary<string, string>>("Выберите файл с шаблоном работ", "Json файлы(*.json)|*.json"))
                Params.WorkTitlePageParamsFilePath = _dialog.FileName;
        }

        /// <summary>
        /// Показывает диалог для выбора расположения шаблона, проверяет корректность данных, обноляет свойство CurrentTemplateFilePath
        /// </summary>
        private void SelectCurrentTemplateFile()
        {
            if (ShowDialogAndCheckPathIsCorrect<Dictionary<string, Dictionary<string, ReportInformation>>>("Выберите файл с шаблоном работ", "Json файлы(*.json)|*.json"))
                Params.CurrentTemplateFilePath = _dialog.FileName;
        }

        /// <summary>
        /// Показывает диалог для выбора расположения файла с разрешенными расширенями для Drag & Drop, проверяет корректность данных, обноляет свойство PermittedDragAndDropExtentionsFilePath
        /// </summary>
        private void SelectPermittedExtentionFile()
        {
            if (ShowDialogAndCheckPathIsCorrect<List<string>>("Выберите файл с разрешенными расширениями файлов для Drag & Drop", "Json файлы(*.json)|*.json"))
                Params.PermittedDragAndDropExtentionsFilePath = _dialog.FileName;
        }

        /// <summary>
        /// Показывает диалог для выбора расположения, где будут сохраняться отчеты, обноляет свойство AllReportsPath
        /// </summary>>
        private void SelectFolderForReports()
        {
            FolderBrowserDialog dialog = new FolderBrowserDialog()
            {
                Description = "Выберите папку, где будут сохраняться отчеты"
            };
            if (dialog.ShowDialog() == DialogResult.OK)
                Params.AllReportsPath = dialog.SelectedPath;
        }

        /// <summary>
        /// Показывает диалог для выбора файла в формате json с указанным заголовком, проверяет начилие типа T в файле
        /// </summary>
        /// <typeparam name="T">Данные, которые хранятся в файле</typeparam>
        /// <param name="title">Заголовок диалога</param>
        /// <param name="filter">Фильтр файлов</param>
        /// <returns><paramref name="True"/> если данные корректны, в противном случае <paramref name="false"/></returns>
        private bool ShowDialogAndCheckPathIsCorrect<T>(string title, string filter)
        {
            _dialog = BuildDialog(title, filter);
            if (_dialog.ShowDialog() == DialogResult.OK)
            {
                if (CheckJsonFileHasFormat<T>(File.ReadAllText(_dialog.FileName)))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Проверяет, содержится ли указанный тип в тексте формата json
        /// </summary>
        /// <typeparam name="T">Тип данных, который нужно проверить</typeparam>
        /// <param name="jsonText">Текст в формате json</param>
        /// <returns><paramref name="True"/> если содержится, в противном случае <paramref name="false"/></returns>
        private bool CheckJsonFileHasFormat<T>(string jsonText)
        {
            try
            {
                JsonConvert.DeserializeObject<T>(jsonText);
                return true;
            }
            catch (Exception)
            {
                System.Windows.MessageBox.Show("Файл имеет неверный формат!", "Ошибка!", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        /// <summary>
        /// Создает и возвращает диалог для открытия файла с указанным заколовком и фильтром
        /// </summary>
        /// <param name="title">Заголовок диалогового окна</param>
        /// <param name="filter">Фильтр файлов</param>
        /// <returns></returns>
        private OpenFileDialog BuildDialog(string title, string filter)
        {
            OpenFileDialog dialog = new OpenFileDialog()
            {
                Title = title,
                Filter = filter
            };
            return dialog;
        }
    }
}
