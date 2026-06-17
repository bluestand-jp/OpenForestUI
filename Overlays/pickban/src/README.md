> 🌐 **English** ・ [日本語](README-ja.md)

# Pick & ban overlay source

The React application that renders the champion-select overlay. The entry point boots React, subscribes to the backend WebSocket via `Window.PB`, normalizes the incoming state, and hands it to the `europe` draft layout.

## Contents

| File | Role |
| --- | --- |
| `index.js` | Renders `<App>` into `#root` and unregisters the service worker. |
| `App.jsx` | Subscribes to `newState` / `heartbeat` from `Window.PB`, holds `state`/`config`, and renders `<Overlay>` (hidden while in-game). |
| `convertState.js` | Pads each team to 5 picks/bans with placeholder art and rewrites `/cache/...` image URLs to absolute backend URLs. |
| `serviceWorker.js` | Default CRA PWA service-worker helper (registration is disabled). |
| `index.css` | Base page reset and the red `.infoBox` "not connected" banner styles. |

## Subdirectories

| Directory | Purpose |
| --- | --- |
| [`assets/`](assets/) | Logo, placeholder splash/ban SVGs, fonts |
| [`assets/fonts/`](assets/fonts/) | TrueType fonts used by the overlay |
| [`europe/`](europe/) | The "europe" draft layout components and styles |
| [`europe/style/`](europe/style/) | LESS styles and draft-reveal animations |
