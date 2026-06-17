> 🌐 **English** ・ [日本語](README-ja.md)

# In-game overlay source (Phaser 3 / TypeScript)

All TypeScript for the in-game browser source. `main.ts` boots a single Phaser
WebGL game with one scene (`IngameScene`), which owns the WebSocket connection to
the backend, instantiates the visual elements, and routes each pushed state/event to
the matching visual. Modules use the `~/` path alias (configured in `tsconfig.json`)
to reference files under `src/`.

## Contents

| File | Role |
| --- | --- |
| [`main.ts`](main.ts) | Entry point. Builds the `Phaser.Game` config (1920×1080, transparent, WebGL), registers the rexUI + WebFontLoader plugins, and starts `IngameScene`. |
| [`index.html`](index.html) / [`style.css`](style.css) | Host page (the `#gameContainer` div) and full-bleed transparent canvas styling. |
| [`variables.ts`](variables.ts) | Static config: backend host/port (`localhost:9001`), WS path (`api`), SSL flag, fallback team colors, gold color constants. |
| [`PlaceholderConversion.ts`](PlaceholderConversion.ts) | Rewrites backend `cache/...` asset placeholders into absolute `http(s)://host:port/cache/...` URLs; passes through already-absolute URLs (DataDragon, team logos). |

## Subdirectories

| Directory | Purpose |
| --- | --- |
| [`scenes/`](scenes/) | The single Phaser scene (`IngameScene`) — WebSocket client, config/event router, visual orchestrator. |
| [`visual/`](visual/) | One class per on-screen element (scoreboard, timers, graph, pop-ups…), all extending a shared `VisualElement` base. |
| [`data/`](data/) | TypeScript mirrors of the JSON the backend pushes (`StateData`, scoreboard/team/objective/inhibitor models). |
| [`data/config/`](data/config/) | The `OverlayConfig` interfaces describing the per-element display/layout config. |
| [`convert/`](convert/) | Small browser helpers (e.g. reading the `?backend=` query var). |
| [`util/`](util/) | Generic helpers: gold/text formatting, font loading, color, `Vector2`, `Queue`, `Dictionary`. |
