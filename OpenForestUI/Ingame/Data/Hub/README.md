> 🌐 **English** ・ [日本語](README-ja.md)

# Aggregated game-state hub models

The broadcast-side aggregate model — the layer between the raw per-tick Riot DTOs (`RIOT/`) and the frontend payloads (`Frontend/`). These types accumulate teams, players, inhibitors, objectives, and the gold figures the overlay shows. This is where the Vanguard-compatibility constraints live: per-player/team gold is estimated or OCR-sourced because the spectator API doesn't expose it.

## Contents

- [`Team.cs`](Team.cs) — Per-team aggregate: players, kills/towers/plates, void grubs, inhibitors destroyed, baron/elder timers, `dragonsTaken`. Owns the **gold logic**: `GetGold` prefers an OCR-sourced `ExternalGold` (with a `GoldConfidence` flag) and otherwise falls back to `EstimatePlayerGold`, a CS/kill/assist + passive-income heuristic. Also carries OCR-sourced objective counts (`OcrGrubs/Baron/Dragons/Towers`) that override event-counted values when present.
- [`PlayerTab.cs`](PlayerTab.cs) — Builders for the info side page rows: `GetGoldTabs` (estimated per-player gold) and `GetCSPerMinTabs` (CS/min), each producing `PlayerTab` rows with a `ValueBar` (min/current/max) and champion icon path. (XP tabs were removed — no Vanguard-safe per-player XP source.)
- [`InfoSidePage.cs`](InfoSidePage.cs) — A side-page of `PlayerTab`s sorted by the `PlayerOrder` enum (`MaxToMin` / `MinToMax`).
- [`Inhibitor.cs`](Inhibitor.cs) — A single inhibitor (`id`, `key`, `timeLeft`) and `InhibitorInfo`, which seeds the six standard inhibitors.

## Subdirectories

| Directory | Purpose |
| --- | --- |
| [`Objectives/`](Objectives/) | Baron/Dragon power-play and upcoming-spawn models. |
