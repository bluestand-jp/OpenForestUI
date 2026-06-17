> 🌐 **English** ・ [日本語](README-ja.md)

# Overlay scene (WebSocket client + visual orchestrator)

The single Phaser scene that drives the in-game overlay. It is registered in
`main.ts` and is the central coordinator between the backend and the visual
elements.

## Contents

- [`IngameScene.ts`](IngameScene.ts) — the one scene. Responsibilities:
  - **Asset loading** (`preload`): masks, objective/dragon/timer icons, scoreboard
    and broadcast top-bar icons, lane SVGs, item backgrounds, and Chart.js (for the gold
    graph). All relative loads are anchored at site root so they resolve under
    `/frontend/...`.
  - **WebSocket lifecycle** (`create` → `connect`): opens `ws://host:9001/api`,
    requests the `Ingame` `OverlayConfig`, auto-reconnects on close, and on each
    message dispatches by `eventType` (`GameHeartbeat`, `OverlayConfig`,
    `PlayerLevelUp`, `ObjectiveKilled`/`Spawn`, `ItemCompleted`, `GameEnd`,
    `ForceRefresh`, …).
  - **Config wiring** (`UpdateConfig` / `UpdateConfigWhenReady`): loads Google +
    local fonts, then creates/updates every visual element. Selects the legacy
    `ScoreboardVisual` vs. the opt-in broadcast top bar (`PrmScoreboardVisual`) and, when
    `PrmScore.BottomBar` is on, the bottom comparison scoreboard variant (by `BottomStyle`). The
    gold graph (`GraphVisual`) is opt-in / lazily created.
  - **State fan-out** (`OnNewState`): wraps each heartbeat in `StateData` and feeds
    inhibitors, scoreboard/top bar, bottom bar, legacy objective timers/power-plays,
    info page, and gold graph. Implements the scoreboard master-toggle gate (absent
    `GameTime` ⇒ scoreboard off → hide the relevant boards) and soul-point detection
    (`CheckSoulPoint`).
  - Holds the 11 `RegionMask`s (one per player slot + a global region) used to clip
    per-player level-up / item pop-up animations.
