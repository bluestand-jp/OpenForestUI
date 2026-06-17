using OpenForestUI.Common.Controllers;
using OpenForestUI.MVVM.Core.Services;
using OpenForestUI.MVVM.ViewModel;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Windows;

namespace OpenForestUI
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static App Instance;

        /// <summary>The application DI container. Resolved from in <c>BroadcastController.PostInit</c>.</summary>
        public static IServiceProvider Services { get; private set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            Instance = this;

            // INVARIANT (C1): build the container BEFORE BroadcastController.Instance is ever touched.
            // BroadcastController.PostInit and AppStateController resolve view-models + services through
            // App.Services while creating the main window, so the provider must already exist. The
            // BroadcastController.Instance access MUST remain the last statement here.
            ServiceCollection services = new();
            ConfigureServices(services);
            Services = services.BuildServiceProvider();

            // Brand the Fluent (WPF-UI) accent green (#5CC59E) so toggles, primary buttons and
            // selection states match OpenForestUI's identity across every restyled surface, instead of
            // WPF-UI's default blue. The merged ThemesDictionary/ControlsDictionary are already loaded
            // (App resources init before OnStartup), so swapping the accent dynamic resources is safe here.
            Wpf.Ui.Appearance.ApplicationAccentColorManager.Apply(
                System.Windows.Media.Color.FromRgb(0x5C, 0xC5, 0x9E),
                Wpf.Ui.Appearance.ApplicationTheme.Dark);

            base.OnStartup(e);

            BroadcastController bController = BroadcastController.Instance;
        }

        private static void ConfigureServices(ServiceCollection services)
        {
            // Application services.
            services.AddSingleton<INavigationService, NavigationService>();
            services.AddSingleton<IConfigService, ConfigService>();
            services.AddSingleton<IStateService, StateService>();
            services.AddSingleton<IWindowService, WindowService>();

            // Page view-models, all singletons (the menu keeps one of each alive for the app's lifetime).
            // MainViewModel is intentionally NOT registered: it is created in BroadcastController.PostInit
            // via ActivatorUtilities, anchored after Init() so the eager Ingame VM graph resolves runtime
            // state (IngameController.CurrentSettings) only once that controller exists (C2).
            services.AddSingleton<HomeViewModel>();
            services.AddSingleton<PickBanViewModel>();
            services.AddSingleton<IngameViewModel>();
            services.AddSingleton<PostGameViewModel>();
            services.AddSingleton<SettingsViewModel>();
            services.AddSingleton<InfoEditViewModel>();
        }
    }
}
