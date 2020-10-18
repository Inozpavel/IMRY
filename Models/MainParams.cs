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
        private bool _workHasTitlePage = false;

        private string _workTitlePageFilePath = "./Configs/TitlePage.docx";
        
        private bool _workHasTitlePageParams = false;

        private string _workTitlePageParamsFilePath = "./Configs/TitlePageParams.json";

        private string _permittedDragAndDropExtentionsFilePath = "./Configs/PermittedDragAndDropExtentions.json";

        private string _currentTemplateFilePath = "./Configs/JavaTasks.json";

        private string _standartUserDataFileName = "StudentInformation";

        private string _allReportsPath = "./Reports/";

        #region Properties

        /// <summary>
        /// Имеется ли титульная страница
        /// </summary>
        public bool WorkHasTitlePage
        {
            get => _workHasTitlePage;
            set
            {
                _workHasTitlePage = value;
                TrySave();
            }
        }
       
        /// <summary>
        /// Путь до страницы с титульником
        /// </summary>
        public string WorkTitlePageFilePath
        {
            get => _workTitlePageFilePath;
            set
            {
                _workTitlePageFilePath = value;
                TrySave();
            }
        }

        /// <summary>
        /// Имеется ли титульная страница
        /// </summary>
        public bool WorkHasTitlePageParams
        {
            get => _workHasTitlePageParams;
            set
            {
                _workHasTitlePageParams = value;
                TrySave();
            }
        }

        /// <summary>
        /// Путь до файла с параметрами для титульной страницы
        /// </summary>
        public string WorkTitlePageParamsFilePath
        {
            get => _workTitlePageParamsFilePath;
            set
            {
                _workTitlePageParamsFilePath = value;
                TrySave();
            }
        }

        /// <summary>
        /// Разрешенные расширения файлов для Drag & Drop
        /// </summary>
        public string PermittedDragAndDropExtentionsFilePath
        {
            get => _permittedDragAndDropExtentionsFilePath;
            set
            {
                _permittedDragAndDropExtentionsFilePath = value;
                TrySave();
            }
        }

        /// <summary>
        /// Путь до текущего шаблона
        /// </summary>
        public string CurrentTemplateFilePath
        {
            get => _currentTemplateFilePath;
            set
            {
                _currentTemplateFilePath = value;
                TrySave();
            }
        }

        /// <summary>
        /// Путь, по которому программа будет пытаться подгрузить информацию о пользователе
        /// </summary>
        public string UserDataFileName
        {
            get => _standartUserDataFileName;
            set
            {
                _standartUserDataFileName = value;
                TrySave();
            }
        }

        /// <summary>
        /// Путь, где все отчеты будут сохранены
        /// </summary>
        public string AllReportsPath
        {
            get => _allReportsPath;
            set
            {
                _allReportsPath = value;
                TrySave();
            }
        }

        #endregion

        public MainParams()
        {
            var parameters = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText("./MainConfig.json"));
            _workHasTitlePage = bool.Parse(parameters["WorkHasTitlePage"]);
            if (_workHasTitlePage)
            {
                _workTitlePageFilePath = parameters["WorkTitlePageFilePath"];
                _workHasTitlePageParams= bool.Parse(parameters["WorkHasTitlePageParams"]);
                if (_workHasTitlePageParams)
                    _workTitlePageParamsFilePath = parameters["WorkTitlePageParamsFilePath"];
            }
            _permittedDragAndDropExtentionsFilePath = parameters["PermittedDragAndDropExtentionsFilePath"];
            _currentTemplateFilePath = parameters["CurrentTemplateFilePath"];
            _standartUserDataFileName = parameters["UserDataFileName"];
            _allReportsPath = parameters["AllReportsPath"];
        }

        public static void ValidateAllParams()
        {
            try
            {
                List<string> requiredParams = new List<string>()
                {
                    "WorkHasTitlePage",
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
