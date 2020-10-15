﻿using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace WorkReportCreator.Views
{
    /// <summary>
    /// Логика взаимодействия для InputWorkNumberBox.xaml
    /// </summary>
    public partial class InputWorkNumberBox : Window
    {
        public Command CloseWindow { get; private set; }

        public int? ResultNumber { get; private set; } = null;

        private readonly List<string> _existingWorks;

        public InputWorkNumberBox(List<string> existingWorks)
        {
            InitializeComponent();
            DataContext = this;
            _existingWorks = existingWorks;
            CloseWindow = new Command((sender) => Close(), null);
        }

        private void ValidateInput(object sender, RoutedEventArgs e)
        {
            string text = textBox.Text;

            if (text.Trim().All(x => char.IsDigit(x)) == false)
            {
                MessageBox.Show("Для ввода разрешены только цифры!\nПроверьте данные.", "Недопустимое значение!", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            else if (_existingWorks.Contains(int.Parse(text.Trim()).ToString()))
            {
                MessageBox.Show("Введенное значение уже есть в списке!\nОдинаковых значений быть не должно!", "Недопустимое значение!", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            else if (text.Trim().Length > 3)
            {
                MessageBox.Show("Введенное значение слишком большое!", "Недопустимое значение!", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            ResultNumber = int.Parse(text.Trim());
            Close();
        }
    }
}