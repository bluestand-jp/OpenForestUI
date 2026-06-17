> 🌐 **English** ・ [日本語](README-ja.md)

# Ingame game-state engine (live state model + serialized overlay snapshot)

This directory holds the in-memory model of a live/replay game and the snapshot that gets serialized to the ingame overlay. `State` is the per-game engine that ingests Live Client Data (`/playerlist`, `/eventdata`), Farsight memory reads, and the OCR sidecar, then maintains team/objective/gold/scoreboard state. `StateData` is the JSON-serializable view of that state shipped to the overlay inside each `HeartbeatEvent` (see [`../Events/`](../Events/)).

## Contents

- **`State.cs`** (`State`) — the core engine, driven each tick by `IngameController.DoTick`. Responsibilities:
  - `UpdateTeams` — builds/updates `blueTeam`/`redTeam` from `/playerlist`; diffs per-player level and inventory (by `(itemID, slot)`) to fire `LevelUp` / `ItemCompleted` events; tracks Baron/Elder buff expiry.
  - `UpdateEvents` — consumes the cumulative `/eventdata` batch, dedups by `EventID`, and credits turret/inhibitor kills, void grubs, and Dragon/Baron/Herald takes (the legacy LiveEvents API on port 34243 was removed in patch 14.1, so `/eventdata` is now the sole objective driver).
  - `ApplyHistoricalBaseline` — reconstructs tower/dragon counts when observing a game already mid-progress, so counters don't start at zero.
  - Gold-diff graph: `RecordGoldDiff` / `TrimGoldDiffHistory` / `GetGoldGraph` maintain a time-series of the blue−red gold difference (lock-guarded; downsampled), used for the side gold graph.
  - `UpdateScoreboard` — projects all of the above into `stateData.scoreboard` (kills, towers, gold, dragons, grubs, inhibs, per-player roster), preferring OCR-exact values when the sidecar has a lock.
  - `ResetState` — clears everything between games; `CreditTurretKill` / `CreditInhibKill` accept both legacy `T1/T2` and current `TOrder/TChaos` id spellings.
- **`StateData.cs`** (`StateData`) — the serialized overlay snapshot: objective timers (`dragon`/`baron`, `nextDragon`/`nextBaron`), `gameTime`/`gamePaused`, blue/red gold, the gold graph, inhibitor info, the `ScoreboardConfig`, the optional info side page, and team colors. `ShouldSerialize*` methods gate each field on the user's ingame feature toggles (`IngameController.CurrentSettings`) so disabled components are omitted from the wire payload (and `infoPage` is dropped when empty to avoid the frontend treating present-but-empty as data).
- **`ObjectiveSpawnClock.cs`** (`ObjectiveSpawnClock`) — derives Dragon/Baron/Herald spawn countdowns from first principles every tick (`remaining = max(0, nextSpawnAt − gameTime)`, where `nextSpawnAt` is the patch first-spawn constant or last-kill + respawn). This stateless recompute replaces a fragile decrement-a-seeded-timer approach and handles mid-game joins and replay rewinds correctly; it also flags Elder (4+ drakes) and emits spawn zero-crossings for the pop-up pipeline.

## Notes

- `State` is `internal`; the overlay only ever sees the `StateData` projection, never the live engine.
- Many comments in `State.cs` reference "Phase N" replay/rewind handling and the no-memory-reader configuration — the engine is built to stay accurate across both live spectating and replay seeking.
