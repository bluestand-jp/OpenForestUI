> 🌐 **English** ・ [日本語](README-ja.md)

# Control themes (styles & templates)

Per-control `Style` / `ControlTemplate` resource dictionaries that give the control center its custom
borderless, dark Fluent look. All of these are merged in [`../../App.xaml`](../../App.xaml) and reference
the shared design tokens in [`../Resources/Tokens.xaml`](../Resources/Tokens.xaml). Styles are keyed
(e.g. `x:Key="MenuButtonTheme"`) and applied explicitly via `Style="{StaticResource ...}"`.

## Contents

| File | Styles |
| --- | --- |
| `MenuButtonTheme.xaml` | `RadioButton` style (`MenuButtonTheme`) for the sidebar navigation items. |
| `ConnectionStatusTheme.xaml` | `Button` styles for the title-bar connection-status pill (`ConnectionStatusTheme`) and the saved-teams `AddTheme` button. |
| `CloseButtonTheme.xaml` | Title-bar close `Button` style. |
| `MinimizeButtonTheme.xaml` | Title-bar minimize `Button` style. |
| `StartupButtonTheme.xaml` | `Button` style used on the startup/splash window. |
| `ColorSelectButtonTheme.xaml` | `Button` styles (`ColorSelectTheme`, `ColorSelectLeftTheme`) for the color-picker swatches. |
| `ComboBoxTheme.xaml` | `ComboBox` template and its supporting brushes. |
| `TextBoxTheme.xaml` | `TextBox` styles `InstantUpdateTextBox` / `LostFocusTextBox` (pairing with the `InstantBinding`/`LostFocusBinding` from [`../Core/InstantBinding.cs`](../Core/InstantBinding.cs)). |
| `CollapsableStackPanelTheme.xaml` | `StackPanel` style (`Expanding`) for collapsible/expanding sections. |
| `GenericToggle.xaml` | Default `ControlTemplate` and `Style` for the custom [`ToggleSwitch`](../Controls/ToggleSwitch.cs); wires the thumb offset via [`ToggleSwitchOffsetConverter`](../Converters/ToggleSwitchOffsetConverter.cs) and the state brushes from [`../Resources/ColorStyles.xaml`](../Resources/ColorStyles.xaml). |
