using System.Windows;

namespace OpenForestUI.MVVM.Core.Services
{
    /// <summary>
    /// Abstracts the window-management actions the shell needs so they become bindable commands
    /// instead of code-behind Click handlers. Drag is handled separately by an attached behavior
    /// (DragMove is inherently view-level); this covers minimize/close.
    /// </summary>
    internal interface IWindowService
    {
        /// <summary>Associate the service with the shell window (called once the window exists).</summary>
        void Register(Window window);
        void Minimize();
        void Close();
    }
}
