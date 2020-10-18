using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Controls;

namespace WorkReportCreator.Views
{
    /// <summary>
    /// Вкладка с быстрыми действиями
    /// </summary>
    public partial class FastActionsItem : UserControl, INotifyPropertyChanged
    {
        /// <summary>
        /// Происходит, при нажатии на кнопку назад
        /// </summary>
        public event Action<object> ButtonBackClicked;

        /// <summary>
        /// Происходит, при нажатии на кнопку сохранить все
        /// </summary>
        public event Action<object> ButtonSaveAllClicked;

        /// <summary>
        /// Происходит, при нажатии на кнопку сгенерировать все
        /// </summary>
        public event Action<object> ButtonGenerateAllClicked;

        public event PropertyChangedEventHandler PropertyChanged;

        private bool _isButtonEnabled = false;
        public bool IsButtonEnabled
        {
            get => _isButtonEnabled;
            set
            {
                _isButtonEnabled = value;
                OnPropertyChanged();
            }
        }

        public FastActionsItem()
        {
            InitializeComponent();
            DataContext = this;
        }

        public void OnPropertyChanged([CallerMemberName] string propertyName = "") => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        private void ButtonBackClick(object sender, System.Windows.RoutedEventArgs e) => ButtonBackClicked?.Invoke(this);

        private void ButtonGenerateAllClick(object sender, System.Windows.RoutedEventArgs e) => ButtonGenerateAllClicked?.Invoke(this);

        private void ButtonSaveAllClick(object sender, System.Windows.RoutedEventArgs e) => ButtonSaveAllClicked?.Invoke(this);
    }
}
