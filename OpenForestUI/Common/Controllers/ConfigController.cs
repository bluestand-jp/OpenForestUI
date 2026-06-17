using OpenForestUI.Common.Data.Config;
using OpenForestUI.Farsight;
using OpenForestUI.Ingame.Data.Config;
using OpenForestUI.Ingame.Data.Frontend;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using System.Windows;

namespace OpenForestUI.Common.Controllers
{
    class ConfigController
    {
        private static ConfigController _instance;
        public static ConfigController Instance => GetInstance();

        public static ChampSelect.Data.Config.PickBanConfig PickBan = new();
        public static ConfigWatcher PickBanWatcher;

        public static ComponentConfig Component = new();
        public static ConfigWatcher ComponentWatcher;

        public static FarsightConfig Farsight = new();
        public static ConfigWatcher FarsightWatcher;

        public static IngameConfig Ingame = new();
        public static ConfigWatcher IngameWatcher;

        public static string ConfigLocation = Path.Combine(Directory.GetCurrentDirectory(), "Config");
        public static string FontLocation = Path.Combine(Directory.GetCurrentDirectory(), "Cache", "Fonts");

        public ObservableCollection<LocalFont> LocalFonts;

        public ConfigController()
        {
            Log.Info("Starting Config Controller");

            JSONConfigProvider controller = JSONConfigProvider.Instance;

            controller.ReadConfig(Component);
            controller.ReadConfig(PickBan);
            controller.ReadConfig(Ingame);

            if (PickBan.FileVersion == null || Component.FileVersion == null || Ingame.FileVersion == null)
            {
                Log.Warn("Config load failed");
                MessageBoxResult result = MessageBox.Show("Failed to load configuration. Corrupted Install detected. Try removing Config folder and restarting", "OpenForestUI", MessageBoxButton.OK, MessageBoxImage.Error);
                Application.Current.Dispatcher.Invoke((Action)delegate
                {
                    Application.Current.Shutdown();
                });
            }

            Log.Info("Loading config watchers");
            PickBanWatcher = new("PickBan.json", PickBan);
            ComponentWatcher = new("Component.json", Component);
            IngameWatcher = new("Ingame.json", Ingame);
            App.Instance.Exit += OnClose;
        }

        public static void LoadOffsetConfig()
        {
            // When the memory reader is disabled (Vanguard-compatible default), skip
            // loading offsets entirely — they are only meaningful for Farsight.
            if (!FarsightController.ShouldRun)
            {
                Log.Info("Memory reader disabled, skipping Farsight offset load");
                return;
            }

            JSONConfigProvider.Instance.ReadConfig(Farsight);
            if (Farsight.FileVersion == null)
            {
                Log.Warn("Could not load Offsets");
                MessageBoxResult result = MessageBox.Show("Failed to load offsets. Manually download or write Config/Farsight.json. Check github for current file version. Ingame will not work properly!", "OpenForestUI", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            FarsightWatcher = new("Farsight.json", Farsight);
        }

        private static ConfigController GetInstance()
        {
            if (_instance == null)
            {
                _instance = new();
            }

            return _instance;
        }

        public static void UpdateConfigFile(JSONConfig config)
        {
            JSONConfigProvider.Instance.WriteConfig(config);
        }

        private void OnClose(object sender, EventArgs e)
        {
            Log.Info("Saving all configs to file");
            JSONConfigProvider controller = JSONConfigProvider.Instance;

            try
            {
                // FarsightWatcher is only created when the memory reader is enabled
                // (LoadOffsetConfig short-circuits otherwise), so it can be null on the
                // Vanguard-safe default. Null-guard every teardown so a missing watcher
                // doesn't abort the whole save block and drop the other configs on exit.
                PickBanWatcher?.Stop();
                ComponentWatcher?.Stop();
                FarsightWatcher?.Stop();
                IngameWatcher?.Stop();
                controller.WriteConfig(PickBan);
                controller.WriteConfig(Component);
                // Only persist Farsight when it was actually loaded; otherwise the in-memory
                // default would overwrite a valid Farsight.json on disk.
                if (FarsightWatcher != null)
                    controller.WriteConfig(Farsight);
                controller.WriteConfig(Ingame);

                Log.Info("Configs saved");
            }
            catch
            {
                Log.Warn("Could not save all configs");
            }
        }
    }

    public class ConfigWatcher
    {
        private FileSystemWatcher watcher;
        private JSONConfig config;
        private bool waitingForRead;

        public ConfigWatcher(string configName, JSONConfig config)
        {
            this.config = config;

            watcher = new FileSystemWatcher(ConfigController.ConfigLocation)
            {
                NotifyFilter = NotifyFilters.LastWrite
            };

            watcher.Error += OnError;
            watcher.Changed += OnChanged;
            watcher.Filter = configName;

            watcher.IncludeSubdirectories = false;
            watcher.EnableRaisingEvents = true;

            Log.Info($"Watching {watcher.Filter} for changes");
        }

        public void Stop()
        {
            watcher.Dispose();
        }

        public async void OnChanged(object sender, FileSystemEventArgs e)
        {
            if (waitingForRead)
            {
                return;
            }

            waitingForRead = true;

            Log.Info($"{watcher.Filter} change detected");
            int attempts = 0;
            while (attempts < 10)
            {
                try
                {
                    await Task.Delay(500);
                    config.Reload();
                    await Task.Delay(500);
                    waitingForRead = false;
                    return;
                }
                catch
                {
                    Log.Warn($"{watcher.Filter} locked, cannot read!");
                    attempts++;
                }
            }

        }

        protected void OnError(object sender, ErrorEventArgs e)
        {
            Log.Warn("File Watch error:" + e.GetException());
        }
    }
}
