> 🌐 **English** ・ [日本語](README-ja.md)

# Ingame overlay static assets

Static files served at `/frontend` by the OpenForestUI embedded HTTP server and loaded by the Phaser 3 ingame overlay (`Overlays/ingame/src`). Asset paths are referenced from code as `frontend/<path>` (e.g. preloaded in [`scenes/IngameScene.ts`](../src/scenes/IngameScene.ts)). Holds the background plates, objective/dragon icons, lane glyphs, popup videos, alpha masks and the broadcast theme art used to render the scoreboard, info pages and objective bars.

## Subdirectories

| Dir | Purpose |
| --- | --- |
| [`backgrounds/`](backgrounds/) | Background plates and looping video backings for scoreboard, info page, graph, inhibitor and objective bars |
| [`images/`](images/) | General overlay icons (objectives, towers, separators) plus themed icon subtrees |
| [`masks/`](masks/) | Alpha/bitmap masks that clip overlay surfaces (champ covers, item text, graph, info page) |
