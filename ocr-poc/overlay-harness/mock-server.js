// Verification harness for the ingame overlay.
//
// Serves the built overlay (Overlays/ingame/dist) + its /frontend assets on a
// single port, answers the overlay's `OverlayConfig` request with the persisted
// Ingame.json, and pushes a `GameHeartbeat` carrying a state we fully control —
// so we can render the overlay against the exact PRM reference values and
// screenshot it headlessly (Edge --headless --screenshot), with no live game.
//
//   node mock-server.js [--port 9001] [--state mock-state.json]
//
// Then:
//   msedge --headless=new --screenshot=out.png --window-size=1920,1080 \
//     "http://127.0.0.1:9001/index.html?backend=127.0.0.1"
//
// ws is loaded from the overlay's own node_modules (no install needed).

const http = require('http');
const fs = require('fs');
const path = require('path');

const ROOT = path.resolve(__dirname, '..', '..');
const DIST = path.join(ROOT, 'Overlays', 'ingame', 'dist');
const CONFIG_JSON = path.join(ROOT, 'OpenForestUI', 'bin', 'Release', 'net6.0-windows', 'Config', 'Ingame.json');
const WS = require(path.join(ROOT, 'Overlays', 'ingame', 'node_modules', 'ws'));

function arg(name, def) {
  const i = process.argv.indexOf('--' + name);
  return i >= 0 && process.argv[i + 1] ? process.argv[i + 1] : def;
}
const PORT = parseInt(arg('port', '9001'), 10);
const STATE_FILE = arg('state', path.join(__dirname, 'mock-state.json'));

function loadJson(p) {
  // tolerate UTF-8 BOM (the C# configs are written with one)
  return JSON.parse(fs.readFileSync(p, 'utf8').replace(/^﻿/, ''));
}

const overlayConfig = loadJson(CONFIG_JSON);
// merge harness-local overrides (e.g. enable PrmScore) without touching the real config
const OVERRIDES = path.join(__dirname, 'config-overrides.json');
if (fs.existsSync(OVERRIDES)) {
  Object.assign(overlayConfig, loadJson(OVERRIDES));
  console.log('[harness] merged config-overrides.json:', Object.keys(loadJson(OVERRIDES)).join(','));
}

const MIME = {
  '.html': 'text/html', '.js': 'application/javascript', '.css': 'text/css',
  '.png': 'image/png', '.jpg': 'image/jpeg', '.svg': 'image/svg+xml',
  '.mp4': 'video/mp4', '.json': 'application/json', '.map': 'application/json',
  '.ttf': 'font/ttf', '.otf': 'font/otf', '.woff': 'font/woff', '.woff2': 'font/woff2',
};

function serveFile(res, file) {
  fs.readFile(file, (err, buf) => {
    if (err) { res.writeHead(404); res.end('not found: ' + file); return; }
    res.writeHead(200, { 'Content-Type': MIME[path.extname(file).toLowerCase()] || 'application/octet-stream' });
    res.end(buf);
  });
}

const server = http.createServer((req, res) => {
  const u = decodeURIComponent(req.url.split('?')[0]);
  // /cache/fonts : the overlay XHRs this for remote font discovery; empty 200 is fine.
  if (u === '/cache/fonts' || u === '/cache/fonts/') {
    res.writeHead(200, { 'Content-Type': 'text/html' });
    res.end('<html><body></body></html>');
    return;
  }
  // /frontend/* -> dist/*   (bundle js/css + copied static asset dirs)
  let rel;
  if (u.startsWith('/frontend/')) rel = u.slice('/frontend/'.length);
  else if (u === '/' || u === '' || u === '/index.html') rel = 'index.html';
  else rel = u.replace(/^\//, '');
  // case-insensitive 'Masks' vs 'masks' tolerance
  let file = path.join(DIST, rel);
  if (!fs.existsSync(file)) {
    const alt = path.join(DIST, rel.replace(/Masks/g, 'masks').replace(/masks/g, 'masks'));
    if (fs.existsSync(alt)) file = alt;
  }
  serveFile(res, file);
});

// ---- WebSocket at /api ----
const wss = new WS.Server({ server, path: '/api' });

function buildState() {
  const s = loadJson(STATE_FILE);
  return { eventType: 'GameHeartbeat', stateData: s };
}

wss.on('connection', (ws) => {
  console.log('[harness] overlay connected');
  let heartbeat = null;
  ws.on('message', (raw) => {
    let msg;
    try { msg = JSON.parse(raw.toString()); } catch { return; }
    if (msg.requestType === 'OverlayConfig') {
      console.log('[harness] -> OverlayConfig');
      ws.send(JSON.stringify({ eventType: 'OverlayConfig', type: 1, config: overlayConfig }));
      // start pushing heartbeats shortly after config is applied
      setTimeout(() => {
        const send = () => { try { ws.send(JSON.stringify(buildState())); } catch {} };
        send();
        heartbeat = setInterval(send, 500);
      }, 1500);
    }
  });
  ws.on('close', () => { if (heartbeat) clearInterval(heartbeat); console.log('[harness] overlay disconnected'); });
});

server.listen(PORT, '127.0.0.1', () => {
  console.log(`[harness] serving ${DIST}`);
  console.log(`[harness] http+ws on http://127.0.0.1:${PORT}  (config: ${path.basename(CONFIG_JSON)}, state: ${path.basename(STATE_FILE)})`);
});
