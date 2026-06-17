> 🌐 **English** ・ [日本語](README-ja.md)

# Vanguard-compatible feature-completion design

The design notes for making every Ingame overlay feature work **without memory
reading**, under the fork's policies: strictly accurate (prefer hiding to
approximating), Vanguard-compatible (Live Client Data API + HUD OCR only). This is
the design that turned the formerly non-functional Ingame tiles (Baron Timer,
spawn pop-ups, Gold Graph/Tab, EXP Tab) into working — or honestly-unavailable —
features.

## Contents

- [`DESIGN.md`](DESIGN.md) — the full design. Highlights:
  - **Ground truth** — root cause (file:line) for why each tile was broken (e.g.
    timers seeded-and-decremented with an inverted-sign rewind recompute; dead
    `goldHistory`/`csHistory`; the LiveEvents API removed by Riot in patch 14.1).
  - **Design 1 — `ObjectiveSpawnClock`**: replace seed+decrement with a pure
    per-tick derivation of Dragon/Herald/Baron countdowns from kill events +
    `ObjectiveTimingsConfig`; fixes the Baron timer, spawn pop-ups, and the rewind
    sign bug. *Implemented & table-tested.*
  - **Design 2 — Gold Graph from OCR team gold**; **Design 3 — Gold Tab**
    (per-player gold via estimate); **Design 4 — EXP Tab** (XP is structurally
    unobtainable without memory reading → feature dropped).
  - **Design 5 — `FeatureAvailability`** capability map so tiles self-document
    when a feature can't work right now.
  - Patch constants, a recursive verification plan, build order, and dated
    "Implemented" / review-hardening notes.

The verification plan extends the mock fixtures in
[`../../ocr-poc/overlay-harness/`](../../ocr-poc/overlay-harness/) (e.g.
`mock-state-empty-infopage.json`, added as a regression fixture here).
