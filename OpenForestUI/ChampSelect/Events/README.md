> 🌐 **English** ・ [日本語](README-ja.md)

# Champ-select WebSocket events (outbound)

The event payloads OpenForestUI pushes to the pickban browser-source overlay over `ws://localhost:9001/api`. Each type extends `OpenForestUI.Common.Events.LeagueEvent` and sets a string `eventType` that the overlay's JS dispatches on. They are emitted by `StateInfo/State` handlers and `Common/Controllers/PickBanController`, then serialized and sent via the embedded socket server.

## Contents

| File | `eventType` | Role |
| --- | --- | --- |
| `NewState.cs` | `newState` | Full draft snapshot — carries the current `StateData` (teams, timer, phase). Sent on every state/timer update. |
| `NewAction.cs` | `newAction` | The slot that just became active — carries a `CurrentAction` (state `pick`/`ban`/`none`, team, slot index). Sent when the active action changes. |
| `ChampSelectStart.cs` | `champSelectStart` | Signals draft has begun. |
| `ChampSelectEnd.cs` | `champSelectEnd` | Signals draft has ended. |
| `Heartbeat.cs` | `heartbeat` | Periodic (every 10 s) keepalive that also delivers the current `PickBanConfig` to the overlay. |
