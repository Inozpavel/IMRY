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
        private Visibility _textBoxVisiblity = Visibility.Visible;

        public Visibility TextBoxVisiblity
        {
            get => _textBoxVisiblity;
            set
            {
                _textBoxVisiblity = value;
                OnPropertyChanged();
            }
        }

        private Visibility _fileNameVisiblity = Visibility.Collapsed;

        public Visibility FileNameVisiblity
        {
            get => _fileNameVisiblity;
            set
            {
                _fileNameVisiblity = value;
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
                TextBoxVisiblity = string.IsNullOrEmpty(_fileName) == false && IsSelected ? Visibility.Visible : Visibility.Collapsed;
                FileNameVisiblity = string.IsNullOrEmpty(_fileName) == false ? Visibility.Visible : Visibility.Collapsed;
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
                OnPropertyChanged();
            }
        }

        public bool IsSelected
        {
            get { return (bool)GetValue(IsSelectedProperty); }
            set
            {
                SetValue(IsSelectedProperty, value);
                TextBoxVisiblity = string.IsNullOrEmpty(_fileName) == false && IsSelected ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public ReportMenuItem()
        {
            InitializeComponent();
            DataContext = this;
            TextBoxVisiblity = Visibility.Collapsed;
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
                Filter = "Xml файлы(*.xml)|*.xml|Все файлы (*.*)|*.*",
                DefaultExt = "xml",
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
