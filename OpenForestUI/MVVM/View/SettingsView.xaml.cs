using OpenForestUI.Common;
using OpenForestUI.Common.Controllers;
using OpenForestUI.Common.Utils;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace OpenForestUI.MVVM.View
{
    /// <summary>
    /// Interaction logic for SettingsView.xaml
    /// </summary>
    public partial class SettingsView : UserControl
    {

        private static Dictionary<short, string> LangToIndex = new Dictionary<short, string>()
        {
            { 0, "en_US"},
            { 1, "de_DE"},
            { 2, "pt_BR"},
            { 3, "fr_FR"},
            { 4, "es_ES"},
            { 5, "it_IT"},
            { 6, "ja_JP"},
            { 7, "tr_TR"},
            { 8, "hu_HU"},
            { 9, "pl_PL"},
            { 10, "ru_RU"},
            { 11, "ro_RO"},
            { 12, "cs_CZ"},
            { 13, "zh_CN"},
            { 14, "zh_TW"},
            { 15, "ko_KR"}
        };
        public SettingsView()
        {
            InitializeComponent();
            LogLevelSelector.SelectedIndex = (int)ConfigController.Component.App.LogLevel;
            LangSelector.SelectedIndex = LangToIndex.KeyByValue(ConfigController.Component.DataDragon.Locale);
        }

        private void LogLevelSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Log.LogLevel level = (Log.LogLevel)Enum.Parse(typeof(Log.LogLevel), ((ComboBoxItem)LogLevelSelector.SelectedItem).Tag.ToString());
            Log.SetLogLevel(level);
            if (level != ConfigController.Component.App.LogLevel)
            {
                Log.Write($"Log Level set to {level.ToString()}");
                ConfigController.Component.App.LogLevel = level;
            }
        }


        private void LangSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string lang = ((ComboBoxItem)LangSelector.SelectedItem).Tag.ToString();
            if (lang != ConfigController.Component.DataDragon.Locale)
            {
                Log.Write($"Locale set to {lang}");
                ConfigController.Component.DataDragon.Locale = lang;
                ConfigController.UpdateConfigFile(ConfigController.Component);
            }
        }

        // "Set up OCR now" — provision (or retry) the bundled-Python OCR environment in the
        // background. Status flows back to the card via SettingsViewModel.OcrStatusText.
        private void OcrSetup_Click(object sender, RoutedEventArgs e)
        {
            OcrEnvController.Retry();
        }

    }
}
