> 🌐 **English** ・ [日本語](README-ja.md)

# MVVM primitives

Low-level building blocks the view-models and views are built on: the observable base class, command
implementations, and small WPF helpers. The DI services that view-models depend on live in the
[`Services/`](Services/) subdirectory.

## Contents

| File | Role |
| --- | --- |
| `ObservableObject.cs` | Shim that re-points the project's `ObservableObject` base onto `CommunityToolkit.Mvvm.ComponentModel.ObservableObject`, so existing `: ObservableObject` view-models compile unchanged while opting into `[ObservableProperty]`/`[RelayCommand]` source generators. Removed once all view-models reference the toolkit base directly. |
| `RelayCommand.cs` | `ICommand` with `execute`/optional `canExecute` delegates; routes `CanExecuteChanged` through `CommandManager.RequerySuggested`. |
| `DelegateCommand.cs` | Simpler `ICommand` (always-executable) that also carries `Key`/`ModifierKeys`/`MouseAction` gesture metadata. |
| `DependencyObjectExtension.cs` | Helpers: `TryCast<T>` and `FindAncestorOfType` (visual-tree walk). |
| `InstantBinding.cs` | `Binding` subclasses with preset two-way + validation: `InstantBinding` (`UpdateSourceTrigger.PropertyChanged`) and `LostFocusBinding` (`UpdateSourceTrigger.LostFocus`). Used by the text-box themes. |

## Subdirectories

| Directory | Purpose |
| --- | --- |
| [`Services/`](Services/) | DI services injected into view-models (navigation, config, app state, window) |
