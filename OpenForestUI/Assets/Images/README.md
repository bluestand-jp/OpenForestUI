> 🌐 **English** ・ [日本語](README-ja.md)

# In-app images

Small PNG bitmaps used by the WPF dashboard UI: brand logos and a few control glyphs. Several are
copied to the output directory by `OpenForestUI.csproj` because they are loaded by relative path at
runtime.

## Contents

- **`OpenForestUI_icon.png`** — primary in-app logo, shown in the main window brand header
  (`MainWindow.xaml`) and the startup/splash window (`StartupWindow.xaml`).
- **`ArrowsDownWhite.png`** — white chevron/expander glyph used by collapsible sections in
  `IngameView.xaml` and `InfoEditView.xaml`.
- **`LeagueOfLegendsIcon.png`** — default team-logo placeholder; `PickBanView.xaml.cs` copies it into
  the team-icon folder as `Default.png` when no custom logo is set.
- **`BE_icon.png`** / **`BE_icon_white.png`** — legacy "Blue Essence" mark from the upstream lineage.
  `BE_icon.png` is still wired as the NuGet `<PackageIcon>` in the csproj; both are otherwise vestigial.
