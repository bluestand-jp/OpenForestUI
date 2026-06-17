using System.ComponentModel;

namespace OpenForestUI.MVVM.Core.Services
{
    /// <summary>
    /// The top-level destinations reachable from the sidebar. Today the menu only really
    /// swaps between Home and Settings; PickBan/Ingame/PostGame are sub-states rendered inside
    /// the Home host. The navigation service formalizes the seam so those three can be promoted
    /// to first-class navigated pages without the view-models reaching into the concrete window.
    /// </summary>
    internal enum AppRoute
    {
        Home,
        PickBan,
        Ingame,
        PostGame,
        Settings
    }

    /// <summary>
    /// Single source of truth for "which page is showing". The shell binds a ContentControl to
    /// <see cref="CurrentView"/> and the sidebar RadioButtons bind their IsChecked to
    /// <see cref="CurrentRoute"/> (via EnumToBooleanConverter). Replaces the old pattern where
    /// MainViewModel held a concrete MainWindow reference and called Window.SetXSelected().
    /// </summary>
    internal interface INavigationService : INotifyPropertyChanged
    {
        object CurrentView { get; }
        AppRoute CurrentRoute { get; }
        void NavigateTo(AppRoute route);
    }
}
