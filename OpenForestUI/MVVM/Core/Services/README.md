> 🌐 **English** ・ [日本語](README-ja.md)

# DI services (MVVM seams)

The dependency-injected services that view-models consume instead of touching controllers or concrete
windows directly. Each is registered in the app's `Microsoft.Extensions.DependencyInjection` container
and injected via constructor; the interfaces are the testable seams that keep a future out-of-process
(web) menu reachable without changing the view-models.

## Contents

| Interface | Implementation | Role |
| --- | --- | --- |
| `INavigationService.cs` | `NavigationService.cs` | Single source of truth for "which page is showing". Exposes `CurrentView` (bound by a `ContentControl`), `CurrentRoute` (bound by sidebar radio buttons via `EnumToBooleanConverter`), and `NavigateTo(AppRoute)`. Defines the `AppRoute` enum (`Home`, `PickBan`, `Ingame`, `PostGame`, `Settings`). The service injects the page view-models and toggles each page's `IsOpen` so its full view renders. |
| `IConfigService.cs` | `ConfigService.cs` | Thin, testable seam over the static `ConfigController`. Exposes the component "active" toggles (`PickBanActive`, `IngameActive`, `PostGameActive`) the Home page drives, plus `Save()` which persists to `./Config/Component.json`. |
| `IStateService.cs` | `StateService.cs` | Holds live, non-persisted app state the UI observes — currently the LCU/game `ConnectionStatus` the title-bar pill binds to. The setter marshals onto the dispatcher thread so background-thread LCU/game callbacks are safe (closes a latent cross-thread write). Starts `DISCONNECTED`. |
| `IWindowService.cs` | `WindowService.cs` | Window-management actions as bindable commands: `Register(window)` (called once the shell is shown), `Minimize()`, `Close()`. Drag is handled separately by [`WindowDragBehavior`](../../Behaviors/WindowDragBehavior.cs). |

These replace the old patterns where view-models held a concrete `MainWindow` reference and read
`ConfigController.Component.*` directly.
