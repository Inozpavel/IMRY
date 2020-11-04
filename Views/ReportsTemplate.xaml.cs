using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using WorkReportCreator.Models;
using WorkReportCreator.ViewModels;
using WorkReportCreator.Views;

namespace WorkReportCreator
{
    /// <summary>
    /// Окно с редактированием информации в шаблоне
    /// </summary>
    public partial class ReportsTemplate : Window
    {
        private readonly ReportsTemplateWindowViewModel _model;

        private readonly MainWindow _mainWindow;

        /// <param name="mainWindow">Главное окно</param>
        public ReportsTemplate(MainWindow mainWindow)
        {
            InitializeComponent();
            DataContext = _model = new ReportsTemplateWindowViewModel();
            _mainWindow = mainWindow;
        }

        /// <param name="mainWindow">Главное окно</param>
        /// <param name="template">Шаблон для работ</param>
        /// <param name="filePath">Путь до файла с шаблоном для работ</param>
        public ReportsTemplate(MainWindow mainWindow, Dictionary<string, Dictionary<string, ReportInformation>> template, string filePath)
        {
            InitializeComponent();
            DataContext = _model = new ReportsTemplateWindowViewModel(template, filePath);
            _mainWindow = mainWindow;
        }

        private void CloseApplicationClicked(object sender, System.ComponentModel.CancelEventArgs e)
        {
            bool shouldClose = true;
            if (string.IsNullOrEmpty(_model.FilePath) && (_model.LaboratoriesWorksButtons.Count > 0 || _model.PractisesWorksButtons.Count > 0))
            {
                shouldClose = MessageBox.Show("Данные не сохранены!\nПри выходе они будут ПОТЕРЯНЫ!\nВы уверены?", "Внимание!",
                           MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No) == MessageBoxResult.Yes;
            }

            if (shouldClose)
                _mainWindow.Show();
            else
            {
                e.Cancel = true;
                return;
            }
        }

        /// <summary>
        /// Показывает окно для удобного редактирования текста
        /// </summary>
        /// <param name="caption">Название в вернней части окна</param>
        /// <param name="sender">Кнопка, по которой нажали</param>
        private void ShowEditWindow(string caption, object sender)
        {
            Button button = sender as Button;
            FieldInputWindow window = new FieldInputWindow(caption, button.Content?.ToString() ?? "");
            window.ShowDialog();
            button.Content = window.ResultText;
            window.Close();
        }

        /// <summary>
        /// Проверяет состояние клавиши Shift, если оно совпадает с ожидаемым, форматирует текст на кнопке
        /// </summary>
        /// <param name="shouldShiftBePressed">Ожидаемое состояние шифта</param>
        /// <param name="button">Копка, на которой форматируется текст</param>
        /// <param name="isName">Нужно ли использовать правила форматирования для как для имени работы</param>
        private void CheckShiftStateAndFormatText(bool shouldShiftBePressed, Button button, bool isName = false)
        {
            if (Keyboard.IsKeyDown(Key.LeftShift) == shouldShiftBePressed)
                return;

            if (string.IsNullOrEmpty(button.Content as string))
                return;

            button.Content = _model.FormatText(button.Content as string, isNameOfWork: isName);
        }

        /// <summary>
        /// Вставляет форматированный текст из буфера обмена в кнопку
        /// </summary>
        /// <param name="button">Кнопка, в которую будет вставлен текст</param>
        /// <param name="mouseButton">Кнопка, которую нажали</param>
        /// <param name="isName">Нужно ли использовать правила форматирования для как для имени работы</param>
        private void InsertFromBufferAndFormat(Button button, MouseButton mouseButton, bool isName)
        {
            if (mouseButton == MouseButton.Middle)
            {
                string text = button.Content as string;
                if (string.IsNullOrEmpty(text))
                    button.Content = Clipboard.GetText();

                CheckShiftStateAndFormatText(true, button);
            }
        }

        private void FormatText(object sender, MouseButtonEventArgs e) => CheckShiftStateAndFormatText(shouldShiftBePressed: false, sender as Button);

        private void InsertNotNameFromBufferAndFormat(object sender, MouseButtonEventArgs e) => InsertFromBufferAndFormat(sender as Button, e.ChangedButton, false);

        private void InsertNameFromBufferAndFormat(object sender, MouseButtonEventArgs e) => InsertFromBufferAndFormat(sender as Button, e.ChangedButton, true);

        private void FormatNameText(object sender, MouseButtonEventArgs e) => CheckShiftStateAndFormatText(shouldShiftBePressed: false, sender as Button, true);

        private void ShowEditWindowForWorkName(object sender, RoutedEventArgs e) => ShowEditWindow("Введите название работы", sender);

        private void ShowEditWindowForWorkTarget(object sender, RoutedEventArgs e) => ShowEditWindow("Введите цель работы", sender);

        private void ShowEditWindowForWorkTheoryPart(object sender, RoutedEventArgs e) => ShowEditWindow("Введите теоретическую часть работы", sender);

        private void ShowEditWindowForWorkCommonTask(object sender, RoutedEventArgs e) => ShowEditWindow("Введите общее задание работы", sender);

        private void ShowEditWindowForDynamicTask(object sender, RoutedEventArgs e) => ShowEditWindow("Введите описание индивидуального задания", sender);
    }
}
