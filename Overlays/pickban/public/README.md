> 🌐 **English** ・ [日本語](README-ja.md)

# Static public assets

Files copied verbatim into the production `build/` (CRA `public/` folder). Holds the HTML shell and the WebSocket client that bridges the overlay to the OpenForestUI backend.

## Contents

| File | Role |
| --- | --- |
| `index.html` | HTML template (`<div id="root">`, title "LoL CS UI", Google web-font links). Loads `frontend-lib.js` before the React bundle. |
| `frontend-lib.js` | Defines the global `Window.PB` — an event-emitter WebSocket client. `PB.start()` connects to the backend (`?backend=` query var), auto-reconnects every 500 ms, emits messages by `eventType` (e.g. `newState`, `heartbeat`), and exposes `PB.toAbsoluteUrl()` for `/cache` image URLs. Consumed by [`../src/App.jsx`](../src/App.jsx). |
| `robots.txt` | Default disallow-nothing robots file. |
