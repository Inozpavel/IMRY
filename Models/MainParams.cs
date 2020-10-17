using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;

namespace WorkReportCreator.Models
{
    internal class MainParams
    {
        private string _titlePageFilePath = "./ TitlePage.docx";

        private string _titlePageParametersFilePath = "./Configs/TitlePageParams.json";

        private string _permittedDragAndDropExtentionsFilePath = "./Configs/PermittedDragAndDropExtentions.json";

        private string _currentTemplateFilePath = "./Configs/JavaTasks.json";

        private string _standartUserDataFileName = "StudentInformation";

        private string _allReportsPath = "./Reports/";

        public string TitlePageFilePath
        {
            get => _titlePageFilePath;
            set
            {
                _titlePageFilePath = value;
                TrySave();
            }
        }

        public string TitlePageParametersFilePath
        {
            get => _titlePageParametersFilePath;
            set
            {
                _titlePageParametersFilePath = value;
                TrySave();
            }
        }

        public string PermittedDragAndDropExtentionsFilePath
        {
            get => _permittedDragAndDropExtentionsFilePath;
            set
            {
                _permittedDragAndDropExtentionsFilePath = value;
                TrySave();
            }
        }

        public string CurrentTemplateFilePath
        {
            get => _currentTemplateFilePath;
            set
            {
                _currentTemplateFilePath = value;
                TrySave();
            }
        }

        public string UserDataFileName
        {
            get => _standartUserDataFileName;
            set
            {
                _standartUserDataFileName = value;
                TrySave();
            }
        }

        public string AllReportsPath
        {
            get => _allReportsPath;
            set
            {
                _allReportsPath = value;
                TrySave();
            }
        }

        public MainParams()
        {
            var parameters = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText("./MainConfig.json"));
            _titlePageFilePath = parameters["TitlePageFilePath"];

            _titlePageParametersFilePath = parameters["TitlePageParametersFilePath"];

            _permittedDragAndDropExtentionsFilePath = parameters["PermittedDragAndDropExtentionsFilePath"];

            _currentTemplateFilePath = parameters["CurrentTemplateFilePath"];

            _standartUserDataFileName = parameters["UserDataFileName"];

            _allReportsPath = parameters["AllReportsPath"];
        }

        public void ValidateAllParams()
        {
            try
            {
                List<string> requiredParams = new List<string>()
                {
                    "TitlePageFilePath",
                    "TitlePageParametersFilePath",
                    "PermittedDragAndDropExtentionsFilePath",
                    "CurrentTemplateFilePath",
                    "AllReportsPath"
                };
                var globalParams = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText("./MainConfig.json"));
                if (requiredParams.All(param => globalParams.Keys.Contains(param)) == false)
                {
                    MessageBox.Show("В главном конфигурационном файле отсутствует обязательный параметр!\nБез него нельзя использовать приложение!",
                    "Невозможно запустить приложение!", MessageBoxButton.OK, MessageBoxImage.Error);
                    Application.Current.Shutdown();
                    return;
                }

                foreach (var param in globalParams.Keys.Where(x => x.Contains("FilePath")))
                {
                    if (File.Exists(globalParams[param]) == false)
                    {
                        MessageBox.Show($"Ошибка в параметре {param},\nфайла {globalParams[param]} не существует!\nОн необходим для работы приложения!\nПроверьть его корректность",
                            "Ошибка!", MessageBoxButton.OK, MessageBoxImage.Error);
                        Application.Current.Shutdown();
                        return;
                    }
                }
            }
            catch (Exception)
            {
                MessageBox.Show("Не получилось проверить главый конфигурационный файл!\nБез него нельзя использовать приложение!",
                    "Невозможно запустить приложение!", MessageBoxButton.OK, MessageBoxImage.Error);
                Application.Current.Shutdown();
                return;
            }
        }

        private bool TrySave()
        {
            try
            {
                File.WriteAllText("./MainConfig.json", JsonConvert.SerializeObject(this, Formatting.Indented));
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
