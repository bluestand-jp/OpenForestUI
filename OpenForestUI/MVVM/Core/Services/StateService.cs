using System.Windows;
using System.Windows.Threading;
using OpenForestUI.MVVM.ViewModel;

namespace OpenForestUI.MVVM.Core.Services
{
    /// <summary>
    /// Default <see cref="IStateService"/>. Connection status starts DISCONNECTED and is updated by
    /// the LCU/game lifecycle (AppStateController / BroadcastController). Writes are marshalled to the
    /// dispatcher thread so background-thread callers don't touch WPF objects off-thread.
    /// </summary>
    internal class StateService : ObservableObject, IStateService
    {
        private ConnectionStatusViewModel _connectionStatus = ConnectionStatusViewModel.DISCONNECTED;

        public ConnectionStatusViewModel ConnectionStatus
        {
            get => _connectionStatus;
            set
            {
                Dispatcher dispatcher = Application.Current?.Dispatcher;
                if (dispatcher != null && !dispatcher.CheckAccess())
                {
                    dispatcher.Invoke(() => ConnectionStatus = value);
                    return;
                }

                _connectionStatus = value;
                OnPropertyChanged();
            }
        }
    }
}
