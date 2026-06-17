> 🌐 **English** ・ [日本語](README-ja.md)

# Ingame data providers (local Riot HTTP clients)

The thin HTTP layer between OpenForestUI and the game client's loopback endpoints on port 2999. Each provider owns an `HttpClient` (self-signed cert accepted, 250ms timeout) and deserializes responses into the DTOs under `RIOT/` and `Replay/`. The ingame state machine (`Ingame/State/State.cs`) calls these every tick.

## Contents

- [`IngameDataProvider.cs`](IngameDataProvider.cs) — Live Client Data client. `GetGameData` (`/gamestats`), `GetPlayerData` (`/playerlist`), `GetEventData` (`/eventdata`). Also defines the `PlaybackMode` enum (`Live` / `Spectator` / `Replay`) and `DetectPlaybackMode`, which probes the endpoints to classify the session (replay wins over spectator; live games have no usable path because `/playerlist` only exposes the local player). `IsSpectatorGame` detects an empty `activeplayername` as the spectator signal.
- [`ReplayDataProvider.cs`](ReplayDataProvider.cs) — Replay API client (all-static; a static ctor initializes the `HttpClient`). `IsReplayActive`/`GETGameAsync` (`/replay/game`), `GET/POSTPlaybackAsync` (`/replay/playback`, clock + seek), `GET/POSTRenderAsync` (`/replay/render`, camera + HUD toggles).
- [`ObjectiveTakenArgs.cs`](ObjectiveTakenArgs.cs) — Event args (`Type`, `Team`, `GameTime`) for Dragon/Baron/Herald takedowns fired from the `/eventdata` pipeline and consumed by `IngameController`.
