> 🌐 **English** ・ [日本語](README-ja.md)

# Control-center Views (WPF XAML pages & windows)

The visual layer of the OpenForestUI desktop control center: the Fluent/WPF-UI dashboard shell plus the per-feature pages (Pre Game / Ingame / Post Game / Settings) and a few popup windows. Each `*.xaml` is paired with a thin `*.xaml.cs` code-behind. Pages bind to the view-models in [`../ViewModel/`](../ViewModel/) and are swapped into the shell by the `INavigationService` (see [`../Core/Services/`](../Core/Services/)); routing is keyed off the `AppRoute` enum. As part of the MVVM modernization most chrome/click logic was moved out of code-behind into commands, behaviors, and services — the remaining code-behind handles only what XAML binding cannot (file dialogs, drag/drop, list seeding).

## Contents

| File | Role |
| --- | --- |
| `MainWindow.xaml(.cs)` | Top-level borderless window: sidebar navigation (RadioButtons bound to `MainViewModel.NavigateCommand`), titlebar status pill, and the `ContentControl` host that renders `Nav.CurrentView`. Code-behind only keeps owned dialogs centered; drag/min/close run through behaviors + `IWindowService`. |
| `StartupWindow.xaml(.cs)` | Loading splash shown before the main window: glow background, status text, and a progress bar driven by `StartupViewModel.LoadProgress`. Hosts the optional "update available" prompt (Update / Skip buttons raise the view-model's `Update`/`SkipUpdate` events). |
| `HomeView.xaml(.cs)` | The dashboard landing page; hosts the feature cards (Pre Game / Ingame / Post Game) and the InfoEdit overlay chevron. Pure code-behind (just `InitializeComponent`). |
| `PickBanView.xaml(.cs)` | Champion-select ("Pre Game") configuration page: team names/tags/scores/coach, patch, per-team colors and logos, saved-team selectors, and side swap. Code-behind handles text validation, team-config persistence via `JSONConfigProvider`, logo file dialogs, and launching `ColorPickerWindow`. |
| `IngameView.xaml(.cs)` | Ingame overlay configuration page: Fluent toggle lists for the Objectives / Players / Teams feature groups. Code-behind assigns the three panel `DataContext`s from `IngameViewModel` (they are plain fields, not bindable) and is null-guarded for shell teardown. |
| `IngameTeamsView.xaml(.cs)` | Live roster panel for the ingame overlay: drag-and-drop reorderable blue/red player lists and a best-of series-count selector. Code-behind wires `ListViewDragDropManager` (vendored `WPF.JoshSmith` drag/drop) and exposes static `InitPlayers`/`ClearPlayers` for the broadcast controller. |
| `PostGameView.xaml(.cs)` | Post-game overlay configuration page. Minimal code-behind (collapses the detail pane on load). |
| `SettingsView.xaml(.cs)` | App settings page: log-level and Data Dragon locale selectors, offset-update / memory-reader (Farsight) / mock-feed toggles, plus a credits + version footer. Code-behind maps the combo selections to config. |
| `InfoEditView.xaml(.cs)` | Slide-in info/help overlay shown over Home; forwards to `HomeVM`'s InfoEdit command. Pure code-behind. |
| `ColorPickerWindow.xaml(.cs)` | RGB color-picker dialog for team / default pick-ban colors. Code-behind validates 0-255 input and writes the chosen color back to the `TeamConfigViewModel` or `PickBanConfigViewModel`, then triggers a team-color refresh. |

Notes:
- "Pre Game" is the renamed champion-select page; the type names still use `PickBan` throughout.
- Several pages expose a collapsible detail pane (`OpenContent`) initialized hidden in code-behind and animated open by behaviors/the navigation service.
