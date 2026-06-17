> 🌐 **English** ・ [日本語](README-ja.md)

# Value converters (XAML bindings)

`IValueConverter` implementations referenced from XAML to adapt view-model values for display.
These are mostly one-way (Convert only); `ConvertBack` throws or no-ops unless noted.

## Contents

| File | Role |
| --- | --- |
| `EnumToBooleanConverter.cs` | Maps an enum to bool by comparing against a `ConverterParameter`, two-way. Binds sidebar `RadioButton.IsChecked` to `INavigationService.CurrentRoute`; `ConvertBack` returns the route only when a button becomes checked (returns `Binding.DoNothing` otherwise). |
| `InvertableBooleanToVisibilityConverter.cs` | Bool → `Visibility`, with a `Normal`/`Inverted` `ConverterParameter` to flip the mapping. |
| `BooleanToColorConverter.cs` | Bool → `SolidColorBrush`. Optional `ConverterParameter` CSV `ColorIfTrue;ColorIfFalse;Opacity` (default `LimeGreen;Transparent;1.0`). |
| `StringToImageSourceConverter.cs` | A URI string → `ImageSource` via `BitmapFrame.Create` (loads on demand, ignores cache); returns `null` on failure. |
| `ToggleSwitchOffsetConverter.cs` | Computes the thumb travel offset from the switch width for the `ToggleSwitch` template; `IsReversed` negates it. Used in [`../Theme/GenericToggle.xaml`](../Theme/GenericToggle.xaml). |
