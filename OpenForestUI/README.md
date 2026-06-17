> 🌐 **English** ・ [日本語](README-ja.md)

# OpenForestUI — WPF control-center app

The `OpenForestUI` project is the desktop control center for OpenForestUI: a WPF (.NET 6,
`net6.0-windows`) application that builds to **`OpenForestUI.exe`**. It is the host process that
ingests live League of Legends game data, runs the embedded overlay web server, and presents the
operator dashboard used during a broadcast.

It is a hard-fork descendant of [LeagueBroadcast](https://github.com/floh22/LeagueBroadcast) (floh22,
MIT). The UI has been modernized to MVVM: [CommunityToolkit.Mvvm], `Microsoft.Extensions.DependencyInjection`
for a DI container, an `INavigationService`, design tokens, and a Fluent dashboard shell built on
[WPF-UI] (pinned to 3.0.5, the last release targeting `net6.0-windows`).

## How it fits together

- **`App.xaml` / `App.xaml.cs`** — application entry point and DI composition root. `OnStartup`
  builds the `ServiceCollection` (nav/config/state/window services + page view-models, all singletons)
  **before** `BroadcastController.Instance` is first touched — an ordering invariant the code calls out
  explicitly. It applies the OpenForestUI green accent (`#5CC59E`) over the WPF-UI Dark theme. `App.xaml`
  merges the WPF-UI Fluent dictionaries plus the project's design tokens and per-control themes, and maps
  view-models to views via `DataTemplate`s.
- **`AssemblyInfo.cs`** — `ThemeInfo` attribute for WPF theme resource resolution.
- **`OpenForestUI.csproj`** — `WinExe`, app icon `Assets/Icons/OpenForestUI.ico`, the Venus Rising
  brand font embedded as a `Resource`. References the three sibling projects (`LCUSharp`,
  `OpenForestUI.Common`, `OpenForestUI.Farsight`) and packages (EmbedIO, WPF-UI, CommunityToolkit.Mvvm,
  System.Management). A post-publish target builds the `ingame`/`pickban` overlays, stages the OCR
  sidecar, and zips a release.

## Subdirectories

| Directory | Purpose |
| --- | --- |
| [`Assets/`](Assets/) | App branding assets (icons, images, brand font) bundled into the build |
| [`ChampSelect/`](ChampSelect/) | Champion-select (pick & ban) data model, state tracking, and LCU integration |
| [`Common/`](Common/) | App-level controllers, config/state plumbing, and shared helpers |
| [`Http/`](Http/) | Embedded EmbedIO web/WebSocket server feeding the browser-source overlays |
| [`Ingame/`](Ingame/) | Ingame overlay data layer (Live Client API, Farsight, OCR) and event DTOs |
| [`MVVM/`](MVVM/) | Views, view-models, converters, themes, and design tokens for the dashboard UI |
| [`OperatingSystem/`](OperatingSystem/) | Win32 input synthesis, process watching, and small extension helpers |
