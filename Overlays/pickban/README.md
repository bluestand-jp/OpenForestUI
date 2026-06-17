> 🌐 **English** ・ [日本語](README-ja.md)

# Champion-select (pick & ban) overlay

The browser-source overlay that renders the League of Legends champion-select / draft stage for tournament broadcasts. It is a React (Create React App, ejected) single-page app that connects over WebSocket to the OpenForestUI C# backend, receives champion-select state, and draws the picks, bans, summoner spells, team names/scores and the draft timer.

This overlay's data is sourced from the League Client (LCU) API: the OpenForestUI desktop app watches champion select via `OpenForestUI/ChampSelect/*` and pushes events through [`PickBanConnector`](../../OpenForestUI/Http/PickBanConnector.cs) on the embedded WebSocket server (`ws://localhost:9001/api`). The overlay HTML/JS is served from `/frontend`.

## Provenance

The champion-select overlay is a fork/integration of **RCVolus** [`lol-pick-ban-ui`](https://github.com/RCVolus/lol-pick-ban-ui) (the `pick-ban-overlay` package). It retains the ejected CRA build tooling and the "europe" draft layout from that project. Distributed under the repo's MIT license.

## How it connects

- `public/frontend-lib.js` exposes a global `Window.PB` event-emitter that opens the WebSocket (backend URL comes from the `?backend=` query string), auto-reconnects, and re-emits each message keyed by its `eventType`.
- `src/App.jsx` subscribes to `newState` (full champion-select state) and `heartbeat` (config only), runs the payload through `convertState.js`, and renders `<Overlay>`.
- `convertState.js` pads each team to 5 picks/bans with placeholder splash/ban art and rewrites `/cache/...` image URLs to absolute `http(s)://<backend>` URLs.

## Contents

| File | Role |
| --- | --- |
| [`src/`](src/) | React app source (App, state conversion, the `europe` draft layout) |
| [`public/`](public/) | Static HTML template, `frontend-lib.js` WS client, `robots.txt` |
| [`config/`](config/) | Ejected CRA webpack/babel/jest/env configuration |
| [`scripts/`](scripts/) | Ejected CRA `start` / `build` / `test` entry scripts |
| `package.json` | Package `pick-ban-overlay`; scripts run `scripts/{start,build,test}.js` |
| `.env` | `REACT_APP_LCSU_BACKEND` default backend host |
| `installPB.bat` / `runFrontend.bat` | Convenience wrappers for `npm install` / `npm start` |

## Subdirectories

| Directory | Purpose |
| --- | --- |
| [`config/`](config/) | Ejected Create React App build configuration |
| [`config/jest/`](config/jest/) | Jest CSS/file transformers |
| [`public/`](public/) | Static assets and the WebSocket client library |
| [`scripts/`](scripts/) | CRA start/build/test scripts |
| [`src/`](src/) | React application source |
| [`src/assets/`](src/assets/) | Logo, placeholder splash/ban art, fonts |
| [`src/assets/fonts/`](src/assets/fonts/) | Web fonts used by the overlay |
| [`src/europe/`](src/europe/) | The "europe" draft layout components |
| [`src/europe/style/`](src/europe/style/) | LESS styles and draft-reveal animations |

## Building / running

```
npm install        # or installPB.bat
npm start          # dev server (port 3000) — runFrontend.bat
npm run build      # production bundle into build/
```

`npm run build` output is what the OpenForestUI backend serves to broadcast/OBS browser sources.
