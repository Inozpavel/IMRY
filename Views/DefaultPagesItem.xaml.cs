using System;
using System.Windows.Controls;

namespace WorkReportCreator.Views
{
    public partial class DefaultPagesItem : UserControl
    {
        public event Action<object> ButtonBackClicked;

        public event Action<object> ButtonSaveAllClicked;

        public event Action<object> ButtonGenerateAllClicked;
        public DefaultPagesItem()
        {
            InitializeComponent();
        }

        private void ButtonBackClick(object sender, System.Windows.RoutedEventArgs e) => ButtonBackClicked?.Invoke(this);

        private void ButtonGenerateAllClick(object sender, System.Windows.RoutedEventArgs e) => ButtonGenerateAllClicked?.Invoke(this);

        private void ButtonSaveAllClick(object sender, System.Windows.RoutedEventArgs e) => ButtonSaveAllClicked?.Invoke(this);
    }
}
