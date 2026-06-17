> 🌐 **English** ・ [日本語](README-ja.md)

# Ingame data models

All the data types for the ingame overlay, organized by where the data sits in the pipeline: raw Riot DTOs come in from the local HTTP providers, get aggregated into a broadcast model, and are reshaped into the JSON payloads pushed to the overlay. Configuration models for the overlay and the memory reader live here too.

## Subdirectories

| Directory | Purpose |
| --- | --- |
| [`Provider/`](Provider/) | Local Riot HTTP clients (Live Client Data + Replay API) and their event args. |
| [`RIOT/`](RIOT/) | DTOs mirroring the spectator Live Client Data API JSON. |
| [`Replay/`](Replay/) | DTOs for the in-client Replay API (`/replay/*`) — playback clock, camera, HUD toggles. |
| [`Hub/`](Hub/) | Aggregated game-state model (teams, players, gold, inhibitors, objectives). |
| [`Frontend/`](Frontend/) | Broadcast payload shapes serialized over the WebSocket to the overlay. |
| [`Config/`](Config/) | Persisted JSON config for the ingame overlay (`IngameConfig`) and memory-reader offsets (`FarsightConfig`). |
