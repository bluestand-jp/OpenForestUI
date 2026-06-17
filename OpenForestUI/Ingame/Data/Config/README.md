> 🌐 **English** ・ [日本語](README-ja.md)

# Ingame & Farsight configuration models

The two persisted JSON config sections that drive the ingame overlay and the memory reader. Both derive from `OpenForestUI.Common`'s `JSONConfig` (versioned, with default-creation and migration hooks) and are loaded through `ConfigController` as `Ingame.json` / `Farsight.json`.

## Contents

- [`IngameConfig.cs`](IngameConfig.cs) — The big layout/styling config for every ingame overlay element: scoreboard, inhibitors, objective kill/spawn pop-ups, item-completed and level-up animations, info side page, gold graph, Baron/Elder power plays, Dragon/Baron timers, Google fonts, and the opt-in `PrmScore` block (broadcast top bar + comparison scoreboard style). Contains the nested `*DisplayConfig` / `FontConfig` / `VisualElementAnimationConfig` types and the large `CreateDefault()` baseline. Current file version `3.1`.
- [`FarsightConfig.cs`](FarsightConfig.cs) — Holds the memory-reader **offsets** (`GameOffsets`, `ObjectOffsets`) and `OffsetVersion`. `CreateDefault`/`UpdateValues` auto-download the matching offset file for the local patch from the configured offset repository; on failure it disables `FarsightController.ShouldRun`. Current file version `3.0`.
