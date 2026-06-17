> 🌐 **English** ・ [日本語](README-ja.md)

# Window / executable icons

`.ico` icons for the OpenForestUI desktop app — used as the executable icon and the WPF window icons.

## Contents

- **`OpenForestUI.ico`** — current app icon (green "weeds" mark). Set as `<ApplicationIcon>` in
  `OpenForestUI.csproj` (the `.exe` icon) and used as the `Icon` of `MainWindow.xaml` and
  `StartupWindow.xaml`. Copied to the output directory on build.
- **`BlueEssence.ico`** — legacy icon inherited from the upstream lineage; still copied to output by
  the csproj but no longer referenced by the current windows.
