> 🌐 **English** ・ [日本語](README-ja.md)

# Objective popup art

Spawn / kill / soul popup art for major neutral objectives, shown as a banner over the scoreboard by `ObjectivePopUpVisual` ([`../../../src/visual/ObjectivePopUpVisual.ts`](../../../src/visual/ObjectivePopUpVisual.ts)). Each event ships as both a still `.png` and a looping `.mp4`; the visual loads the video when its config sets `UseVideo`, else the image. The consumer builds the path as `frontend/images/scoreboardPopUps/<Baron|Herald|Dragon/<Type>>/<type><Event>.{png,mp4}`.

Naming convention: `<type><Spawn|Kill|Soul>` (Baron/Herald have no Soul variant).

## Subdirectories

| Dir | Purpose |
| --- | --- |
| [`Baron/`](Baron/) | Baron spawn/kill popups |
| [`Herald/`](Herald/) | Rift Herald spawn/kill popups |
| [`Dragon/`](Dragon/) | Per-elemental-dragon spawn/kill/soul popups |
