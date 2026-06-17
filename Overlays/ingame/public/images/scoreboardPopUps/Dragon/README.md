> 🌐 **English** ・ [日本語](README-ja.md)

# Dragon popup art

Per-elemental-dragon spawn/kill/soul popup banners, one subdirectory per dragon type. The consumer ([`../../../../src/visual/ObjectivePopUpVisual.ts`](../../../../src/visual/ObjectivePopUpVisual.ts)) strips `Kill`/`Spawn`/`Soul` from the popup type to choose the folder, then loads `<type><Event>.{png,mp4}`. Elder has no Soul variant.

## Subdirectories

| Dir | Purpose |
| --- | --- |
| [`Fire/`](Fire/) | Infernal dragon popups |
| [`Mountain/`](Mountain/) | Mountain dragon popups |
| [`Cloud/`](Cloud/) | Cloud dragon popups |
| [`Ocean/`](Ocean/) | Ocean dragon popups |
| [`Hextech/`](Hextech/) | Hextech dragon popups |
| [`Chemtech/`](Chemtech/) | Chemtech dragon popups |
| [`Elder/`](Elder/) | Elder dragon popups (spawn/kill only) |
