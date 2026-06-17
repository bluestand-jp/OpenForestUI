> 🌐 **English** ・ [日本語](README-ja.md)

# Brand font

Holds the **Venus Rising** brand typeface used for the OpenForestUI wordmark.

## Contents

- **`Venus Rising Rg.otf`** — the wide, all-caps brand font. It is included in `OpenForestUI.csproj`
  as a WPF `Resource` (not just copied), so the pack URI resolves at runtime. Referenced in XAML as
  `FontFamily="pack://application:,,,/Assets/Fonts/#Venus Rising"` — note the family name after `#` is
  **`Venus Rising`** (the typeface's internal name), not the file name.

Consumed by `MVVM/View/MainWindow.xaml`, `StartupWindow.xaml`, and `HomeView.xaml` for the brand
wordmark.
