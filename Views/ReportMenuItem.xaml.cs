using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;

namespace WorkReportCreator.Views
{
    public partial class ReportMenuItem : UserControl, INotifyPropertyChanged
    {
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

        public ReportMenuItem()
        {
            InitializeComponent();
            DataContext = this;
        }
    }
}
