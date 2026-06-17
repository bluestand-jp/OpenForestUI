> 🌐 **English** ・ [日本語](README-ja.md)

# Ingame overlay backend

The C# subsystem that powers the **ingame** browser-source overlay. Each tick it pulls data from the local Riot endpoints (spectator Live Client Data API on port 2999, the Replay API, and — when enabled — the Farsight memory reader and OCR sidecar), folds it into an aggregated game state, derives objective spawn timers, and serializes a frontend payload pushed over the WebSocket to the Phaser overlay.

## Subdirectories

| Directory | Purpose |
| --- | --- |
| [`Data/`](Data/) | All ingame data models — providers, Riot/Replay DTOs, the aggregate hub model, frontend payloads, and config. |
| [`Events/`](Events/) | `RiotEvent` and typed `/eventdata` event DTOs (kills, objective takedowns, item-completed, level-up, etc.) plus the `IngameOverlay` config-overlay registration. |
| [`State/`](State/) | The per-tick state machine: `State` (ingests providers → updates the hub model → emits events), `StateData` (the serialized frontend snapshot), and `ObjectiveSpawnClock` (derives Dragon/Baron/Herald countdowns from kill events + patch timing). |
