> 🌐 **English** ・ [日本語](README-ja.md)

# Overlay visual elements (Phaser render components)

One class per on-screen element of the in-game overlay. Each extends the abstract
`VisualElement` base, which standardizes the lifecycle (`Load` / `Start` / `Stop` /
`UpdateValues` / `UpdateConfig`), tween-driven animation states, and registration
with the scene. `IngameScene` creates these from the `OverlayConfig` and feeds them
each `StateData` heartbeat.

## Contents

| File | Role |
| --- | --- |
| [`VisualElement.ts`](VisualElement.ts) | Abstract base for every visual: id registration, `PlayAnimationState` tween sequencer, `AnimationStart`/`Complete` signals, text-style helper. |
| [`VisualComponent.ts`](VisualComponent.ts) | Lightweight wrapper pairing a Phaser game object with its `Size` and an `AnimateScale` flag, for the animation sequencer. |
| [`ScoreboardVisual.ts`](ScoreboardVisual.ts) | Legacy/default scoreboard: per-team kills/towers/gold, drake icons, score, names, center clock; masked, config-driven layout. |
| [`PrmScoreboardVisual.ts`](PrmScoreboardVisual.ts) | Opt-in broadcast top bar (pro/esports broadcast style): gradient bar, per-team objective counters (dragons/grubs/towers), gold + lead badge, kills, team panel, center clock. Replaces `ScoreboardVisual` when `PrmScore.Enabled`. |
| [`PrmBottomBarVisual.ts`](PrmBottomBarVisual.ts) | Bottom comparison scoreboard: 5 lane-matchup rows (icons/spells/items/KDA/CS/level), mirrored about center; DataDragon icons. Opt-in via `PrmScore.BottomBar`. |
| [`LckScoreboardVisual.ts`](LckScoreboardVisual.ts) | Alternative comparison scoreboard (pro/esports broadcast style): 5 role rows, center per-lane gold diff with leader arrow, patch label. Used instead of `PrmBottomBarVisual` when `BottomStyle === 'lck'`. |
| [`InfoPageVisual.ts`](InfoPageVisual.ts) | Side info page: per-player stat tabs (gold/XP/CSPM) with icon, value bars, and ordering. |
| [`GraphVisual.ts`](GraphVisual.ts) | Center gold-difference graph (Chart.js via rexUI), masked wipe in/out. Opt-in / lazily created from `GoldGraph` data. |
| [`ObjectiveTimerVisual.ts`](ObjectiveTimerVisual.ts) | Dragon / baron spawn-timer widget (icon + countdown). Legacy (non-top-bar) mode. |
| [`PowerPlayVisual.ts`](PowerPlayVisual.ts) | Baron / Elder "Power Play" widget: gold-diff and/or timer with objective icon. Legacy (non-top-bar) mode. |
| [`InhibitorVisual.ts`](InhibitorVisual.ts) | Inhibitor respawn timers per team/lane, with team-color and background options. |
| [`ObjectivePopUpVisual.ts`](ObjectivePopUpVisual.ts) | Center scoreboard pop-up for objective kill / spawn / soul-point (image or video, alpha mask). |
| [`ItemVisual.ts`](ItemVisual.ts) | Item-completion notification anchored to a player slot (icon + optional item-name text). |
| [`LevelUpVisual.ts`](LevelUpVisual.ts) | Player level-up notification anchored to a player slot (level number + colored background). |
