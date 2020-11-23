using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;

namespace WorkReportCreator.Models
{
    public class MainParams : INotifyPropertyChanged
    {
        private string _shortSubjectName = "";

        private bool _workHasTitlePage = false;

        private string _workTitlePageFilePath = "";

        private bool _workHasTitlePageParams = false;

        private string _workTitlePageParamsFilePath = "";

        private string _permittedDragAndDropExtentionsFilePath = "";

        private string _currentTemplateFilePath = "";

        private string _userDataFilePath = "";

        private string _reportsPath = "./Reports/";

        private string _savedReportsPath = "./SavedReports/";

        public event PropertyChangedEventHandler PropertyChanged;

        #region Properties

        /// <summary>
        /// Имеется ли титульная страница
        /// </summary>
        public string ShortSubjectName
        {
            get => _shortSubjectName;
            set
            {
                _shortSubjectName = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Имеется ли титульная страница
        /// </summary>
        public bool WorkHasTitlePage
        {
            get => _workHasTitlePage;
            set
            {
                _workHasTitlePage = value;
                OnPropertyChanged();
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
                OnPropertyChanged();
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
                OnPropertyChanged();
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
                OnPropertyChanged();
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
                OnPropertyChanged();
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
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Путь, по которому программа будет пытаться подгрузить информацию о пользователе
        /// </summary>
        public string UserDataFilePath
        {
            get => _userDataFilePath;
            set
            {
                _userDataFilePath = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Путь, где все отчеты будут сохранены
        /// </summary>
        public string ReportsPath
        {
            get => _reportsPath;
            set
            {
                _reportsPath = value;
                OnPropertyChanged();
            }
        }

        public string SavedReportsPath
        {
            get => _savedReportsPath;
            set
            {
                _savedReportsPath = value;
                OnPropertyChanged();
            }
        }

        #endregion

        public MainParams()
        {
            var parameters = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText("./MainConfig.json"));
            _workHasTitlePage = bool.Parse(parameters["WorkHasTitlePage"]);
            _workTitlePageFilePath = parameters["WorkTitlePageFilePath"];
            _workHasTitlePageParams = bool.Parse(parameters["WorkHasTitlePageParams"]);
            _workTitlePageParamsFilePath = parameters["WorkTitlePageParamsFilePath"];
            _permittedDragAndDropExtentionsFilePath = parameters["PermittedDragAndDropExtentionsFilePath"];
            _currentTemplateFilePath = parameters["CurrentTemplateFilePath"];
            if (parameters.Keys.Contains("UserDataFilePath"))
                _userDataFilePath = parameters["UserDataFilePath"];
            _reportsPath = parameters["ReportsPath"];
            _savedReportsPath = parameters["SavedReportsPath"];
            _shortSubjectName = parameters["ShortSubjectName"];
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
                    "ReportsPath",
                    "SavedReportsPath",
                    "ShortSubjectName",
                };

                var globalParams = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText("./MainConfig.json"));

                if (requiredParams.All(param => globalParams.Keys.Contains(param)) == false)
                {
                    MessageBox.Show("В главном конфигурационном файле отсутствует обязательный параметр!\nБез него нельзя использовать приложение!",
                    "Невозможно запустить приложение!", MessageBoxButton.OK, MessageBoxImage.Error);
                    Application.Current.Shutdown();
                    return;
                }

                foreach (var param in globalParams.Keys.Where(x => x.Contains("FilePath") && x != "UserDataFilePath"))
                {
                    if (File.Exists(globalParams[param]) == false)
                    {
                        MessageBox.Show($"Ошибка в параметре {param},\nфайл {globalParams[param]} не существует!\nОн необходим для работы приложения!\nПроверьть его корректность",
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

        private void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            TrySave();
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
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
