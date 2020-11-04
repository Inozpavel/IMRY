using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media.Imaging;

namespace WorkReportCreator.Views
{
    public partial class FieldInputWindow : Window
    {
        public Command SubmitInput { get; private set; }

        public Command FormatText { get; private set; }

        public Command HideWindow { get; private set; }

        public Command AddImage { get; private set; }

        public string ResultText { get; private set; }

        /// <param name="caption">Надпись в верхней части окна</param>
        /// <param name="text">Текст, который нужно редактировать</param>
        public FieldInputWindow(string caption, string text)
        {
            InitializeComponent();
            DataContext = this;
            ResultText = text;

            SubmitInput = new Command(Submit, null);
            HideWindow = new Command((sender) => Hide(), null);
            FormatText = new Command(FormatInputText, null);
            AddImage = new Command(ShowDialogAddImage, null);

            labelCaption.Content = caption;
            textBoxInput.Text = text;
            textBoxInput.Focus();
            textBoxInput.CaretIndex = text.Length;
        }

        /// <summary>
        /// Сохраняет текст, закрывает окно
        /// </summary>
        private void Submit(object sender)
        {
            ResultText = textBoxInput.Text;
            Hide();
        }

        /// <summary>
        /// Форматирует текст
        /// </summary>
        private void FormatInputText(object sender)
        {
            string text = textBoxInput.Text.Trim();
            if (text.Length == 0)
                return;
            string labelCaptionText = labelCaption.Content as string;
            if (labelCaptionText.Contains("название работы") == false & ".!?".Contains(text.Last()) == false)
                text += ".";
            else if (labelCaptionText.Contains("название работы"))
            {
                while (text.Length > 0 && ".!?".Contains(text[text.Length - 1]))
                    text = text.Substring(0, text.Length - 1);

                text = text.ToUpper();
            }
            textBoxInput.Text = text;
        }

        /// <summary>
        /// Показывает диалог для выбора картинки и вставки ссылки на нее в текст
        /// </summary>
        private void ShowDialogAddImage(object sender)
        {
            OpenFileDialog fileDialog = new OpenFileDialog()
            {
                Title = "Выберите файл с изображением. Ссылка не него будет скопирована в буфер обмена.",
                Filter = "Изображения (*.jpg, *.png, *.bmp, *.jpeg)|*.jpg; *.png; *.bmp; *.jpeg",
            };
            if (fileDialog.ShowDialog() == true)
            {
                try
                {
                    Uri uri = new Uri(fileDialog.FileName);
                    BitmapImage image = new BitmapImage(uri);
                    string relativePath = new Uri(Directory.GetCurrentDirectory()).MakeRelativeUri(uri).ToString();
                    List<string> splittedPath = relativePath.Split('/').Skip(1).ToList();
                    splittedPath.Insert(0, ".");
                    Clipboard.SetText("{{image source=\"" + string.Join("/", splittedPath) + "\", name=\"\"}}");
                }
                catch (Exception)
                {
                    MessageBox.Show("Не получилось обработать картинку!");
                }
            }
        }
    }
}
