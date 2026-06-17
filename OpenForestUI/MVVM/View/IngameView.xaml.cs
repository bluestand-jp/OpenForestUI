using OpenForestUI.MVVM.ViewModel;
using System.Windows.Controls;

namespace OpenForestUI.MVVM.View
{
    /// <summary>
    /// Interaction logic for IngameView.xaml. The feature panels are now Fluent toggle lists; the
    /// per-tile DataContext wiring is gone. Objectives/Players/Teams are plain fields on the view-model
    /// (not bindable properties), so the three panel DataContexts are still assigned here — null-guarded
    /// because DataContext is cleared to null when the page is navigated away from (shell teardown).
    /// </summary>
    public partial class IngameView : UserControl
    {
        public IngameView()
        {
            InitializeComponent();

            OpenContent.Width = 360;
            OpenContent.Opacity = 0;

            DataContextChanged += (s, e) =>
            {
                if (e.NewValue is not IngameViewModel ctx)
                    return;
                ObjectivePanel.DataContext = ctx.Objectives;
                PlayerPanel.DataContext = ctx.Players;
                TeamPanel.DataContext = ctx.Teams;
            };
        }

        private void MainContainer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
        }
    }
}
