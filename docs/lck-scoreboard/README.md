> 🌐 **English** ・ [日本語](README-ja.md)

# Comparison bottom-scoreboard overlay — spec & reference

Spec and reference asset for reproducing a **pro-broadcast bottom
comparison scoreboard layout** in the ingame overlay, designed to coexist with the
broadcast top bar. The implementation is
`Overlays/ingame/src/visual/LckScoreboardVisual.ts`, selected via the additive
`BottomStyle: 'prm' | 'lck'` config discriminator; it reuses the existing per-player
roster (`state.scoreboard.Players`) with no new backend data.

## Contents

- [`SPEC.md`](SPEC.md) — the `/goal` build spec: scope/non-goals, reference decode
  of the band layout (5 mirrored role rows of champion+level · items · KDA · CS,
  with a center lane gold-diff), metric → data-source map, file-by-file frontend
  changes, the calibrated 1920×1080 geometry, invariants to preserve, the
  verification-harness procedure, and acceptance criteria. Includes the dated
  "implemented & harness-verified" status and the baked-in operator decisions
  (edge dmg/vision/KP numbers and a per-player gold column are **out of scope**;
  gold is shown only as the center lane gold-diff).
- `lck_reference.png` — the broadcast reference frame (a pro match, ~1920×1080,
  the event/series title, patch 26.11) that §7's geometry is calibrated against in
  the render harness.

Verified with [`../../ocr-poc/overlay-harness/`](../../ocr-poc/overlay-harness/);
follows the same pattern as the broadcast top bar's bottom bar
([`../prm-overlay/`](../prm-overlay/)).
