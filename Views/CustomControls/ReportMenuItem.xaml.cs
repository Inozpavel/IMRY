using Microsoft.Win32;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;

namespace WorkReportCreator.Views
{
    public partial class ReportMenuItem : UserControl, INotifyPropertyChanged
    {
        private Visibility _textBoxVisibility = Visibility.Visible;

        public Visibility TextBoxVisibility
        {
            get => _textBoxVisibility;
            set
            {
                _textBoxVisibility = value;
                OnPropertyChanged();
            }
        }

        private Visibility _fileNameVisibility = Visibility.Collapsed;

        public Visibility FileNameVisibility
        {
            get => _fileNameVisibility;
            set
            {
                _fileNameVisibility = value;
                OnPropertyChanged();
            }
        }

        private Visibility _hintVisibility;

        public Visibility HintVisibility
        {
            get => _hintVisibility;
            set
            {
                _hintVisibility = value;
                OnPropertyChanged();
            }
        }

        private string _fileName;

        public string FileName
        {
            get => _fileName;
            set
            {
                _fileName = value;
                TextBoxVisibility = string.IsNullOrEmpty(_fileName) == false && IsSelected ? Visibility.Visible : Visibility.Collapsed;
                FileNameVisibility = string.IsNullOrEmpty(_fileName) == false ? Visibility.Visible : Visibility.Collapsed;
                OnPropertyChanged();
            }
        }

        private int _number;

        public int Number
        {
            get => _number;
            set
            {
                _number = value;
                OnPropertyChanged();
            }
        }

        public static readonly DependencyProperty CodeFileDescriptionProperty =
            DependencyProperty.Register("CodeFileDescription", typeof(string), typeof(ReportMenuItem), new PropertyMetadata("", CodeFileDescriptionPropertyChanged));

        public static readonly DependencyProperty CodeFilePathProperty =
            DependencyProperty.Register("CodeFilePath", typeof(string), typeof(ReportMenuItem), new PropertyMetadata("", CodeFilePathPropertyChanged));


        public static readonly DependencyProperty IsSelectedProperty =
           DependencyProperty.Register("IsSelected", typeof(bool), typeof(ReportMenuItem), new PropertyMetadata(false));

        public string CodeFileDescription
        {
            get { return (string)GetValue(CodeFileDescriptionProperty); }
            set
            {
                SetValue(CodeFileDescriptionProperty, value);
                OnPropertyChanged();
            }
        }

        public string CodeFilePath
        {
            get { return (string)GetValue(CodeFilePathProperty); }
            set
            {
                FileName = Regex.Match(value, @"(\w+\.[\w]+)+$").Value;
                SetValue(CodeFilePathProperty, value);
                HintVisibility = string.IsNullOrEmpty(CodeFilePath) == false ? Visibility.Hidden : Visibility.Visible;
                OnPropertyChanged();
            }
        }

        public bool IsSelected
        {
            get { return (bool)GetValue(IsSelectedProperty); }
            set
            {
                SetValue(IsSelectedProperty, value);
                TextBoxVisibility = string.IsNullOrEmpty(_fileName) == false && IsSelected ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public ReportMenuItem()
        {
            InitializeComponent();
            DataContext = this;
            TextBoxVisibility = Visibility.Collapsed;
        }

        private static void CodeFileDescriptionPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            (sender as ReportMenuItem).CodeFileDescription = e.NewValue as string;
        }

        private static void CodeFilePathPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            (sender as ReportMenuItem).CodeFilePath = e.NewValue as string;
        }

        public void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void ChooseFile(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog()
            {
                Title = "Сохранение инфорации о студенте",
                Filter = "Все файлы (*.*)|*.*",
            };

            if (dialog.ShowDialog() == true)
            {
                CodeFilePath = dialog.FileName;
            }
        }
        public void MarkAsSelected() => IsSelected = true;

        public void MarkAsNotSelected() => IsSelected = false;
    }
}
