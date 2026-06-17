> 🌐 **English** ・ [日本語](README-ja.md)

# MVVM layer (WPF control-center UI)

This is the entire WPF presentation layer of the OpenForestUI desktop app (assembly `OpenForestUI.exe`).
The control center is built MVVM-first on CommunityToolkit.Mvvm + Microsoft.Extensions.DependencyInjection,
with an `INavigationService`, design tokens, and a Fluent/WPF-UI dashboard shell. Views bind to view-models;
view-models talk to thin DI services instead of reaching into controllers or concrete windows.

The app is a *control center*, not the overlays themselves — these screens toggle and configure the
browser-source overlays (`ingame`, `pickban`) that the embedded server streams over WebSocket / HTTP.

## Subdirectories

| Directory | Purpose |
| --- | --- |
| [`Core/`](Core/) | MVVM primitives: `ObservableObject`, relay/delegate commands, dependency-object helpers |
| [`Core/Services/`](Core/Services/) | DI services (navigation, config, app state, window) injected into view-models |
| [`View/`](View/) | XAML windows/pages and their code-behind (the shell, Home, Settings, PickBan, Ingame, PostGame) |
| [`ViewModel/`](ViewModel/) | View-models backing each view |
| [`Models/`](Models/) | UI-side model/helper types bound by the view-models |
| [`Converters/`](Converters/) | `IValueConverter`s used in XAML bindings |
| [`Behaviors/`](Behaviors/) | Attached behaviors that replace code-behind event handlers (e.g. window drag) |
| [`Controls/`](Controls/) | Custom WPF controls (the templated `ToggleSwitch`) |
| [`Theme/`](Theme/) | Per-control `Style` / `ControlTemplate` resource dictionaries |
| [`Resources/`](Resources/) | Design tokens (colors, brushes, dimensions) merged first in `App.xaml` |
| [`DragDrop/`](DragDrop/) | Vendored Josh Smith ListView drag-drop helpers (used by the Ingame roster) |

All theme and resource dictionaries are merged in [`App.xaml`](../App.xaml); `Resources/Tokens.xaml`
is merged first so themes and views can reference the shared tokens.
