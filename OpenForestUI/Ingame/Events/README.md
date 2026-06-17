> 🌐 **English** ・ [日本語](README-ja.md)

# Ingame overlay events (WebSocket messages to the ingame overlay)

This directory holds the outbound event types the desktop app pushes to the **ingame** browser-source overlay over WebSocket (`ws://localhost:9001/api`). Each type derives from `OpenForestUI.Common.Events.LeagueEvent`, which carries an `eventType` string discriminator the overlay switches on. The `IngameController` / `State` pipeline constructs these from live game data (Live Client API, Farsight, OCR sidecar) and broadcasts them; the overlay then animates banners, timers, and the scoreboard.

The central message is the periodic `HeartbeatEvent`, which carries the full serialized `StateData` snapshot (see [`../State/`](../State/)). The remaining types are transient one-shot notifications for animated pop-ups (objective taken/spawned, item completed, level-up, game lifecycle).

## Contents

| File | Type(s) | Role |
| --- | --- | --- |
| `RiotEvent.cs` | `RiotEvent`, `EventTypes` | DTO mirroring a Riot `/liveclientdata/eventdata` event (EventID, name, time, killer/victim/assisters, turret/inhib killed, kill streak). `EventTypes` has factory helpers for synthetic Baron/Dragon take/end markers (`EventID == -1`). |
| `RiotEventList.cs` | `RiotEventList` | Thin wrapper holding `List<RiotEvent> Events` — the deserialization shape of an `/eventdata` batch. |
| `Heartbeat.cs` | `HeartbeatEvent` | `eventType = "GameHeartbeat"`. Wraps a `StateData` snapshot; the primary periodic state push to the overlay. |
| `ObjectiveKilled.cs` | `ObjectiveKilled`, `ObjectiveKilledSimple` | `eventType = "ObjectiveKilled"`. Objective name + team that took it; the `RiotEvent`-derived variant also carries game time. Drives objective-taken banners. |
| `ObjectiveSpawn.cs` | `ObjectiveSpawn`, `ObjectiveSpawnSimple` | `eventType = "ObjectiveSpawn"`. Fired when an objective (e.g. Baron) spawns at countdown zero; drives spawn pop-ups. |
| `ItemCompleted.cs` | `ItemCompleted` | `eventType = "ItemCompleted"`. Player id + completed `ItemData`; drives the item-purchase notification. |
| `PlayerLevelUp.cs` | `PlayerLevelUp` | `eventType = "PlayerLevelUp"`. Player id + new level (6/11/16 power spikes). |
| `BuffDespawn.cs` | `BuffDespawn` | `eventType = "BuffDespawn"`. Objective name + team id when a Baron/Elder buff ends. |
| `GameStart.cs` | `GameStart` | `eventType = "GameStart"`. Game-begin lifecycle signal. |
| `GameEnd.cs` | `GameEnd` | `eventType = "GameEnd"`. Game-over lifecycle signal. |
| `GamePause.cs` | `GamePause` | `eventType = "GamePause"`. Carries current game time when play pauses. |
| `GameUnpause.cs` | `GameUnpause` | `eventType = "GameUnpause"`. Carries current game time when play resumes. |
| `IngameOverlay.cs` | `IngameOverlay` | Singleton `OverlayConfig` (`FrontEndType.Ingame`) exposing the active `IngameConfig`; the overlay-config payload, not a transient event. |

## Notes

- `RiotEvent.EventID == -1` denotes a **synthetic** marker generated internally (objective take/end), distinct from real Riot events whose IDs are `>= 0`. `State.cs` relies on this to carry objective markers forward across per-tick `/eventdata` overwrites (see [`../State/`](../State/)).
- The `*Simple` variants exist for cases that send only name/team without the full `RiotEvent` fields.
