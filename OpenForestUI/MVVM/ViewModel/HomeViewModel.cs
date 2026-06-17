using CommunityToolkit.Mvvm.ComponentModel;
using System.Windows.Input;

namespace OpenForestUI.MVVM.ViewModel
{
    // Modernized to source-gen: inherits the CommunityToolkit ObservableObject directly (the
    // intended end-state) so [ObservableProperty] works; unconverted view-models still use the
    // OpenForestUI.MVVM.Core.ObservableObject shim.
    internal partial class HomeViewModel : ObservableObject
    {
        // Child view-models are injected (the DI singletons), replacing the old reach-ins through
        // the MainViewModel.*VM static façade.
        public PickBanViewModel PickBanVM { get; }
        public IngameViewModel IngameVM { get; }
        public PostGameViewModel PostGameVM { get; }
        public InfoEditViewModel InfoEditVM { get; }

        [ObservableProperty]
        private bool _infoIsOpen;

        [ObservableProperty]
        private bool _infoButtonIsVisible = true;

        // Kept as a DelegateCommand because both chevrons (HomeView + InfoEditView) bind its
        // MouseGesture; retired together with the other gesture bindings in Pilot 2.
        public OpenForestUI.MVVM.Core.DelegateCommand InfoEditButtonCommand { get; }

        public HomeViewModel(
            PickBanViewModel pickBan,
            IngameViewModel ingame,
            PostGameViewModel postGame,
            InfoEditViewModel infoEdit)
        {
            PickBanVM = pickBan;
            IngameVM = ingame;
            PostGameVM = postGame;
            InfoEditVM = infoEdit;

            InfoEditButtonCommand = new(o => { InfoIsOpen ^= true; });
            InfoEditButtonCommand.MouseGesture = MouseAction.LeftClick;
        }
    }
}
