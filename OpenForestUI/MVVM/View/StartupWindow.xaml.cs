using OpenForestUI.MVVM.ViewModel;
using System;
using System.Windows;

namespace OpenForestUI.MVVM.View
{
    /// <summary>
    /// Interaction logic for StartupWindow.xaml
    /// </summary>
    public partial class StartupWindow : Window
    {
        public StartupWindow()
        {
            InitializeComponent();
        }

        public StartupViewModel GETDataContext()
        {
            return (StartupViewModel)DataContext;
        }

        private void UpdateNow_Click(object sender, RoutedEventArgs e)
        {
            StartupViewModel ctx = (StartupViewModel)this.DataContext;
            ctx.Update?.Invoke(null, EventArgs.Empty);

        }

        private void UpdateSkip_Click(object sender, RoutedEventArgs e)
        {
            StartupViewModel ctx = (StartupViewModel)this.DataContext;
            ctx.SkipUpdate?.Invoke(null, EventArgs.Empty);
        }

    }
}
