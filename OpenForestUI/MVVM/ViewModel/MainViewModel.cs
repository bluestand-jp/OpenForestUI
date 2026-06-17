using OpenForestUI.Common;
using OpenForestUI.MVVM.Core;
using OpenForestUI.MVVM.Core.Services;
using Microsoft.Extensions.DependencyInjection;

namespace OpenForestUI.MVVM.ViewModel
{
    class MainViewModel : ObservableObject
    {
        /// <summary>
        /// Single sidebar navigation command. CommandParameter is the target <see cref="AppRoute"/>
        /// (bound via <c>{x:Static services:AppRoute.X}</c>); the RadioButton highlight follows
        /// <c>Nav.CurrentRoute</c>, so no code-behind RadioButton poking is needed.
        /// </summary>
        public RelayCommand NavigateCommand { get; }

        /// <summary>Window-chrome commands, backed by IWindowService (replaces code-behind Click handlers).</summary>
        public RelayCommand MinimizeCommand { get; }

        public RelayCommand CloseCommand { get; }

        // Strangler façade: legacy view-models/controllers still read MainViewModel.XVM statically.
        // These now resolve the DI singletons (the very instances the navigation service holds), so old
        // and new wiring share one object graph. Removal trigger: delete once
        //   grep "MainViewModel\.(Home|PickBan|Ingame|PostGame|Settings|InfoEdit)VM"
        // returns zero outside this file (after Pilot 2).
        public static HomeViewModel HomeVM => App.Services.GetRequiredService<HomeViewModel>();
        public static SettingsViewModel SettingsVM => App.Services.GetRequiredService<SettingsViewModel>();
        public static PickBanViewModel PickBanVM => App.Services.GetRequiredService<PickBanViewModel>();
        public static IngameViewModel IngameVM => App.Services.GetRequiredService<IngameViewModel>();
        public static PostGameViewModel PostGameVM => App.Services.GetRequiredService<PostGameViewModel>();
        public static InfoEditViewModel InfoEditVM => App.Services.GetRequiredService<InfoEditViewModel>();

        private readonly INavigationService _nav;
        private readonly IStateService _state;
        private readonly IWindowService _window;

        /// <summary>Bound by the shell ContentControl (<c>Nav.CurrentView</c>) and sidebar RadioButtons (<c>Nav.CurrentRoute</c>).</summary>
        public INavigationService Nav => _nav;

        /// <summary>Bound by the titlebar status pill (<c>State.ConnectionStatus</c>).</summary>
        public IStateService State => _state;

        /// <summary>
        /// Kept for the existing writers (AppStateController / BroadcastController) that assign
        /// <c>mainCtx.ConnectionStatus = ...</c>. Forwards to the state service, whose setter marshals
        /// to the UI thread.
        /// </summary>
        public ConnectionStatusViewModel ConnectionStatus
        {
            get => _state.ConnectionStatus;
            set { _state.ConnectionStatus = value; Log.Info("Connection State Changed"); }
        }

        public MainViewModel(INavigationService nav, IStateService state, IWindowService window)
        {
            _nav = nav;
            _state = state;
            _window = window;

            NavigateCommand = new(o => { if (o is AppRoute route) _nav.NavigateTo(route); });
            MinimizeCommand = new(o => _window.Minimize());
            CloseCommand = new(o => _window.Close());
        }
    }
}
