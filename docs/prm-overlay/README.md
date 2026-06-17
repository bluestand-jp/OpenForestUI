> 🌐 **English** ・ [日本語](README-ja.md)

# Broadcast top-bar overlay — spec & reference

Spec, reference frame, and extracted icons for reproducing a **pro/esports
broadcast top bar** in the ingame overlay, with strictly-accurate data from the
Live Client Data API (port 2999), the LCU client, and operator config —
preferring to hide a value over showing an approximation. Implemented as
`Overlays/ingame/src/visual/PrmScoreboardVisual.ts` (top bar) +
`PrmBottomBarVisual.ts` (bottom comparison bar), opt-in via
`OverlayConfig.PrmScore`.

## Contents

- [`SPEC.md`](SPEC.md) — the `/goal` build spec: reference decode (top-bar regions
  + center objective counters), metric → data-source map, the void-grub
  (`HordeKill`) event verification, color/layout spec, the config-driven
  counter-slot architecture, backend + frontend change lists, the verification
  harness, and the phasing (Phase 1 top bar + Phase 2 bottom bar both implemented
  & harness-verified). Includes open questions to re-confirm during visual diffing.
- `prm_reference.png` — the decoded 1920×1080 broadcast reference (a pro match,
  19:52) the layout/colors are calibrated against.
- Extracted top-bar counter icons (white-on-transparent), the source for the
  in-overlay assets under `Overlays/ingame/public/images/prm/`:
  - `ic_tower.png` — turret silhouette (towers count)
  - `ic_grub.png` — void-grub head (void grubs count)
  - `ic_shield.png` — shield/crest (dragons count)

Verified with [`../../ocr-poc/overlay-harness/`](../../ocr-poc/overlay-harness/);
the comparison scoreboard ([`../lck-scoreboard/`](../lck-scoreboard/)) follows this
same pattern.
