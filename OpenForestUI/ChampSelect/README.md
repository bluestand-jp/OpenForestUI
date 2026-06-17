> 🌐 **English** ・ [日本語](README-ja.md)

# Champion-select pipeline (LCU draft -> overlay)

This namespace is OpenForestUI's champion-select feature: it ingests live draft data from the **League Client (LCU) API** over a WebSocket, normalizes it into a stable broadcast model, and pushes pick/ban/timer/state updates to the `pickban` browser-source overlay (a C# integration of RCVolus' `lol-pick-ban-ui`).

The flow is driven by `Common/Controllers/PickBanController.cs`: an LCU WebSocket event arrives as a raw `Session`, `StateInfo/Converter` flattens and converts it into clean `Team`/timer/state values, `StateInfo/State` diffs and stores them in the shared `StateData`, and the result is broadcast to overlays as the `Events` defined here (serialized over `ws://localhost:9001/api`). The controller also runs a per-tick timer refresh (LCU only fires on phase changes) and a 10 s `heartbeat` carrying the current `PickBanConfig`.

## Subdirectories

| Directory | Purpose |
| --- | --- |
| [`Data/`](Data/) | Draft data models: raw LCU input DTOs, normalized overlay DTOs, and config |
| [`Events/`](Events/) | Outbound WebSocket event payloads sent to the pickban overlay |
| [`StateInfo/`](StateInfo/) | Current-draft state store, LCU->overlay converter, and current-action logic |

## How it fits together

1. **In:** LCU `champSelect` session JSON -> `Data/LCU/Session` (and its `Cell`, `Action`, `Timer`).
2. **Convert:** `StateInfo/Converter` flattens action groups and builds normalized `Data/DTO/Team` (`Pick`/`Ban`/`Champion`), the countdown timer, and the phase name.
3. **Store/diff:** `StateInfo/State` updates the singleton `StateData`, firing `StateUpdate` / `NewAction` / start / end handlers.
4. **Out:** wrapped as `Events/*` (`newState`, `newAction`, `champSelectStart`, `champSelectEnd`, `heartbeat`) and sent to the overlay.
