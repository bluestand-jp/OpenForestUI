> 🌐 **English** ・ [日本語](README-ja.md)

# Overlay data models (backend state DTOs)

TypeScript mirrors of the JSON the OpenForestUI backend pushes over the WebSocket.
`StateData` is the per-`GameHeartbeat` payload; the visual elements read these typed
objects rather than raw JSON. Each class is constructed from the parsed `message`,
mapping fields straight across (with light coercion/defaults).

## Contents

| File | Role |
| --- | --- |
| [`stateData.ts`](stateData.ts) | Top-level heartbeat payload: dragon/baron objectives, next-spawn objectives, game time/pause, team gold, gold-graph series, inhibitors, scoreboard, info page, team colors. |
| [`scoreboardConfig.ts`](scoreboardConfig.ts) | Scoreboard state: blue/red `FrontEndTeam`, `GameTime`, series game count, tournament name, and the per-player `PlayerScoreboardEntry[]`. |
| [`frontEndTeam.ts`](frontEndTeam.ts) | One team's display stats: name/icon/score, kills/towers/gold, typed dragon list, plus broadcast top-bar extras (void grubs, baron/dragon counts, inhibitors, plates, region/seed/flag). |
| [`playerScoreboardEntry.ts`](playerScoreboardEntry.ts) | One player's row for the bottom comparison scoreboard: team/position/name, champion + spell keys, level, KDA, CS, gold, item IDs. |
| [`frontEndObjective.ts`](frontEndObjective.ts) | An objective's display state: underlying `Objective`, duration-remaining string, rounded gold difference, spawn timer. |
| [`objective.ts`](objective.ts) | Core objective fact: cooldown, alive flag, times taken, last taker, type. |
| [`upcomingObjective.ts`](upcomingObjective.ts) | A pending objective: element name + spawn timer (drives spawn-timer widgets). |
| [`inhibitor.ts`](inhibitor.ts) | `Inhibitor` (id/key/time-left) and `InhibitorInfo` (the list + a map `Location`). |
| [`goldEntry.ts`](goldEntry.ts) | A single `{x: time, y: gold}` point for the gold-diff graph. |
| [`infoSidePage.ts`](infoSidePage.ts) | Info-tab side page: title, `PlayerOrder` enum, and `PlayerInfoTab[]`; null-safe (backend sends `null` when the tab has no data). |
| [`playerInfoTab.ts`](playerInfoTab.ts) | One info-page tab entry: player name, icon path, a `ValueBar`, extra-info strings. |
| [`ValueBar.ts`](ValueBar.ts) | Min/current/max numeric triple for an info-page progress bar. |
| [`RegionMask.ts`](RegionMask.ts) | Pairs a player-slot's Phaser bitmap masks with a per-region animation `Queue`, so pop-ups in the same slot play one at a time. |

## Subdirectories

| Directory | Purpose |
| --- | --- |
| [`config/`](config/) | The `OverlayConfig` interfaces (per-element layout / display settings sent once at connect). |
