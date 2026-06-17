using System.Windows;

namespace OpenForestUI.MVVM.Core.Services
{
    /// <summary>
    /// Operates on the registered shell window. BroadcastController.PostInit calls
    /// <see cref="Register"/> right after the MainWindow is shown.
    /// </summary>
    internal class WindowService : IWindowService
    {
        private Window _window;

        public void Register(Window window) => _window = window;

        public void Minimize()
        {
            if (_window != null)
                _window.WindowState = WindowState.Minimized;
        }

        public void Close() => _window?.Close();
    }
}
