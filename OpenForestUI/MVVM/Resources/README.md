> 🌐 **English** ・ [日本語](README-ja.md)

# Shared resource dictionaries (design tokens)

XAML resource dictionaries that hold the menu's shared design values. `Tokens.xaml` is merged **first**
in [`../../App.xaml`](../../App.xaml) so every theme and view can reference these keys.

## Contents

- **`Tokens.xaml`** — The single source of truth for the control center's colors and key dimensions.
  Defines `Color.*` values and matching `Brush.*` `SolidColorBrush`es (titlebar/sidebar/content
  backgrounds, accents, borders, text) plus `Dim.*` doubles (shell width/height, titlebar height,
  sidebar width, content width). Values were lifted from literals previously scattered inline across
  `MainWindow.xaml` and the theme dictionaries; the migration re-points hard-coded literals onto these
  keys incrementally.
- **`ColorStyles.xaml`** — `SolidColorBrush` palette for the `ToggleSwitch` control's visual states
  (static / mouse-over / pressed / checked / disabled, with on-state variants). Consumed by
  [`../Theme/GenericToggle.xaml`](../Theme/GenericToggle.xaml).
