using OpenForestUI.Common.Data.Provider;
using OpenForestUI.Farsight;
using OpenForestUI.Http;
using OpenForestUI.MVVM.Core.Services;
using OpenForestUI.MVVM.View;
using OpenForestUI.MVVM.ViewModel;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using static OpenForestUI.Common.Log;

namespace OpenForestUI.Common.Controllers
{
    class BroadcastController
    {
        public static string AppVersion = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).FileVersion;
        public static int TickRate = 2;
        public static LeagueState CurrentLeagueState;
        public static BroadcastController Instance => GetInstance();
        public static EventHandler EarlyInitComplete, InitComplete, PostInitComplete;

        private static BroadcastController _instance;

        public ConfigController CfgController;
        public PickBanController PBController;
        public IngameController IGController;
        public AppStateController AppStController;
        public ReplayAPIController ReplayController;
        public DataDragon DDragon;
        public PickBanConnector PBConnector;
        public FarsightController MemoryController;
        public GameInputController GIController;

        public List<ITickable> ToTick;

        public MainWindow Main { get; private set; }
        public StartupWindow Startup { get; private set; }


        private Timer tickTimer;
        private StartupViewModel _startupContext;
        private MainViewModel _mainContext;

        private DateTime loadStart, initFinish;
        private BroadcastController()
        {
            loadStart = DateTime.Now;
            initFinish = DateTime.Now;

            EarlyInit();
        }

        private static BroadcastController GetInstance()
        {
            if (_instance == null)
            {
                _instance = new();
            }

            return _instance;
        }

        private void StatusUpdate(string Status)
        {
            Log.Info(Status);
            _startupContext.Status = Status;
        }

        private async void EarlyInit()
        {
            DataDragon.FinishLoading += (s, e) => Init();
            InitComplete += (s, e) => PostInit();

            Startup = new StartupWindow();
            Startup.Show();
            _startupContext = (StartupViewModel)Startup.DataContext;
            _startupContext.Status = "Early Init";

            ToTick = new();

            _ = new Log(LogLevel.Verbose, FileVersionInfo.GetVersionInfo("OpenForestUI.exe").FileVersion);


            CfgController = ConfigController.Instance;
            Log.SetLogLevel(ConfigController.Component.App.LogLevel);
            Log.Info($"OpenForestUI Version {ConfigController.Component.App.Version}");

            EarlyInitComplete?.Invoke(null, EventArgs.Empty);
            Log.Info($"Early Init Complete in {(DateTime.Now - loadStart).TotalMilliseconds}ms");
            initFinish = DateTime.Now;

            _startupContext.UpdateLoadProgress(LoadStatus.PreInit);

            await Task.Delay(50);

            DDragon = DataDragon.Instance;
        }

        private void Init()
        {
            Log.Info("DDragon loaded");

            StatusUpdate("Loading PickBan Controller");
            PBController = new();
            _startupContext.UpdateLoadProgress(LoadStatus.Init, 25);

            StatusUpdate("Loading Ingame Controller");
            IGController = new();
            GIController = new();
            _startupContext.UpdateLoadProgress(LoadStatus.Init, 35);

            StatusUpdate("Loading Replay Controller");
            ReplayController = new();
            _startupContext.UpdateLoadProgress(LoadStatus.Init, 50);

            StatusUpdate("Loading Farsight");
            // Sync the Farsight static gate with config before constructing the controller —
            // the constructor short-circuits when ShouldRun is false.
            FarsightController.ShouldRun = ConfigController.Component.Ingame.UseMemoryReader;
            MemoryController = new();

            StatusUpdate("Loading Frontend Webserver (HTTP/WS)");

            EmbedIOServer WebServer = new EmbedIOServer("*", ConfigController.Component.App.FrontendPort);
            _startupContext.UpdateLoadProgress(LoadStatus.Init, 85);

            StatusUpdate("Whats that ticking noise?");
            tickTimer = new Timer { Interval = 1000 / TickRate };
            tickTimer.Elapsed += DoTick;
            _startupContext.UpdateLoadProgress(LoadStatus.Init);

            Log.Info($"Init Complete in {(DateTime.Now - initFinish).ToString(@"s\.fff")}s");
            initFinish = DateTime.Now;
            InitComplete?.Invoke(null, EventArgs.Empty);


        }

        private void PostInit()
        {
            Log.Info("Opening main window");
            Application.Current.Dispatcher.Invoke((Action)delegate
            {
                Main = new();

                // DataContext is no longer set in XAML; build the root view-model from the DI
                // container. Anchored here (after Init() built the controllers) so the eager Ingame
                // VM graph resolves IngameController.CurrentSettings only once that exists (C2).
                _mainContext = ActivatorUtilities.CreateInstance<MainViewModel>(App.Services);
                Main.DataContext = _mainContext;
                Main.Show();

                App.Services.GetRequiredService<IWindowService>().Register(Main);

                StatusUpdate("Loading State Controller");
                AppStController = AppStateController.Instance;
            });

            AppStController.Init();

            AppStateController.GameStart += (s, p) =>
            {
                if (FarsightController.ShouldRun)
                {
                    MemoryController.Connect(p);
                }

                // Mock is intentionally NOT auto-disabled here. The user may keep a Mock preview running
                // across a real game, and Mock takes priority: IngameController.Broadcast gates the live
                // feed off while Mock is on, so the two never interleave. Turning Mock off hands control
                // back to the live feed on the very next tick (seamless). The chip shows MOCKING while
                // Mock is on via AppStateController's tick resolver.
            };

            // GameStop needs no handler here: IngameController.OnGameStop tears the overlay down and
            // the tick resolver moves the chip back to "Client Loaded" on its own.



            _startupContext.UpdateLoadProgress(LoadStatus.PostInit, 33);

            PBConnector = new PickBanConnector();
            ToTick.Add(AppStController);
            _startupContext.UpdateLoadProgress(LoadStatus.PostInit, 66);

            Application.Current.Dispatcher.Invoke((Action)delegate
            {
                Startup.Close();
            });

            Log.Info($"Starting OpenForestUI with tickrate of {TickRate}tps");
            tickTimer.Start();

            Log.Info("Checking for running Game");
            IGController.StartWaitingForTargetProcess();
            _startupContext.UpdateLoadProgress(LoadStatus.PostInit);

            PostInitComplete?.Invoke(null, EventArgs.Empty);
            Log.Info($"Post Init Complete in {(DateTime.Now - initFinish).TotalMilliseconds}ms");
            Log.Info($"Total Startup time: {DateTime.Now - loadStart:s\\.fff}s");
        }

        public void OnAppExit()
        {
            //IngameController.OnExit();
        }

        private void DoTick(object sender, EventArgs e)
        {
            //Thread safe iterator
            List<ITickable> tickNow = ToTick.GetRange(0, ToTick.Count);
            tickNow.ForEach(tickable => tickable.DoTick());
        }
    }

    [Flags]
    public enum LeagueState
    {
        Connected,
        ChampSelect,
        InProgress,
        PostGame
    }
}
