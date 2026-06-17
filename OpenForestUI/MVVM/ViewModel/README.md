> 🌐 **English** ・ [日本語](README-ja.md)

# Control-center ViewModels (MVVM bindable state)

The presentation-logic layer behind the desktop control center's views in [`../View/`](../View/). Each view-model exposes bindable properties and `ICommand`s; most are thin facades over the app's config/state (`ConfigController.Component.*`, `IngameController.CurrentSettings`) so toggling a control in the UI writes straight through to the persisted broadcast configuration. The modernized graph is constructed by `Microsoft.Extensions.DependencyInjection` and navigated via the `INavigationService`/`AppRoute` (see [`../Core/Services/`](../Core/Services/)); view-models inherit either the CommunityToolkit `ObservableObject` (source-gen `[ObservableProperty]`) or the legacy `OpenForestUI.MVVM.Core.ObservableObject` shim during the staged migration.

## Contents

| File | Role |
| --- | --- |
| `MainViewModel.cs` | Shell view-model: `NavigateCommand` + window-chrome commands, exposes `Nav` (current view/route) and `State` (connection status). Holds a "strangler facade" of `static` `XVM` accessors that resolve the DI singletons so legacy callers and new wiring share one object graph. |
| `HomeViewModel.cs` | Dashboard host; injects the `PickBan`/`Ingame`/`PostGame`/`InfoEdit` child view-models and drives the InfoEdit overlay open/visibility state. Modernized to CommunityToolkit source-gen. |
| `PickBanViewModel.cs` | Champion-select ("Pre Game") page state: `IsActive`/`IsOpen`, open/close commands; ctor seeds the static blue/red `TeamConfigViewModel`s from config. |
| `PickBanConfigViewModel.cs` | Champion-select overlay settings facade (patch, spells/coaches/score toggles, default blue/red colors + brushes, pick-ban delay). Singleton `ChampSelectSettings`. |
| `TeamConfigViewModel.cs` | Per-team config (name/tag/score/coach/color, logo icon path, broadcast region/seed/flag metadata) with static `BlueTeam`/`RedTeam` instances; loads/saves via `JSONConfigProvider` and reflects color changes back to config. |
| `IngameViewModel.cs` | Ingame overlay page state plus the three nested feature-group view-models — `ObjectivesTabViewModel`, `PlayersTabViewModel`, `TeamsTabViewModel` — and the reusable `ControlButtonViewModel` (title/description/enabled toggle tile). Toggling `IsActive` enables/disables the ingame pipeline and resumes the live tick. |
| `IngameTeamsViewModel.cs` | Live roster state: static `BluePlayers`/`RedPlayers` observable collections + change events, and `PlayerViewModel` (name/champion/team/baron flag) with drag-reorder (`OnProcessDrop`) that re-syncs the underlying game-state player order. |
| `PostGameViewModel.cs` | Post-game overlay page state (`IsActive`/`IsOpen` + open/close commands). |
| `SettingsViewModel.cs` | App settings facade: offset-update check, Farsight memory-reader toggle, transient mock-feed toggle, and credits/version strings. |
| `StartupViewModel.cs` | Loading-splash state: status text, load progress, and the update-prompt flag; maps the `LoadStatus` enum stages and Data Dragon download progress onto the progress bar. |
| `ConnectionStatusViewModel.cs` | Color/label model for the titlebar status chip, with static palette instances (Disconnected / Client Loaded / Connected / Mocking / Issue Found) written each tick by `AppStateController`. |
| `ColorPickerViewModel.cs` | RGB <-> `Color`/`SolidColorBrush` binding model behind `ColorPickerWindow`. |
| `InfoEditViewModel.cs` | Thin proxy exposing the Home view-model's InfoEdit button command/visibility to `InfoEditView`. |
