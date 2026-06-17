> 🌐 **English** ・ [日本語](README-ja.md)

# Overlay render verification harness

Render the ingame overlay against a fully controlled state and screenshot it
headlessly (no live game), so the broadcast overlays can be compared element-by-
element against their reference frames — the broadcast top/bottom bar vs
[`../../docs/prm-overlay/prm_reference.png`](../../docs/prm-overlay/prm_reference.png)
and the comparison scoreboard vs
[`../../docs/lck-scoreboard/lck_reference.png`](../../docs/lck-scoreboard/lck_reference.png).
See [`../../docs/prm-overlay/SPEC.md`](../../docs/prm-overlay/SPEC.md) §8 and
[`../../docs/lck-scoreboard/SPEC.md`](../../docs/lck-scoreboard/SPEC.md) §9.

## Usage

```sh
# 1. build the overlay (PowerShell, NOT git-bash — bash mangles --public-url /frontend)
#    cd ../../Overlays/ingame ; npm run build

# 2. serve dist + mock WebSocket on :9001
node mock-server.js --port 9001 [--state mock-state.json]

# 3. screenshot via Edge headless (Chrome DevTools Protocol)
node shoot.js "http://127.0.0.1:9001/index.html?backend=127.0.0.1" out.png 8000
```

## Contents

- [`mock-server.js`](mock-server.js) — serves `Overlays/ingame/dist` + `/frontend/*`
  assets and runs a WS server at `/api`. Answers the overlay's `OverlayConfig`
  request with the persisted `Config/Ingame.json` merged with
  `config-overrides.json`, then pushes `GameHeartbeat` carrying the chosen mock
  state every 500 ms.
- [`shoot.js`](shoot.js) — launches Edge with `--remote-debugging-port`, waits
  (real time) for WS connect + config + heartbeat + intro animation, then captures
  a **transparent** PNG via CDP `Page.captureScreenshot` (so it composites over the
  reference). `--screenshot` was removed from modern Chromium headless, so CDP is
  required.
- [`config-overrides.json`](config-overrides.json) — harness-only overlay-config
  overrides (enables `PrmScore`: Enabled, BottomBar, tournament name, DDragon
  version) without touching the real config.
- [`mock-state.json`](mock-state.json) — the controlled `StateData` fed to the
  overlay (the broadcast reference values: blue/red 7-9, towers 3/4, a 10-player roster, etc.).
- [`mock-state-empty-infopage.json`](mock-state-empty-infopage.json) — regression
  fixture for the empty-info-page crash documented in
  [`../../docs/feature-completion/DESIGN.md`](../../docs/feature-completion/DESIGN.md)
  (a titled InfoPage with zero tabs).

`ws` is loaded from `Overlays/ingame/node_modules` (no install needed). Transient
output (`out_*.png`, `sidebyside.png`, `edge-profile*/`, `*.log`, ad-hoc `*.py`
measurement scripts such as `scan_api.py`) is gitignored — committed harness files
are `*.js` / `*.json` / `README.md` / `.gitignore`.
