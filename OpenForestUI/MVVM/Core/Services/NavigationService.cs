using OpenForestUI.MVVM.ViewModel;

namespace OpenForestUI.MVVM.Core.Services
{
    /// <summary>
    /// Concrete navigation host. Home is the dashboard; PickBan/Ingame/PostGame are first-class
    /// navigated pages (CurrentView swaps to the page view-model and its IsOpen flag is set so the
    /// full view renders at 1080px); Settings swaps to the settings view-model. Returning to Home
    /// closes the InfoEdit overlay and re-shows the chevron.
    /// </summary>
    internal class NavigationService : ObservableObject, INavigationService
    {
        private readonly HomeViewModel _home;
        private readonly PickBanViewModel _pickBan;
        private readonly IngameViewModel _ingame;
        private readonly PostGameViewModel _postGame;
        private readonly SettingsViewModel _settings;

        public NavigationService(
            HomeViewModel home,
            PickBanViewModel pickBan,
            IngameViewModel ingame,
            PostGameViewModel postGame,
            SettingsViewModel settings)
        {
            _home = home;
            _pickBan = pickBan;
            _ingame = ingame;
            _postGame = postGame;
            _settings = settings;

            _currentView = _home;
            _currentRoute = AppRoute.Home;
        }

        private object _currentView;
        public object CurrentView
        {
            get => _currentView;
            private set { _currentView = value; OnPropertyChanged(); }
        }

        private AppRoute _currentRoute;
        public AppRoute CurrentRoute
        {
            get => _currentRoute;
            private set { _currentRoute = value; OnPropertyChanged(); }
        }

        public void NavigateTo(AppRoute route)
        {
            // A page's full view renders when its IsOpen is true; collapse all, then open the target.
            _pickBan.IsOpen = route == AppRoute.PickBan;
            _ingame.IsOpen = route == AppRoute.Ingame;
            _postGame.IsOpen = route == AppRoute.PostGame;

            switch (route)
            {
                case AppRoute.Home:
                    _home.InfoIsOpen = false;
                    _home.InfoButtonIsVisible = true;
                    CurrentView = _home;
                    break;

                case AppRoute.PickBan:
                    CurrentView = _pickBan;
                    break;

                case AppRoute.Ingame:
                    CurrentView = _ingame;
                    break;

                case AppRoute.PostGame:
                    CurrentView = _postGame;
                    break;

                case AppRoute.Settings:
                    CurrentView = _settings;
                    break;
            }

            CurrentRoute = route;
        }
    }
}
