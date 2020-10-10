using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace WorkReportCreator.Views.CustomConrols
{
    public partial class DynamicTaskItem : UserControl, INotifyPropertyChanged
    {
        private string _imagePath;

        public string ImagePath
        {
            get => _imagePath;
            set
            {
                _imagePath = value;
                OnPropertyChanged();
            }
        }

        public bool? IsChecked
        {
            get => (bool?)GetValue(IsCheckedProperty);
            set
            {
                SetValue(IsCheckedProperty, value);
                OnPropertyChanged();
            }
        }

        public static readonly DependencyProperty IsCheckedProperty =
            DependencyProperty.Register("IsChecked", typeof(bool?), typeof(DynamicTaskItem), new PropertyMetadata(false, IsCheckedPropertyChanged));

        public string CheckedImage
        {
            get { return (string)GetValue(CheckedImageProperty); }
            set
            {
                SetValue(CheckedImageProperty, value);
                OnPropertyChanged();
            }
        }

        public static readonly DependencyProperty CheckedImageProperty =
            DependencyProperty.Register("CheckedImage", typeof(string), typeof(DynamicTaskItem), new PropertyMetadata("../../Images/Checked.png", CheckedImagePropertyChanged));

        public string UncheckedImage
        {
            get { return (string)GetValue(UncheckedImageProperty); }
            set
            {
                SetValue(UncheckedImageProperty, value);
                OnPropertyChanged();
            }
        }

        public static readonly DependencyProperty UncheckedImageProperty =
            DependencyProperty.Register("UncheckedImage", typeof(string), typeof(DynamicTaskItem), new PropertyMetadata("../../Images/Transparent.png", UncheckedImagePropertyChanged));

        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set
            {
                SetValue(TextProperty, value);
                OnPropertyChanged();
            }
        }

        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(DynamicTaskItem), new PropertyMetadata("", TextPropertyChanged));

        public event PropertyChangedEventHandler PropertyChanged;

        public event Action<object> CheckedChanged;

        public DynamicTaskItem()
        {
            InitializeComponent();
            DataContext = this;
        }

        private static void IsCheckedPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            (sender as DynamicTaskItem).IsChecked = (bool?)e.NewValue;
        }

        private static void CheckedImagePropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            (sender as DynamicTaskItem).CheckedImage = e.NewValue as string;
        }

        private static void UncheckedImagePropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            (sender as DynamicTaskItem).UncheckedImage = e.NewValue as string;
        }

        private static void TextPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            (sender as DynamicTaskItem).Text = e.NewValue as string;
        }

        public void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void InvertIsChecked(object sender, MouseButtonEventArgs e)
        {
            IsChecked = !IsChecked;
            ImagePath = IsChecked ?? false ? CheckedImage : UncheckedImage;
            OnPropertyChanged();
            CheckedChanged?.Invoke(this);
        }
    }
}
