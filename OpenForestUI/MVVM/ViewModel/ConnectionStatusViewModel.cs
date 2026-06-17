using OpenForestUI.MVVM.Core;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Media;

namespace OpenForestUI.MVVM.ViewModel
{
    class ConnectionStatusViewModel : ObservableObject
    {
        private string _textContent;

        public string TextContent
        {
            get { return _textContent; }
            set { _textContent = value; OnPropertyChanged(); }
        }

        private SolidColorBrush _textColor;

        public SolidColorBrush TextColor
        {
            get { return _textColor; }
            set { _textColor = value; OnPropertyChanged(); }
        }


        private SolidColorBrush _borderColor;

        public SolidColorBrush BorderColor
        {
            get { return _borderColor; }
            set { _borderColor = value; OnPropertyChanged(); }
        }

        private double _borderThickness;

        public double BorderThickness
        {
            get { return _borderThickness; }
            set { _borderThickness = value; OnPropertyChanged(); }
        }


        private SolidColorBrush _backgroundColor;

        public SolidColorBrush BackgroundColor
        {
            get { return _backgroundColor; }
            set { _backgroundColor = value; OnPropertyChanged(); }
        }

        public ConnectionStatusViewModel() { }

        public ConnectionStatusViewModel(Color textColor, string textContent, Color borderColor, double borderThickness, Color backgroundColor)
        {
            TextColor = new SolidColorBrush(textColor);
            TextContent = textContent;
            BorderColor = new SolidColorBrush(borderColor);
            BorderThickness = borderThickness;
            BackgroundColor = new SolidColorBrush(backgroundColor);
        }

        // Status palette for the top-bar chip. The chip binds its dot to BorderColor and its label to
        // TextContent (BackgroundColor/TextColor are legacy fields kept for the old pill consumers).
        // The live status is resolved once per tick by AppStateController.UpdateConnectionStatus from
        // reliable signals (mock flag / client-connected / game-process), which is the single writer:
        //   Disconnected  red     - LCU client API not connected
        //   Client Loaded yellow  - client connected, no game running
        //   Connected     green   - in a live game / spectating
        //   Mocking       cyan    - Use Mock preview feed active (keyed off MockController.IsEnabled)
        //   Issue Found   purple  - a problem was detected        (DEFINED ONLY - not produced yet)
        public static ConnectionStatusViewModel DISCONNECTED = new(Colors.White, "Disconnected", Color.FromRgb(0xFB, 0x69, 0x62), 3, Color.FromRgb(0x3A, 0x20, 0x20));
        public static ConnectionStatusViewModel LCU = new(Colors.White, "Client Loaded", Color.FromRgb(0xF2, 0xC9, 0x4C), 3, Color.FromRgb(0x3A, 0x33, 0x1C));
        public static ConnectionStatusViewModel CONNECTED = new(Colors.White, "Connected", Color.FromRgb(0x79, 0xDE, 0x79), 3, Color.FromRgb(0x1B, 0x3A, 0x2A));
        public static ConnectionStatusViewModel MOCKING = new(Colors.White, "Mocking", Color.FromRgb(0x56, 0xCC, 0xF2), 3, Color.FromRgb(0x1B, 0x33, 0x3A));
        public static ConnectionStatusViewModel ISSUE = new(Colors.White, "Issue Found", Color.FromRgb(0xB0, 0x84, 0xE9), 3, Color.FromRgb(0x2E, 0x24, 0x3A));
    }
}
