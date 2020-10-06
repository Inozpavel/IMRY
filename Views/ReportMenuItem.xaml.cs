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

        private Visibility _fileNameVisiblity = Visibility.Collapsed;

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

        public Visibility TextBoxVisiblity
        {
            get => _textBoxVisiblity;
            set
            {
                _textBoxVisiblity = value;
                OnPropertyChanged();
            }
        }
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
                TextBoxVisiblity = _fileName != "" && IsSelected ? Visibility.Visible : Visibility.Collapsed;
                FileNameVisiblity = _fileName != "" ? Visibility.Visible : Visibility.Collapsed;
                OnPropertyChanged();
            }
        }

        public string CodeFileDescription
        {
            get { return (string)GetValue(CodeFileDescriptionProperty); }
            set
            {
                SetValue(CodeFileDescriptionProperty, value);
                OnPropertyChanged();
            }
        }

        public static readonly DependencyProperty CodeFileDescriptionProperty =
            DependencyProperty.Register("CodeFileDescription", typeof(string), typeof(ReportMenuItem), new PropertyMetadata(""));

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

        public static readonly DependencyProperty CodeFilePathProperty =
            DependencyProperty.Register("CodeFilePath", typeof(string), typeof(ReportMenuItem), new PropertyMetadata(""));

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public bool IsSelected
        {
            get { return (bool)GetValue(IsSelectedProperty); }
            set
            {
                SetValue(IsSelectedProperty, value);
                TextBoxVisiblity = _fileName != "" && IsSelected ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        public static readonly DependencyProperty IsSelectedProperty =
            DependencyProperty.Register("IsSelected", typeof(bool), typeof(ReportMenuItem), new PropertyMetadata(false));

        public ReportMenuItem()
        {
            InitializeComponent();
            TextBoxVisiblity = Visibility.Collapsed;
            DataContext = this;
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
    }
}
