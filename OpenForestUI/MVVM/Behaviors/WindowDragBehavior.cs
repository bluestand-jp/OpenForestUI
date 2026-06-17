using System.Windows;
using System.Windows.Input;

namespace OpenForestUI.MVVM.Behaviors
{
    /// <summary>
    /// Attached behavior that lets an element drag its host window (custom title bar). Replaces the
    /// code-behind <c>Grid.MouseDown -&gt; DragMove()</c> handler. Clicks on child buttons (minimize /
    /// close / status) mark the event handled, so the drag only fires on the empty title-bar area —
    /// matching the previous behavior. Usage:
    /// <c>behaviors:WindowDragBehavior.EnableDrag="True"</c>.
    /// </summary>
    public static class WindowDragBehavior
    {
        public static readonly DependencyProperty EnableDragProperty =
            DependencyProperty.RegisterAttached(
                "EnableDrag",
                typeof(bool),
                typeof(WindowDragBehavior),
                new PropertyMetadata(false, OnEnableDragChanged));

        public static bool GetEnableDrag(DependencyObject element) => (bool)element.GetValue(EnableDragProperty);

        public static void SetEnableDrag(DependencyObject element, bool value) => element.SetValue(EnableDragProperty, value);

        private static void OnEnableDragChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not UIElement element)
                return;

            if ((bool)e.NewValue)
                element.MouseLeftButtonDown += OnMouseLeftButtonDown;
            else
                element.MouseLeftButtonDown -= OnMouseLeftButtonDown;
        }

        private static void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
                Window.GetWindow((DependencyObject)sender)?.DragMove();
        }
    }
}
