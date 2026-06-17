> 🌐 **English** ・ [日本語](README-ja.md)

# Project documentation & design specs

Reference docs, build specs, and design notes for OpenForestUI — the public,
MIT-licensed hub of League of Legends broadcast overlays. These files capture
**what the live data sources actually expose** (so features stay strictly
accurate / Vanguard-compatible) and the **layout specs** the overlays are built
against. They are the source of truth that the `OpenForestUI/` app, the
`Overlays/` browser sources, and the `ocr-poc/` sidecar are implemented to match.

## Subdirectories

| Directory | Purpose |
|---|---|
| [`api/`](api/) | Complete reference for the League local API at `https://127.0.0.1:2999` (Live Client Data + Replay API) — what is and isn't obtainable when spectating |
| [`data/`](data/) | Archived game-data tables used by the app (Farsight memory offsets) |
| [`feature-completion/`](feature-completion/) | Design for making every Ingame feature work without a memory reader (objective clock, gold graph, capability map) |
| [`lck-scoreboard/`](lck-scoreboard/) | Spec + reference image for the comparison scoreboard bottom visual |
| [`prm-overlay/`](prm-overlay/) | Spec + reference image/icons for the broadcast top-bar overlay |

## Contents

- Most leaf docs are written as a `/goal` build target: a table of contents, a
  reference decode, a metric → data-source map, and an explicit list of
  verification / acceptance criteria. Implementation status notes are kept inline
  and dated as features land.
- The overlay specs ([`prm-overlay/`](prm-overlay/), [`lck-scoreboard/`](lck-scoreboard/))
  are verified against the headless render harness in
  [`../ocr-poc/overlay-harness/`](../ocr-poc/overlay-harness/).
