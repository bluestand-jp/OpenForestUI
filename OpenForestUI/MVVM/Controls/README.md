> 🌐 **English** ・ [日本語](README-ja.md)

# Custom controls

Custom templated WPF controls used by the control-center views.

## Contents

- **`ToggleSwitch.cs`** — A WinUI-style on/off switch derived from `ToggleButton`. Exposes dependency
  properties for header placement/alignment/padding, checked/unchecked state text, the graphical
  switch width and padding, per-state brushes (checked/unchecked background, foreground, border), and
  a `SharedSizeGroupName` for column alignment. Layout is driven via the `VisualStateManager`
  (header/switch placement states) and template part bindings set up in `OnApplyTemplate`. Its default
  template lives in [`../Theme/GenericToggle.xaml`](../Theme/GenericToggle.xaml), which also wires the
  thumb offset through [`ToggleSwitchOffsetConverter`](../Converters/ToggleSwitchOffsetConverter.cs).
