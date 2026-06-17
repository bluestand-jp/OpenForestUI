> 🌐 **English** ・ [日本語](README-ja.md)

# HUD OCR sidecar (exact CS / gold / objective counts)

The Python OCR proof-of-concept and sidecar that reads values the spectator Live
Client Data API rounds or omits — exact per-player **CS**, per-team **gold**, and
per-team **objective counts** (grubs/baron/dragon/towers) — straight off the
native HUD. The spectator API floors CS to multiples of 10 and never exposes gold
or objective-monster kills (see [`../docs/api/`](../docs/api/)), so OCR is the only
strictly-accurate source. `goldcap.py` is the live sidecar the C# app launches and
reads JSON lines from; the rest is the reference pipeline / calibration tooling.

## Live-capture requirements

`goldcap.py --live` captures **your actual screen** (DXGI Desktop Duplication) and reads
fixed pixel regions, so a read only works when the HUD is visible and matches the layout
the ROIs were calibrated for:

- **Resolution 1920×1080.** All ROIs (`ROI_1080`, `TOWER_ROI_1080`, `CS_ROI_1080`,
  `OBJ_ROI_1080`) are calibrated for 1080p borderless-fullscreen. Other resolutions
  auto-scale (`scaled_roi`: `W/1920, H/1080`), but the LoL HUD does not scale perfectly
  linearly, so they can misalign — recalibrate if so.
- **Spectator interface / HUD scale at the calibrated setting.** The reference frames were
  captured at interface scale **100 (max)**; a different scale shifts the panels off the ROIs.
- **The observer top bar must be visible.** Team gold, the tower count, and the objective
  row (grub / baron / dragon / tower) are all read from it.
- **The detail scoreboard (bottom-center panel) must be open** at default zoom/position —
  per-player CS is read from its 10 cells (B0–B4 = left team / ORDER top→bottom,
  R0–R4 = right team / CHAOS).
- **Nothing may overlap those regions** — other windows, pop-ups, the mouse cursor, or
  stream overlays over the bars corrupt the read. By design a failed or implausible read is
  *held or hidden* (tri-state Known / Stale / Unknown), never shown wrong.
- **Primary monitor.** `dxcam` grabs the primary output by default; run the spectator client
  there (or edit `make_cam()` to pick another output).
- The reader assumes the standard **blue/red observer colour scheme** — digits are
  team-coloured and `_emphasis()` keys off that.

## Running the sidecar

```bash
py -m pip install -r ../requirements.txt   # one-time; deps live in the repo-root requirements.txt

py goldcap.py --grab frame.png   # save one desktop frame, to calibrate / verify ROIs
py goldcap.py --probe            # capture one frame, run the readers once, print the result
py goldcap.py --live --fps 4     # continuous loop: tri-state JSON lines on stdout (what C# reads)
```

The C# app launches `--live` itself and consumes the JSON lines, so you normally don't run
this by hand. To recalibrate for a different setup, grab a frame, find the digit regions, and
pass them with `--roi "x0,x1,y0,y1;x0,x1,y0,y1"` (blue;red), or adjust the `*_1080` constants
in `topbar_reader.py`.

## Contents

- [`goldcap.py`](goldcap.py) — **the live sidecar**. Captures the observer top bar
  via `dxcam` (DXGI Desktop Duplication), runs the readers, and emits tri-state
  JSON lines on stdout (per-team gold + towers, 10-cell CS, per-team objective
  counts). Modes: `--grab` (save a frame for ROI calibration), `--probe` (one read),
  `--live` (continuous loop). Reads `reset` on stdin to re-lock every gate when the
  C# side reports a replay seek.
- [`topbar_reader.py`](topbar_reader.py) — **the finalized reader logic** (the
  reference to be ported to C#). Fixed 1080p ROIs (`ROI_1080`, `TOWER_ROI_1080`,
  `CS_ROI_1080`, `OBJ_ROI_1080`), the emphasis→Otsu→upscale→EasyOCR digit pipeline,
  range checks + a monotonic/bounded **`TeamGate`** with hold-last-good →
  tri-state (Known / Stale / Unknown — it hides rather than lies). `read_cs` uses a
  glyph-segmentation + NCC template classifier (templates from `_harvest.py`).
- [`digit_templates.npz`](digit_templates.npz) — canonical 0–9 digit templates
  (HUD font, `top` + `cs` contexts) for the NCC classifier. Built by `_harvest.py`;
  do not hand-edit.
- [`_harvest.py`](_harvest.py) — builds `digit_templates.npz` by harvesting labeled
  digit glyphs from **native HUD** captures only (excludes overlay renders, which
  use a different font).
- [`csdiag.py`](csdiag.py) — one-shot CS-cell diagnostic: captures via PIL/GDI,
  crops the 10 CS cells, and prints `read_cs` + the per-glyph segmentation/NCC
  breakdown to debug why specific cells fail.
- [`replay.py`](replay.py) — re-runs a recorded raw-OCR JSONL log through the
  (fixed) parse + gate to validate logic changes without a fresh live capture.

## Subdirectories

| Directory | Purpose |
|---|---|
| [`overlay-harness/`](overlay-harness/) | Headless render harness — feeds a mock WS state to the ingame overlay and screenshots it for layout verification |

> Calibration/debug captures (`cs_*.png`) and `__pycache__` are gitignored.
> The newer per-player CS pipeline is described inline in `topbar_reader.py`.
