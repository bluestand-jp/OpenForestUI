using OpenForestUI.Common.Controllers;
using OpenForestUI.MVVM.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenForestUI.MVVM.ViewModel
{
    class SettingsViewModel : ObservableObject
    {
        public SettingsViewModel()
        {
            // Keep the "OCR (accurate CS/Gold)" card's status live as the env provisions
            // (the event may fire off the UI thread, so marshal to the dispatcher).
            OcrEnvController.StatusChanged += (_, _) =>
            {
                var d = System.Windows.Application.Current?.Dispatcher;
                if (d != null) d.Invoke(() => OnPropertyChanged(nameof(OcrStatusText)));
                else OnPropertyChanged(nameof(OcrStatusText));
            };
        }

        /// <summary>Human-readable OCR environment provisioning status (bound to the Settings card).</summary>
        public string OcrStatusText => OcrEnvController.Status switch
        {
            OcrEnvStatus.Ready => "Ready — accurate CS/Gold OCR is set up.",
            OcrEnvStatus.Provisioning => OcrEnvController.StatusText,
            OcrEnvStatus.Failed => "Setup failed (" + OcrEnvController.StatusText + "). Click “Set up OCR now” to retry.",
            _ => "Not set up. Click “Set up OCR now” to download the Python OCR dependencies (~hundreds of MB, one time).",
        };

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
