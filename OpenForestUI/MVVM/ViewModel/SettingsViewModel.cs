using OpenForestUI.Common.Controllers;
using OpenForestUI.MVVM.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenForestUI.MVVM.ViewModel
{
    class SettingsViewModel : ObservableObject
    {
        public bool OffsetUpdate
        {
            get { return ConfigController.Component.App.CheckForOffsets; }
            set { ConfigController.Component.App.CheckForOffsets = value; OnPropertyChanged(); }
        }

        public bool UseMemoryReader
        {
            get { return ConfigController.Component.Ingame.UseMemoryReader; }
            set
            {
                ConfigController.Component.Ingame.UseMemoryReader = value;
                Farsight.FarsightController.ShouldRun = value;
                OnPropertyChanged();
            }
        }

        // Dev/preview: feed a canned game state to the Ingame overlay so the PRM bar can be tuned in
        // OBS without a live game. Transient (defaults off each launch); see MockController.
        public bool UseMock
        {
            get { return MockController.IsEnabled; }
            set { MockController.SetEnabled(value); OnPropertyChanged(); }
        }

        // ---- Footer (credits + version) at the bottom of the Settings page. Placeholder text —
        // edit the Credits string to taste. Version comes from the assembly's FileVersion. ----
        public string AppVersion => "OpenForestUI  v" + BroadcastController.AppVersion;
        public string Credits => "MIT License · Repository Maintainer: Negi - BlueStand";
    }
}
