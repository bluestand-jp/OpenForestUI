> 🌐 **English** ・ [日本語](README-ja.md)

# App controllers (game lifecycle, data ingestion, broadcast)

The orchestration layer of the OpenForestUI desktop app. These singletons watch the League Client and game process, pull data from every source (LCU, the spectator Live Client Data API, the Replay API, the Farsight memory reader, and the Python OCR sidecar), build the overlay state, and push it to the browser-source overlays over the embedded WebSocket server. Most are constructed and wired together by `BroadcastController` during startup.

## Lifecycle model

- **`BroadcastController`** is the root. Its constructor runs `EarlyInit` -> (DataDragon load) -> `Init` (build every controller, the HTTP/WS server, and the tick timer) -> `PostInit` (open the main window via DI, start ticking). It owns the `ToTick` list and a `~2 tps` timer (`TickRate`) that calls `DoTick()` on every registered `ITickable`. Holds the `[Flags] LeagueState` (Connected / ChampSelect / InProgress / PostGame) and `Instance` singleton accessors for all other controllers.
- **`ITickable`** is the one-method interface (`DoTick()`) every ticked controller implements.
- Champion-select and ingame are turned on/off through `AppStateController.Enable/Disable*`, which subscribes the relevant controller to the LCU/process events.

## Contents

| File | Role |
|------|------|
| [`BroadcastController.cs`](BroadcastController.cs) | App root: startup sequencing, controller wiring, the tick timer + `ToTick` list, `LeagueState` flags. |
| [`ITickable.cs`](ITickable.cs) | Interface (`DoTick()`) for anything driven by the central tick loop. |
| [`AppStateController.cs`](AppStateController.cs) | Connects to the LCU (`LeagueClientApi`), subscribes to gameflow / champ-select events, caches summoners, loads Farsight offsets for the local patch, and reconciles the title-bar connection-status chip each tick (Mocking / Disconnected / Connected / LCU). |
| [`ConfigController.cs`](ConfigController.cs) | Loads/saves all JSON configs (`Component`, `PickBan`, `Ingame`, `Farsight`), holds the static config singletons, and runs a `ConfigWatcher` (FileSystemWatcher) per file for hot-reload. |
| [`PickBanController.cs`](PickBanController.cs) | Champion-select (Pre Game) driver: ticks the LCU champ-select timer, converts LCU session state, and broadcasts pick/ban state + a heartbeat to the `pickban` overlay. |
| [`IngameController.cs`](IngameController.cs) | The big one. Per-tick ingame pipeline: pulls game/player/event data, detects playback mode (Spectator / Replay / Live), derives objective spawn clocks, handles replay-seek rollback, injects OCR gold/CS/objectives, samples the gold graph, builds the scoreboard, and broadcasts the `GameHeartbeat`. Also defines `CurrentSettings` (the runtime overlay-feature gates). |
| [`OcrGoldController.cs`](OcrGoldController.cs) | Launches and reads the Python OCR sidecar (`ocr-poc/goldcap.py --live`), parses its tri-state JSON (Known/Stale/Unknown), and injects gated exact gold / CS / objective counts / replay tower counts into the `Team` objects. Backs the `/debug-ocr` diagnostics. |
| [`ReplayAPIController.cs`](ReplayAPIController.cs) | Talks to the local Replay API (`https://127.0.0.1:2999/replay/render`) to toggle the ingame HUD scoreboard for clean rendering; optionally triggers `GameInputController.InitUI`. |
| [`GameInputController.cs`](GameInputController.cs) | Sends synthetic key inputs (via `InputUtils`) to the League window to initialise the observer HUD on game start (gated behind `Replay.UseAutoInitUI`). |
| [`MockController.cs`](MockController.cs) | Dev/preview helper. Broadcasts a canned `GameHeartbeat` built from the `overlay-harness` fixture so the overlay can be previewed without a live game; overlays live team Details and mirrors the Ingame > Teams `ShouldSerialize*` feature gates. Takes priority over the live feed while enabled. |

## Notes

- This fork defaults the memory reader **off** (`UseMemoryReader = false`) for Vanguard compatibility; `FarsightController.ShouldRun` gates all memory-reader paths. The spectator API + OCR sidecar are the primary data sources.
- `Broadcast(...)` in `IngameController` is the single funnel for all live overlay events and stays silent while Mock is enabled, so the live and mock feeds never interleave.
