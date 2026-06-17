// Headless screenshot via Chrome DevTools Protocol (no puppeteer needed).
// Launches Edge headless with remote debugging, navigates to the overlay,
// waits (real time) for WS connect + config + heartbeat + intro animation,
// then captures a TRANSPARENT png so it can be composited over the reference.
//
//   node shoot.js <url> <out.png> [waitMs] [debugPort]

const http = require('http');
const path = require('path');
const fs = require('fs');
const { spawn } = require('child_process');
const ROOT = path.resolve(__dirname, '..', '..');
const WS = require(path.join(ROOT, 'Overlays', 'ingame', 'node_modules', 'ws'));

const URL_ = process.argv[2] || 'http://127.0.0.1:9001/index.html?backend=127.0.0.1';
const OUT = process.argv[3] || path.join(__dirname, 'out.png');
const WAIT = parseInt(process.argv[4] || '9000', 10);
const PORT = parseInt(process.argv[5] || '9333', 10);

const EDGE = 'C:\\Program Files (x86)\\Microsoft\\Edge\\Application\\msedge.exe';

function httpJson(url) {
  return new Promise((resolve, reject) => {
    http.get(url, (res) => {
      let d = ''; res.on('data', c => d += c); res.on('end', () => {
        try { resolve(JSON.parse(d)); } catch (e) { resolve(d); }
      });
    }).on('error', reject);
  });
}
const sleep = ms => new Promise(r => setTimeout(r, ms));

(async () => {
  const profile = path.join(__dirname, 'edge-profile-' + PORT);
  const edge = spawn(EDGE, [
    '--headless=new', '--enable-unsafe-swiftshader',
    '--use-gl=angle', '--use-angle=swiftshader', '--in-process-gpu',
    '--hide-scrollbars', '--force-device-scale-factor=1',
    '--no-first-run', '--no-default-browser-check',
    `--remote-debugging-port=${PORT}`, '--remote-allow-origins=*',
    `--user-data-dir=${profile}`,
    '--window-size=1920,1080',
    URL_,
  ], { stdio: 'ignore' });

  // wait for the devtools endpoint
  let target = null;
  for (let i = 0; i < 60; i++) {
    await sleep(250);
    try {
      const list = await httpJson(`http://127.0.0.1:${PORT}/json`);
      if (Array.isArray(list)) {
        target = list.find(t => t.type === 'page' && t.webSocketDebuggerUrl);
        if (target) break;
      }
    } catch {}
  }
  if (!target) { console.error('no devtools page target'); edge.kill(); process.exit(2); }

  const ws = new WS(target.webSocketDebuggerUrl, { headers: { Origin: `http://127.0.0.1:${PORT}` } });
  let id = 0; const pending = new Map();
  const send = (method, params = {}) => new Promise((resolve) => {
    const mid = ++id; pending.set(mid, resolve);
    ws.send(JSON.stringify({ id: mid, method, params }));
  });
  ws.on('message', (raw) => {
    const m = JSON.parse(raw.toString());
    if (m.id && pending.has(m.id)) { pending.get(m.id)(m.result); pending.delete(m.id); }
  });
  await new Promise(r => ws.on('open', r));

  // collect console + page errors for diagnosis
  const logs = [];
  ws.on('message', (raw) => {
    const m = JSON.parse(raw.toString());
    if (m.method === 'Runtime.consoleAPICalled') {
      logs.push('[console] ' + (m.params.args || []).map(a => a.value ?? a.description ?? a.type).join(' '));
    } else if (m.method === 'Log.entryAdded') {
      logs.push('[log] ' + m.params.entry.level + ': ' + m.params.entry.text);
    } else if (m.method === 'Runtime.exceptionThrown') {
      logs.push('[EXC] ' + (m.params.exceptionDetails.exception?.description || m.params.exceptionDetails.text));
    }
  });
  await send('Page.enable');
  await send('Runtime.enable');
  await send('Log.enable');
  // force exact viewport dimensions
  await send('Emulation.setDeviceMetricsOverride', { width: 1920, height: 1080, deviceScaleFactor: 1, mobile: false });
  // transparent backdrop so we can composite over the reference frame
  await send('Emulation.setDefaultBackgroundColorOverride', { color: { r: 0, g: 0, b: 0, a: 0 } });
  // give the overlay real time to: connect WS, fetch config, get heartbeat, animate in
  await sleep(WAIT);
  // probe WebGL + Phaser state
  const probe = await send('Runtime.evaluate', { expression: `(function(){var c=document.querySelector('canvas');var gl=c&&(c.getContext('webgl2')||c.getContext('webgl'));return JSON.stringify({hasCanvas:!!c,cw:c&&c.width,ch:c&&c.height,gl:!!gl,glVendor:gl&&gl.getParameter(gl.VERSION)});})()`, returnByValue: true });
  console.log('PROBE', probe && probe.result && probe.result.value);
  console.log('LOGS:\n' + logs.join('\n'));
  const res = await send('Page.captureScreenshot', { format: 'png', captureBeyondViewport: false });
  fs.writeFileSync(OUT, Buffer.from(res.data, 'base64'));
  console.log('wrote', OUT, fs.statSync(OUT).size, 'bytes');
  ws.close(); edge.kill();
  process.exit(0);
})().catch(e => { console.error(e); process.exit(1); });
