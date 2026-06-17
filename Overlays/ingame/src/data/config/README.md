> 🌐 **English** ・ [日本語](README-ja.md)

# Overlay configuration schema (OverlayConfig DTOs)

The TypeScript interface definitions for the `OverlayConfig` the backend pushes once
per connection (event `OverlayConfig`, type 1). It describes the layout, fonts,
colors, animations, and per-element toggles for every visual; `IngameScene.UpdateConfig`
consumes it to create/update the visual elements.

## Contents

- [`overlayConfig.ts`](overlayConfig.ts) — one file holding the whole schema:
  - `OverlayConfigEvent` — the WS envelope (`type`, `eventType`, `config`).
  - `OverlayConfig` — the root: `Inhib`, `Score`, `ObjectiveKill`/`ObjectiveSpawn`,
    `ItemComplete`, `LevelUp`, `InfoPage`, `BaronPowerPlay`/`ElderPowerPlay`,
    `DragonTimer`/`BaronTimer`, `GoldGraph`, `GoogleFonts`, and the opt-in `PrmScore`.
  - `PrmScoreConfig` — broadcast top bar opt-in (`Enabled`, `Font`,
    `TournamentName`, `BottomBar`, `BottomStyle: 'prm' | 'lck'`, `DDragonVersion`).
  - Per-element display configs: `ScoreDisplayConfig` (+ `TeamConfig`, `ElementConfig`,
    `DrakeConfig`, etc.), `InhibitorDisplayConfig`, `PowerPlayDisplayConfig`,
    `ObjectiveTimerDisplayConfig`, `ObjectiveKill`/`SpawnConfig`, `ScoreboardPopUpConfig`,
    `ItemCompletedDisplayConfig`, `LevelUpDisplayConfig`, `InfoPageDisplayConfig`,
    `GoldGraphDisplayConfig`, and the shared `FontConfig` / `VisualElementAnimationConfig`.

Notable: `GoldGraph.Enabled` and `PrmScore.*` are optional and default to off/undefined
under the Vanguard-compatible fork (no per-player `totalGold` from the live API), so
the scene treats absence as "disabled".
