using System;
using System.Windows;

namespace OpenForestUI.MVVM.View
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml. Window chrome (drag / minimize / close) is handled by
    /// the WindowDragBehavior + IWindowService-backed commands, not code-behind. The only remaining
    /// behaviour is keeping owned dialogs centered on the window.
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            LocationChanged += Window_LocationChanged;
        }

        private void Window_LocationChanged(object sender, EventArgs e)
        {
            foreach (Window win in this.OwnedWindows)
            {
                win.Top = (this.Top + this.Height / 2) - win.Height / 2;
                win.Left = (this.Left + this.Width / 2) - win.Width / 2;
            }
        }
    }
}
