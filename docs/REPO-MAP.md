> 🌐 **English** ・ [日本語](REPO-MAP-ja.md)

# Repository Map

OpenForestUI is a public, MIT-licensed hub of League of Legends stream overlays for
tournament broadcasts (Maintainer: Negi - BlueStand). A .NET 6 WPF desktop app
(`OpenForestUI.exe`) acts as the control center: it ingests live game data (LCU API,
spectator Live Client Data API on port 2999, the in-process **Farsight** memory reader,
the Replay API, and a Python OCR sidecar) and serves two browser-source overlays
(**ingame** and **pickban**) over an embedded HTTP + WebSocket server.

This file is the master index of **every** directory in the repository. Each entry links
to that directory's own `README.md` (links are relative to `docs/`). The areas below are:
the WPF app (`OpenForestUI/`), the shared libraries (`OpenForestUI.Common/`,
`OpenForestUI.Farsight/`), the vendored LCU client (`LCUSharp/`), the overlays
(`Overlays/`), this documentation set (`docs/`), and the OCR sidecar (`ocr-poc/`).

---

## OpenForestUI/ — WPF control-center app

The .NET 6 WPF desktop app (`OpenForestUI.exe`): DI composition root, MVVM dashboard
shell, and host for data ingest + the overlay server.

| Directory | Purpose |
|---|---|
| [`OpenForestUI/`](../OpenForestUI/README.md) | The WPF control-center app: DI composition root, MVVM dashboard shell, data ingest + overlay server host |
| [`OpenForestUI/Assets/`](../OpenForestUI/Assets/README.md) | Branding assets (icons, in-app images, brand font) referenced from XAML and bundled by the csproj |
| [`OpenForestUI/Assets/Fonts/`](../OpenForestUI/Assets/Fonts/README.md) | The Venus Rising brand typeface (.otf), embedded as a WPF Resource for the wordmark |
| [`OpenForestUI/Assets/Icons/`](../OpenForestUI/Assets/Icons/README.md) | .ico window/executable icons (current OpenForestUI.ico, legacy BlueEssence.ico) |
| [`OpenForestUI/Assets/Images/`](../OpenForestUI/Assets/Images/README.md) | PNG bitmaps for the dashboard UI (logo, expander glyph, team-logo placeholder, legacy BE marks) |
| [`OpenForestUI/Http/`](../OpenForestUI/Http/README.md) | EmbedIO HTTP + WebSocket server (`ws://localhost:9001/api`, `/frontend`) feeding the overlays |
| [`OpenForestUI/OperatingSystem/`](../OpenForestUI/OperatingSystem/README.md) | Win32 input synthesis, WMI process watching, and small enum/color/messagebox helpers |
| [`OpenForestUI/MVVM/`](../OpenForestUI/MVVM/README.md) | Root of the WPF presentation layer; indexes all MVVM subdirectories |
| [`OpenForestUI/MVVM/Behaviors/`](../OpenForestUI/MVVM/Behaviors/README.md) | Attached behaviors replacing code-behind (WindowDragBehavior for the borderless title bar) |
| [`OpenForestUI/MVVM/Controls/`](../OpenForestUI/MVVM/Controls/README.md) | Custom templated controls; the WinUI-style ToggleSwitch |
| [`OpenForestUI/MVVM/Converters/`](../OpenForestUI/MVVM/Converters/README.md) | IValueConverters for XAML bindings (enum/bool, bool/visibility, bool/color, string/image, toggle offset) |
| [`OpenForestUI/MVVM/Core/`](../OpenForestUI/MVVM/Core/README.md) | MVVM primitives: ObservableObject shim, RelayCommand, dependency-object helpers, preset Bindings |
| [`OpenForestUI/MVVM/Core/Services/`](../OpenForestUI/MVVM/Core/Services/README.md) | DI services injected into view-models: navigation, config, app state, window control |
| [`OpenForestUI/MVVM/DragDrop/`](../OpenForestUI/MVVM/DragDrop/README.md) | Vendored Josh Smith / Dan Crevier ListView reorder helpers (Ingame roster) |
| [`OpenForestUI/MVVM/Resources/`](../OpenForestUI/MVVM/Resources/README.md) | Tokens.xaml design tokens (merged first) and ColorStyles.xaml toggle palette |
| [`OpenForestUI/MVVM/Theme/`](../OpenForestUI/MVVM/Theme/README.md) | Per-control Style/ControlTemplate dictionaries merged in App.xaml |
| [`OpenForestUI/MVVM/View/`](../OpenForestUI/MVVM/View/README.md) | WPF XAML pages/windows for the dashboard shell and per-feature config |
| [`OpenForestUI/MVVM/ViewModel/`](../OpenForestUI/MVVM/ViewModel/README.md) | Bindable view-models behind the views, wired via DI + INavigationService |
| [`OpenForestUI/Common/`](../OpenForestUI/Common/README.md) | App orchestration heart: controllers, config/data layer, overlay event base types |
| [`OpenForestUI/Common/Controllers/`](../OpenForestUI/Common/Controllers/README.md) | Singletons sequencing startup, client/game watching, data ingest, and overlay broadcast |
| [`OpenForestUI/Common/Data/`](../OpenForestUI/Common/Data/README.md) | JSON configuration models/loader and the Data Dragon asset provider |
| [`OpenForestUI/Common/Data/Config/`](../OpenForestUI/Common/Data/Config/README.md) | Strongly-typed Config/*.json models plus JSONConfigProvider (read/migrate/write) |
| [`OpenForestUI/Common/Data/Provider/`](../OpenForestUI/Common/Data/Provider/README.md) | DataDragon singleton resolving game version and caching champion/item/spell assets |
| [`OpenForestUI/Common/Events/`](../OpenForestUI/Common/Events/README.md) | Abstract LeagueEvent / OverlayConfig base types carrying the eventType discriminator |
| [`OpenForestUI/ChampSelect/`](../OpenForestUI/ChampSelect/README.md) | Champ-select pipeline: ingests LCU draft, normalizes it, pushes events to the pickban overlay |
| [`OpenForestUI/ChampSelect/Data/`](../OpenForestUI/ChampSelect/Data/README.md) | Data layer: raw LCU input, normalized overlay DTOs, persisted pickban config |
| [`OpenForestUI/ChampSelect/Data/Config/`](../OpenForestUI/ChampSelect/Data/Config/README.md) | Persisted overlay config: team names/scores/colors, toggles, broadcast top-bar metadata |
| [`OpenForestUI/ChampSelect/Data/DTO/`](../OpenForestUI/ChampSelect/Data/DTO/README.md) | Cleaned pick/ban/team/version models the pickban overlay consumes |
| [`OpenForestUI/ChampSelect/Data/LCU/`](../OpenForestUI/ChampSelect/Data/LCU/README.md) | Raw C# models matching the League Client champ-select session JSON |
| [`OpenForestUI/ChampSelect/Events/`](../OpenForestUI/ChampSelect/Events/README.md) | LeagueEvent payloads (newState, newAction, start, end, heartbeat) to the pickban overlay |
| [`OpenForestUI/ChampSelect/StateInfo/`](../OpenForestUI/ChampSelect/StateInfo/README.md) | Singleton draft state store, LCU→overlay converter, active-slot logic |
| [`OpenForestUI/Ingame/`](../OpenForestUI/Ingame/README.md) | Ingame overlay backend: ingests local Riot data each tick, pushes frontend payload |
| [`OpenForestUI/Ingame/Data/`](../OpenForestUI/Ingame/Data/README.md) | Index of all ingame data types: providers, Riot/Replay DTOs, hub model, frontend payloads, config |
| [`OpenForestUI/Ingame/Data/Config/`](../OpenForestUI/Ingame/Data/Config/README.md) | Persisted config for overlay layout (IngameConfig) and memory-reader offsets (FarsightConfig) |
| [`OpenForestUI/Ingame/Data/Frontend/`](../OpenForestUI/Ingame/Data/Frontend/README.md) | Broadcast-facing DTOs serialized over WS, gated by per-feature ShouldSerialize* checks |
| [`OpenForestUI/Ingame/Data/Hub/`](../OpenForestUI/Ingame/Data/Hub/README.md) | Broadcast aggregate model (teams, players, inhibitors, gold) between raw DTOs and frontend |
| [`OpenForestUI/Ingame/Data/Hub/Objectives/`](../OpenForestUI/Ingame/Data/Hub/Objectives/README.md) | Backend accumulator + frontend payload for Baron/Dragon power-play and spawn panels |
| [`OpenForestUI/Ingame/Data/Provider/`](../OpenForestUI/Ingame/Data/Provider/README.md) | Local Riot HTTP clients for Live Client Data and Replay APIs (port 2999) + objective event args |
| [`OpenForestUI/Ingame/Data/RIOT/`](../OpenForestUI/Ingame/Data/RIOT/README.md) | Plain C# DTOs mirroring the spectator `/liveclientdata/*` JSON |
| [`OpenForestUI/Ingame/Data/Replay/`](../OpenForestUI/Ingame/Data/Replay/README.md) | DTOs for the in-client Replay API: playback clock, camera/render state, HUD toggles, keyframes |
| [`OpenForestUI/Ingame/Events/`](../OpenForestUI/Ingame/Events/README.md) | LeagueEvent-derived DTOs to the ingame overlay (Heartbeat + pop-ups) and the RiotEvent model |
| [`OpenForestUI/Ingame/State/`](../OpenForestUI/Ingame/State/README.md) | Per-game state engine, toggle-gated JSON snapshot (StateData), and ObjectiveSpawnClock |

## OpenForestUI.Common/ — shared low-level library

net6.0 class library of cross-cutting code (logging, HTTP/REST, JSON converters, utils,
data models) referenced by the rest of the solution.

| Directory | Purpose |
|---|---|
| [`OpenForestUI.Common/`](../OpenForestUI.Common/README.md) | Shared low-level library: logging, HTTP/REST, JSON converters, utils, data models |
| [`OpenForestUI.Common/Data/`](../OpenForestUI.Common/Data/README.md) | Index of shared data classes (app-facing DTOs + Riot/Community Dragon static types) |
| [`OpenForestUI.Common/Data/DTO/`](../OpenForestUI.Common/Data/DTO/README.md) | Champion & summoner-spell DTOs pairing CDragon source types with slim overlay shapes |
| [`OpenForestUI.Common/Data/RIOT/`](../OpenForestUI.Common/Data/RIOT/README.md) | Riot fixed tables: XP→level curve and item data/gold cost |
| [`OpenForestUI.Common/Http/`](../OpenForestUI.Common/Http/README.md) | REST client, file downloader, text-fetch helpers (several adapted from GoldDiff, MIT) |
| [`OpenForestUI.Common/Utils/`](../OpenForestUI.Common/Utils/README.md) | Generic helpers: byte-buffer extensions, JSON converters, circular buffer, StringVersion |

## OpenForestUI.Farsight/ — memory reader library

In-process memory reader that walks League's ObjectManager for exact gold/XP/positions the
spectator API omits; opt-in and off by default for Vanguard compatibility.

| Directory | Purpose |
|---|---|
| [`OpenForestUI.Farsight/`](../OpenForestUI.Farsight/README.md) | Memory reader walking the ObjectManager for data the spectator API omits (opt-in) |
| [`OpenForestUI.Farsight/Object/`](../OpenForestUI.Farsight/Object/README.md) | The GameObject class plus its patch-specific per-unit field offset table from Farsight.json |

## LCUSharp/ — League Client (LCU) API client (vendored)

Vendored MIT .NET 6 library that connects to the local LeagueClientUx LCU API (HTTP +
WebSocket); OpenForestUI's data source for the champion-select/pick-ban overlay.

| Directory | Purpose |
|---|---|
| [`LCUSharp/`](../LCUSharp/README.md) | Vendored LCU API client (HTTP + WebSocket) — champ-select data source |
| [`LCUSharp/Http/`](../LCUSharp/Http/README.md) | Authenticated HttpClient plumbing + JSON request/response handlers over `https://127.0.0.1:<port>/` |
| [`LCUSharp/Http/Endpoints/`](../LCUSharp/Http/Endpoints/README.md) | EndpointBase and strongly-typed wrappers over specific League Client REST routes |
| [`LCUSharp/Http/Endpoints/ProcessControl/`](../LCUSharp/Http/Endpoints/ProcessControl/README.md) | Wraps `process-control/v1/process/*`: quit, restart, restart-to-repair/-update |
| [`LCUSharp/Http/Endpoints/RiotClient/`](../LCUSharp/Http/Endpoints/RiotClient/README.md) | Wraps `riotclient/*`: show/minimize/flash/kill/launch the UX and get/set zoom scale |
| [`LCUSharp/Utility/`](../LCUSharp/Utility/README.md) | Locates the LeagueClientUx process and parses the lockfile for (port, token) credentials |
| [`LCUSharp/Websocket/`](../LCUSharp/Websocket/README.md) | WAMP-style WebSocket subscription to live client events, dispatched to per-URI subscribers |

## Overlays/ — browser-source stream overlays (frontends)

The two web overlays the desktop app serves to the broadcast client over HTTP `/frontend`
+ WebSocket `/api`.

| Directory | Purpose |
|---|---|
| [`Overlays/`](../Overlays/README.md) | Index of the two web overlays (ingame, pickban) |
| [`Overlays/ingame/`](../Overlays/ingame/readme.md) | Parcel-bundled Phaser WebGL ingame overlay: scoreboard, timers, graph, pop-ups |
| [`Overlays/ingame/src/`](../Overlays/ingame/src/README.md) | Overlay TypeScript root: main.ts boot, variables/config, scenes/visual/data/util/convert subtrees |
| [`Overlays/ingame/src/convert/`](../Overlays/ingame/src/convert/README.md) | WindowUtils.GetQueryVariable for reading the `?backend=` host param at boot |
| [`Overlays/ingame/src/data/`](../Overlays/ingame/src/data/README.md) | TypeScript mirrors of the per-heartbeat JSON the visuals consume |
| [`Overlays/ingame/src/data/config/`](../Overlays/ingame/src/data/config/README.md) | OverlayConfig interface set: per-element layout/fonts/toggles, incl. PrmScore/GoldGraph flags |
| [`Overlays/ingame/src/scenes/`](../Overlays/ingame/src/scenes/README.md) | IngameScene: single Phaser scene owning the WS connection and fanning state to visuals |
| [`Overlays/ingame/src/util/`](../Overlays/ingame/src/util/README.md) | Dependency-free helpers: gold/text formatting, font loading, RGBA, Vector2, Queue, Dictionary |
| [`Overlays/ingame/src/visual/`](../Overlays/ingame/src/visual/README.md) | One class per on-screen element over a shared VisualElement base |
| [`Overlays/ingame/public/`](../Overlays/ingame/public/README.md) | Root of `/frontend`-served static assets loaded by the Phaser overlay |
| [`Overlays/ingame/public/backgrounds/`](../Overlays/ingame/public/backgrounds/README.md) | PNG/MP4 panel backings for scoreboard, graph, inhibitor, info page, objective bars |
| [`Overlays/ingame/public/images/`](../Overlays/ingame/public/images/README.md) | General objective/tower icons plus dragons, lanes, broadcast-bar and popup subtrees |
| [`Overlays/ingame/public/images/dragons/`](../Overlays/ingame/public/images/dragons/README.md) | Per-elemental-dragon icons split into scoreboard and timer variants |
| [`Overlays/ingame/public/images/dragons/scoreboard/`](../Overlays/ingame/public/images/dragons/scoreboard/README.md) | Per-dragon `*Large.png` icons shown on the scoreboard |
| [`Overlays/ingame/public/images/dragons/timers/`](../Overlays/ingame/public/images/dragons/timers/README.md) | Per-dragon `*Timer.png` icons shown on the respawn timer |
| [`Overlays/ingame/public/images/lanes/`](../Overlays/ingame/public/images/lanes/README.md) | top/mid/bot lane icon SVGs |
| [`Overlays/ingame/public/images/prm/`](../Overlays/ingame/public/images/prm/README.md) | `prm_*` objective icons for the broadcast top bar/scoreboard |
| [`Overlays/ingame/public/images/scoreboardPopUps/`](../Overlays/ingame/public/images/scoreboardPopUps/README.md) | Spawn/kill/soul banner art for major objectives, used by ObjectivePopUpVisual |
| [`Overlays/ingame/public/images/scoreboardPopUps/Baron/`](../Overlays/ingame/public/images/scoreboardPopUps/Baron/README.md) | baronSpawn/baronKill popup banner stills and loops |
| [`Overlays/ingame/public/images/scoreboardPopUps/Dragon/`](../Overlays/ingame/public/images/scoreboardPopUps/Dragon/README.md) | Per-elemental-dragon spawn/kill/soul popup banners, one folder per type |
| [`Overlays/ingame/public/images/scoreboardPopUps/Dragon/Chemtech/`](../Overlays/ingame/public/images/scoreboardPopUps/Dragon/Chemtech/README.md) | chemtech spawn/kill/soul popup banners (PNG+MP4) |
| [`Overlays/ingame/public/images/scoreboardPopUps/Dragon/Cloud/`](../Overlays/ingame/public/images/scoreboardPopUps/Dragon/Cloud/README.md) | cloud spawn/kill/soul popup banners (PNG+MP4) |
| [`Overlays/ingame/public/images/scoreboardPopUps/Dragon/Elder/`](../Overlays/ingame/public/images/scoreboardPopUps/Dragon/Elder/README.md) | elder spawn/kill popup banners (no soul variant) |
| [`Overlays/ingame/public/images/scoreboardPopUps/Dragon/Fire/`](../Overlays/ingame/public/images/scoreboardPopUps/Dragon/Fire/README.md) | fire (infernal) spawn/kill/soul popup banners (PNG+MP4) |
| [`Overlays/ingame/public/images/scoreboardPopUps/Dragon/Hextech/`](../Overlays/ingame/public/images/scoreboardPopUps/Dragon/Hextech/README.md) | hextech spawn/kill/soul popup banners (PNG+MP4) |
| [`Overlays/ingame/public/images/scoreboardPopUps/Dragon/Mountain/`](../Overlays/ingame/public/images/scoreboardPopUps/Dragon/Mountain/README.md) | mountain spawn/kill/soul popup banners (PNG+MP4) |
| [`Overlays/ingame/public/images/scoreboardPopUps/Dragon/Ocean/`](../Overlays/ingame/public/images/scoreboardPopUps/Dragon/Ocean/README.md) | ocean spawn/kill/soul popup banners (PNG+MP4) |
| [`Overlays/ingame/public/images/scoreboardPopUps/Herald/`](../Overlays/ingame/public/images/scoreboardPopUps/Herald/README.md) | heraldSpawn/heraldKill popup banner stills and loops |
| [`Overlays/ingame/public/masks/`](../Overlays/ingame/public/masks/README.md) | Mask textures clipping champ covers, item text, scoreboard, graph, info page, popups |
| [`Overlays/pickban/`](../Overlays/pickban/README.md) | React/CRA pick & ban overlay (forked from RCVolus lol-pick-ban-ui) |
| [`Overlays/pickban/config/`](../Overlays/pickban/config/README.md) | Ejected CRA webpack/dev-server/babel/env/path configuration |
| [`Overlays/pickban/config/jest/`](../Overlays/pickban/config/jest/README.md) | Custom Jest transformers making CSS and binary-asset imports inert in tests |
| [`Overlays/pickban/public/`](../Overlays/pickban/public/README.md) | HTML shell, robots.txt, and frontend-lib.js (the Window.PB WebSocket client bridge) |
| [`Overlays/pickban/scripts/`](../Overlays/pickban/scripts/README.md) | Ejected start/build/test scripts invoking webpack and jest |
| [`Overlays/pickban/src/`](../Overlays/pickban/src/README.md) | React app: boots React, subscribes to backend WS state, renders the europe draft layout |
| [`Overlays/pickban/src/assets/`](../Overlays/pickban/src/assets/README.md) | Center logo plus placeholder splash/ban SVGs used before live champion art arrives |
| [`Overlays/pickban/src/assets/fonts/`](../Overlays/pickban/src/assets/fonts/README.md) | TrueType fonts (Rawline, Raleway, Amarello) declared as @font-face families |
| [`Overlays/pickban/src/europe/`](../Overlays/pickban/src/europe/README.md) | React components (Overlay/Pick/Ban) rendering the European-style draft layout |
| [`Overlays/pickban/src/europe/style/`](../Overlays/pickban/src/europe/style/README.md) | LESS CSS-module stylesheets and draft-reveal animations |

## docs/ — project documentation & design specs

Index of OpenForestUI's reference docs, build specs, and design notes (API availability,
offsets, overlay specs).

| Directory | Purpose |
|---|---|
| [`docs/`](README.md) | Index of reference docs, build specs, and design notes |
| [`docs/api/`](api/README.md) | Definitive endpoint-by-endpoint reference for the LoL local API (port 2999) |
| [`docs/data/`](data/README.md) | Archived game-data tables (currently the per-patch Farsight memory-offset archive) |
| [`docs/data/offsets/`](data/offsets/README.md) | Catalogue of `Offsets-<patch>.json` (11.9→14.6) used by the optional memory reader |
| [`docs/feature-completion/`](feature-completion/README.md) | Design for making every Ingame tile work without memory reading |
| [`docs/lck-scoreboard/`](lck-scoreboard/README.md) | Spec + reference image for the comparison scoreboard bottom visual |
| [`docs/prm-overlay/`](prm-overlay/README.md) | Spec, reference frame, and extracted counter icons for the broadcast top/bottom bars |

## ocr-poc/ — HUD OCR sidecar

Python OCR sidecar + reference pipeline that reads values the spectator API rounds or
omits; `goldcap.py` is the live sidecar.

| Directory | Purpose |
|---|---|
| [`ocr-poc/`](../ocr-poc/README.md) | Python OCR sidecar reading exact CS/gold/objective counts off the HUD |
| [`ocr-poc/overlay-harness/`](../ocr-poc/overlay-harness/README.md) | Node harness feeding a mock WS state to the ingame overlay and screenshotting it headlessly |
