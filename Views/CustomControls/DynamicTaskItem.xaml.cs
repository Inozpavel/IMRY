using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace WorkReportCreator.Views.CustomConrols
{
    /// <summary>
    /// Элемент списка информации о заданиях
    /// </summary>
    public partial class DynamicTaskItem : UserControl, INotifyPropertyChanged
    {
        private string _imagePath;

        #region Properties

        /// <summary>
        /// Путь до текущего изображения
        /// </summary>
        public string ImagePath
        {
            get => _imagePath;
            set
            {
                _imagePath = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Является ли текущий элемент выбранным
        /// </summary>
        public bool IsChecked
        {
            get => (bool)GetValue(IsCheckedProperty);
            set
            {
                SetValue(IsCheckedProperty, value);
                ImagePath = IsChecked ? CheckedImage : UncheckedImage;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Путь до изображения выбранного элемента
        /// </summary>
        public string CheckedImage
        {
            get { return (string)GetValue(CheckedImageProperty); }
            set
            {
                SetValue(CheckedImageProperty, value);
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Путь до изображения невыбранного элемента
        /// </summary>
        public string UncheckedImage
        {
            get { return (string)GetValue(UncheckedImageProperty); }
            set
            {
                SetValue(UncheckedImageProperty, value);
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Описание задания
        /// </summary>
        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set
            {
                SetValue(TextProperty, value);
                OnPropertyChanged();
            }
        }
        #endregion

        #region DependencyProperties

        public static readonly DependencyProperty IsCheckedProperty =
            DependencyProperty.Register("IsChecked", typeof(bool), typeof(DynamicTaskItem), new PropertyMetadata(false, IsCheckedPropertyChanged));

        public static readonly DependencyProperty CheckedImageProperty =
            DependencyProperty.Register("CheckedImage", typeof(string), typeof(DynamicTaskItem), new PropertyMetadata("../../Images/Checked.png", CheckedImagePropertyChanged));

        public static readonly DependencyProperty UncheckedImageProperty =
            DependencyProperty.Register("UncheckedImage", typeof(string), typeof(DynamicTaskItem), new PropertyMetadata("../../Images/Transparent.png", UncheckedImagePropertyChanged));

        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(DynamicTaskItem), new PropertyMetadata("", TextPropertyChanged));

        #endregion

        public event Action<object> CheckedChanged;

        public event PropertyChangedEventHandler PropertyChanged;

        public DynamicTaskItem()
        {
            InitializeComponent();
            DataContext = this;
        }

        private static void IsCheckedPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e) => (sender as DynamicTaskItem)
            .IsChecked = (bool)e.NewValue;

        private static void CheckedImagePropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e) => (sender as DynamicTaskItem)
            .CheckedImage = e.NewValue as string;

        private static void UncheckedImagePropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e) => (sender as DynamicTaskItem)
            .UncheckedImage = e.NewValue as string;

        private static void TextPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e) => (sender as DynamicTaskItem)
            .Text = e.NewValue as string;

        public void OnPropertyChanged([CallerMemberName] string propertyName = "") => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        /// <summary>
        /// Инвертирует значение свойства IsChecked
        /// </summary>
        private void InvertIsChecked(object sender, MouseButtonEventArgs e)
        {
            IsChecked = !IsChecked;
            OnPropertyChanged();
            CheckedChanged?.Invoke(this);
        }
    }
}
