# Vanguard-Compatible Feature Completion — Design

> Goal: make the non-functional Ingame features fully work **without memory reading**,
> under the fork's policies: strictly accurate (prefer hiding over approximating),
> Vanguard-compatible (Live Client Data API + HUD OCR only), coder fully customizable.
>
> Audit baseline (2026-06-12): of the 16 Ingame tiles, NOT functional = EXP Tab,
> Gold Tab, Gold Graph; PARTIAL = Spawn Pop Ups (Baron only), Baron Timer.

## Table of Contents
1. [Ground truth — why each feature is broken](#1-ground-truth)
2. [Design 1: ObjectiveSpawnClock — Baron Timer + Spawn Pop Ups](#2-design-1-objectivespawnclock)
3. [Design 2: Gold Graph from OCR team gold](#3-design-2-gold-graph-from-ocr)
4. [Design 3: Gold Tab (per-player gold)](#4-design-3-gold-tab)
5. [Design 4: EXP Tab (and exact replacement tabs)](#5-design-4-exp-tab)
6. [Design 5: FeatureAvailability capability map (cross-cutting)](#6-design-5-featureavailability)
7. [Patch constants & verification plan](#7-patch-constants--verification-plan)
8. [Build order & open decisions](#8-build-order--open-decisions)

---

## 1. Ground truth

Verified against code (file:line), not the earlier audit's summary alone:

| Feature | Root cause |
|---|---|
| **Baron Timer** | `stateData.baron.SpawnTimer` is never seeded at game start (dragon IS: `State.cs:115`); it only becomes non-zero after `OnBaronTaken` sets 420 (`IngameController.cs:487`). Serialization gate `ShouldSerializenextBaron()` → `UseCustomBaronTimer` defaults **false** (`ComponentConfig.cs:93`, comment "non functional for now"). |
| **Timer architecture bug** | Timers are *seeded + tick-decremented* (`IngameController.cs:137-138`), with a rewind-only recompute that has an **inverted sign**: `420 - (lastKill.EventTime - now)` = 420 + elapsed (`IngameController.cs:207`, dragon `:220`). Correct is `420 - (now - lastKill.EventTime)`. |
| **Spawn Pop Ups (Dragon/Herald)** | Spawn detection relied on the memory snapshot's `Dragon_Indicator`; without it only Baron pops (timer zero-crossing, `IngameController.cs:233-235`). |
| **Gold Graph** | `Player.goldHistory` has exactly one writer: the constructor `goldHistory[0]=500` (`Player.cs:67`). Dead data ⇒ `GetGoldGraph()` always hits the `dataPoints<2` flat fallback. |
| **csHistory (adjacent bug)** | Only writer is `LiveEventsDataProvider.cs:383` — the LiveEvents API (port 34243) was removed by Riot in patch 14.1, so it never connects. The rewind path then does `p.scores.creepScore = p.csHistory.Last().Value` (`IngameController.cs:171`) = **resets CS to 0 on every backward seek**. |
| **EXP Tab / Gold Tab** | `PlayerTab.GetEXPTabs/GetGoldTabs` read `p.farsightObject.EXP/.GoldTotal` — Farsight is disabled under Vanguard, `farsightObject` stays null. `/playerlist` exposes neither per-player gold nor XP. |
| **Stale constants** | Baron first spawn coded as 1200s (20:00) / respawn 420s (7:00) — both changed in later seasons. Must be config, not code (§7). |

## 2. Design 1: ObjectiveSpawnClock

**Replace "seeded + decrement + rewind-recompute" with pure derivation every tick.**

```
ObjectiveSpawnClock.Recompute(gameTime, events, dragonsTaken, timings) -> ClockState
  for each objective in {Dragon, Herald, Baron}:
    lastKill = last kill event for objective at EventTime < gameTime
    nextSpawnAt = lastKill == null ? timings.FirstSpawn[obj]
                                   : lastKill.EventTime + timings.Respawn[obj]   // Elder rule below
    remaining   = max(0, nextSpawnAt - gameTime)
    presumedAlive = remaining == 0 && no kill since nextSpawnAt (&& herald window not closed)
```

- One pure function, called from `DoTick` for **all** paths (normal / rewind / mid-game join /
  pause). The tick-decrement (`IngameController.cs:137-138`) and the entire rewind timer
  recompute block (`:203-226`) are **deleted** — rewind correctness falls out of derivation
  (and the sign bug dies with the branch).
- Elder rule: respawn = `timings.ElderRespawn` once either team has 4+ drakes (soul point);
  else `timings.DragonRespawn`. (Matches current `:220` logic, but config-driven.)
- Herald: spawns at `FirstHerald`, despawns at `HeraldDespawn` (window), no respawn in
  current ruleset; all from config so rule changes are data edits.
- **Zero-crossing events**: clock keeps the previous tick's remaining; fires
  `OnObjectiveSpawn("Dragon"/"Herald"/"Baron")` when `prev > 0 && now == 0` **and**
  `gameTime` moved forward (no popups on rewind or on mid-game join's first tick).
  Dragon popup element = existing `lastDragonType`.
- **Mid-game join**: `ApplyHistoricalBaseline` currently appends synthetic
  `ObjectiveKilled("Dragon", …)` markers only; add the same for **Baron** (and Herald)
  so the clock sees pre-observation kills. Carry-forward (EventID == -1) already keeps them alive.
- `stateData.baron.SpawnTimer` / `dragon.SpawnTimer` become projections of the clock; the
  existing `nextDragon/nextBaron` plumbing and the frontend `ObjectiveTimerVisual` for Baron
  are already in place — **zero frontend work**.
- Flip `UseCustomBaronTimer` default → `true`; delete the "non functional for now" comment.
- Display policy while "presumed alive": timer hidden (strict accuracy: we don't know HP/alive
  for certain, we only know it should have spawned; hiding beats guessing).

**Risks**: /eventdata sparsity after replay seek (documented project-wide) — the clock then
derives from trimmed history exactly like dragon counting does today; first-spawn countdowns
remain correct because they need no events at all.

## 3. Design 2: Gold Graph from OCR

The fork already has **exact team gold** per tick (OCR sidecar, `Team.ExternalGold` +
`GoldConfidence`). The graph just needs a history of it.

- New `State.teamGoldDiffHistory: SortedList<double, float>` (gameTime → blueGold − redGold).
- Append in `DoTick` after `OcrGoldController.ApplyTo`, **only when**:
  both teams `Confidence == Exact` (config `AllowStaleInGraph` default false), game not
  paused, `gameTime` strictly advanced, and ≥ `MinSampleIntervalSec` (default 5) since last
  point. Gaps from OCR-Unknown stretches stay gaps — honest by design.
- `GetGoldGraph()` returns this history (same `Dictionary<double,float>` wire shape ⇒
  frontend `GraphVisual` untouched; keep the existing >500g / >15s downsampler and 1000-point cap).
- **Rewind**: trim keys `> newGameData.gameTime` (the seek **target** — the documented
  old-vs-target lesson from the drake-trim fix).
- **Dead-code deletion**: remove `Player.goldHistory`, `Player.csHistory`, and the rewind
  rollback block `IngameController.cs:159-172`. This *also fixes* the CS-resets-to-0-on-rewind
  bug — `scores.creepScore` refreshes from /playerlist on the next tick. (Verify no other
  readers: `GetGoldGraph` (replaced), CSPM uses live `scores.creepScore` ✓.)
- Gate: `GoldGraph.Enabled` auto-true only when OCR gold is active; otherwise the tile is
  Unavailable (§6). GraphVisual init in `IngameScene` already honors `Enabled`.

## 4. Design 3: Gold Tab

Per-player gold has **no API source**. Decision tree, in order:

1. **Phase 0 — HUD audit (do first, ~1 session)**: in a running replay, enumerate every
   spectator HUD view (Tab scoreboard, side-frame stat cycles, player detail panels) with the
   existing capture tooling; screenshot each; record in SPEC whether *any* shows per-player
   current gold. This decides everything below.
2. **If the HUD shows it** → extend the OCR sidecar exactly like towers: 10 fixed ROIs,
   same emphasis→Otsu→upscale→EasyOCR digit pipeline, per-ROI `CountGate`; sidecar JSON gains
   `playersGold[10]`; C# maps frame order → `Player.ExternalGold` (+ per-player confidence);
   `PlayerTab.GetGoldTabs` switches from `farsightObject.GoldTotal` to it and **hides** rows
   that aren't Exact/Stale.
3. **If not** → tab stays Unavailable by default (§6), plus an explicit operator opt-in
   `ShowEstimatedPlayerGold` (default **off**) that renders the existing item+CS+kills
   estimator per player with a visible "≈" marker. Policy fit: hiding is the default;
   an explicit, labeled opt-in is the coder-customizable escape hatch.
4. **Optional research spike R (replay-only exact)**: parse `.rofl` payload chunks
   (they contain per-player gold/XP). Vanguard-safe (file parsing, no process access) but
   undocumented format, per-patch churn, real maintenance cost. Behind
   `IExactPlayerStatsProvider`; survey prior art (ReplayBook, CommunityDragon) before any code.
   Only worth it if Phase 0 says "no HUD source" **and** replay-mode exactness matters.

## 5. Design 4: EXP Tab

No observable XP exists: not in /playerlist, not numeric on the HUD (the portrait ring arc is
too low-res for trustworthy OCR — would violate strict accuracy), LiveEvents dead.

- Default: tile **Unavailable** under Vanguard (§6), with reason.
- **Exact replacement tabs** (cheap, immediate value): add InfoPage tab types fed by
  /playerlist exacts — **Vision Score** (`wardScore`), **KDA**, **Level**. Same tab-cycling
  system, new `PlayerOrder`-style tab configs. Casters get comparable per-player pages that
  are all true.
- If spike R lands, EXP auto-enables in **Replay mode only** via the capability map.

## 6. Design 5: FeatureAvailability

One service answering "can this feature work *right now*?":

```
FeatureAvailability.Of(feature) -> Available | Unavailable(reason) | Degraded(reason)
inputs: PlaybackMode (Live/Spectator/Replay), UseMemoryReader, OCR sidecar state,
        per-feature data-source requirements (declarative table)
```

- WPF Ingame tiles bind `IsEnabled` + tooltip to it: EXP/Gold Tab grey out with
  "requires memory reader (Vanguard) / no OCR source"; Gold Graph greys when OCR is off;
  Baron Timer becomes enabled once Design 1 ships. Kills the "toggle that silently does
  nothing" class permanently.
- Also exposed on `/api` status payloads so overlays can self-document gaps.

## 7. Patch constants & verification plan

`ObjectiveTimingsConfig` (new JSON section; **data, not code** — values below are placeholders
to be verified in Phase 0 against the live replay + patch notes before shipping defaults):

| Key | Placeholder | Note |
|---|---|---|
| FirstDragon / DragonRespawn | 300 / 300 | matches current code |
| ElderRespawn | 360 | soul-point rule |
| FirstBaron | **1500 (25:00)? verify** | code says 1200 (20:00) — stale |
| BaronRespawn | **360 (6:00)? verify** | code says 420 (7:00) — likely stale |
| FirstHerald / HeraldDespawn | 960 / 1500? verify | new (no herald timer today) |
| FirstGrubs | 360? verify | future: grubs timer uses same clock |

Verification (recursive, per project practice):
- **Clock**: pure function ⇒ table-driven console tests (first spawn, respawn, elder switch,
  rewind across a kill — the sign-bug regression case, mid-game join with baseline markers).
- **Live replay E2E**: seek pre-Baron-spawn → countdown visible → popup fires exactly when the
  HUD pip appears; kill a drake → 5:00 reset; backward seek across a baron kill → timer correct.
- **Gold graph**: live session accumulates points; cover the HUD region → graph gap (no
  interpolation); backward seek → history trimmed to target.
- **Harness**: extend `mock-state.json` with a goldGraph series + baron SpawnTimer to verify
  GraphVisual/ObjectiveTimerVisual rendering without a game.

## 8. Build order & open decisions

Order by value ÷ cost (1-4 are independent of any user decision):

1. **Design 1** ObjectiveSpawnClock — pure backend; fixes Baron Timer, Dragon/Herald spawn
   pop-ups, the rewind sign bug; deletes two special-case branches.
2. **Design 2** Gold Graph OCR history — small; deletes dead goldHistory/csHistory and fixes
   the CS-rewind bug as a side effect.
3. **Design 5** capability map + tile UX — honesty in the UI.
4. **Design 4** exact replacement tabs (Vision/KDA/Level).
5. **Phase 0** HUD audit → decides Gold Tab path (OCR vs opt-in estimate vs spike R).

Decisions (resolved 2026-06-13 by the operator):
- **(a) RESOLVED — estimate shown as real gold.** The item+score estimate is judged accurate
  enough; per-player gold is displayed as the gold value with NO "≈" label. Implemented:
  Gold Tab (`PlayerTab.GetGoldTabs` → `Team.EstimatePlayerGold`, now public), broadcast bottom-bar
  per-lane gold-diff chips (`PlayerScoreboardEntry.Gold` → `applyLaneGoldDiff`, `< 0.4K` style),
  and the Gold Graph.
- **(b) RESOLVED — XP is structurally unobtainable without memory reading** (deep-researched,
  14 agents, adversarially verified): no Riot local endpoint has EVER exposed XP (not even
  `activeplayer` for the local player); the data exists only in Riot's partner-gated esports
  Livestats feed (unification request closed "not planned"). Every non-partner tool either
  reads memory (RCVolus Farsight path) or drops XP. Best achievable: exact at level-up
  boundaries + bracket-bounded interpolation (hard bound 280–1880 XP ≈ ±940 XP worst case) —
  not broadcast-trustworthy as a number. **Follow-up decision: EXP feature dropped entirely
  and removed from the GUI** (tile, ViewModel bindings, CurrentSettings.EXP, GetEXPTabs) —
  the Players panel now has 4 tiles. Exact level-derived metrics (lane level matchups,
  level-up feed) remain available as future additions if wanted.
- **(c) RESOLVED — dropped.** Replay is out of scope for the product (live/spectator broadcast
  is the target); .rofl parsing (spike R) is cancelled.

### Implemented 2026-06-13 (Design 1: ObjectiveSpawnClock) — table-tested

`Ingame/State/ObjectiveSpawnClock.cs` now derives the Dragon/Baron/Herald countdowns from
raw kill events + `ObjectiveTimingsConfig` (Component.json; patch 26.x values verified:
Baron 20:00 / respawn 6:00, Herald 15:00 → despawn 19:45, Dragon 5:00/5:00, Elder 6:00) on
every tick. Deleted: the tick-decrement, the rewind recompute (and its inverted-sign bug),
the baron-only zero-crossing branch, the OnDragonTaken/OnBaronTaken timer seeds (stale 420),
and the game-start dragon seed. Spawn pop-ups now fire for Dragon (named "Elder" at soul
point), Herald, and Baron — including Baron's FIRST spawn, which previously never counted
down. `UseCustomBaronTimer` defaults true; frontend unchanged (ObjectiveTimerVisual already
renders the alive state at remaining == 0). Logic verified by a 17-case table test against
the built assembly (first-tick derivation, single-fire crossings, sign-bug regression,
elder rule, herald one-shot, mid-join silence, rewind suppression). With this, every tile
in the Ingame GUI is functional.

### Implemented 2026-06-13 (Gold Graph + Gold Tab + lane diffs) — review-hardened

Design 2 (graph) and the per-player gold consumers shipped together, live-verified
(graph accumulating OCR/estimate samples; 10-player gold tab; lane diff chips). An
adversarial review (3 lenses) surfaced 5 real issues, all fixed:
1. **Thread-safety (HIGH)**: `teamGoldDiffHistory` mutated on overlapping async-void ticks
   while Newtonsoft serializes `goldGraph` outside DoTick's try/catch → potential process
   kill. Fixed: `goldGraphLock` + snapshot in `GetGoldGraph`.
2. **Trim gating**: history trim ran only inside the `pastIngameEvents.Count != 0` rewind
   branch → seeks before the first objective kept "future" samples and stalled sampling.
   Fixed: trim hoisted to its own backward-seek check.
3. **Empty info page (HIGH)**: a titled page with zero tabs (EXP under Vanguard; any tab in
   the first 5s) crashed the frontend heartbeat handler (`Players[0]`), silently freezing the
   gold graph. Fixed on both sides: backend sends null instead (+`ShouldSerializeinfoPage`
   null gate), frontend guards empty Players (InfoPageVisual + InfoSidePage null-safe ctor).
   Regression fixture: `ocr-poc/overlay-harness/mock-state-empty-infopage.json`.
4. **Live-mode garbage graph**: with only the local player visible, one team sums to 0.
   Fixed: record only when BOTH rosters are non-empty.
5. **CS freeze on default config (HIGH)**: `UseLiveEvents=true` (shipped default) made
   `Score.Update` skip creepScore from /playerlist, but the LiveEvents API (the only other
   CS writer) was removed in patch 14.1 → CS frozen at first observed value, hollowing the
   gold estimator. Fixed: CS now comes from /playerlist unless LiveEvents is actually
   **Connected**.
