> 🌐 **English** ・ [日本語](README-ja.md)

# Overlay background plates and video backings

Background art for the ingame overlay panels. Each panel typically ships as both a static `.png` and a looping `.mp4`; the visual element picks the video when its config sets `UseVideo`, otherwise the image (see `ScoreboardVisual`, `GraphVisual`, `InhibitorVisual`, `InfoPageVisual` in [`../../src/visual`](../../src/visual)). Loaded as `frontend/backgrounds/...`.

## Contents

- `Score.png` / `Score.mp4` — scoreboard panel backing (`ScoreboardVisual`).
- `GoldGraph.png` / `GoldGraph.mp4` — gold/graph panel backing (`GraphVisual`).
- `Inhibitor.png` / `Inhibitor.mp4` — inhibitor-timer panel backing (`InhibitorVisual`).
- `InfoPage.png` / `InfoPage.mp4` — info page backing (`InfoPageVisual`).
- `BaronIcon.png`, `DragonIcon.png` — objective icons on the top objective bar.
- `BaronTimer.png`, `DragonTimer.png` — objective respawn-timer plates.
- `ObjectiveBG.png`, `ObjectiveBGLeft.png` — objective bar backings.
- `ScoreTeamIconBGLeft.png`, `ScoreTeamIconBGRight.png` — per-team scoreboard logo backings.
- `ItemText.png` — item-text panel backing.
