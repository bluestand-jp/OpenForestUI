using System.ComponentModel;
using OpenForestUI.MVVM.ViewModel;

namespace OpenForestUI.MVVM.Core.Services
{
    /// <summary>
    /// Holds live, non-persisted app state that the UI observes (as opposed to <see cref="IConfigService"/>,
    /// which owns persisted settings). Pilot 1 only exposes the LCU/game connection status that the
    /// titlebar pill binds to; Pilot 2 adds the Ingame roster and runtime toggles. The setter marshals
    /// onto the UI thread so the LCU/game callbacks (which fire on background threads) are safe — this
    /// closes the latent cross-thread write in AppStateController.
    /// </summary>
    internal interface IStateService : INotifyPropertyChanged
    {
        ConnectionStatusViewModel ConnectionStatus { get; set; }
    }
}
