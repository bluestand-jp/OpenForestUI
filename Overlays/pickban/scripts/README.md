> 🌐 **English** ・ [日本語](README-ja.md)

# CRA entry scripts

The `start` / `build` / `test` entry points for the pick & ban overlay, referenced by the `scripts` block in [`../package.json`](../package.json). Ejected from Create React App; they set `NODE_ENV`, load [`../config/env.js`](../config/env.js), and invoke webpack/jest with the configs in [`../config/`](../config/).

## Contents

| File | Role |
| --- | --- |
| `start.js` | `npm start` — runs `webpack-dev-server` in development on port 3000 (browser auto-open disabled). |
| `build.js` | `npm run build` — produces the optimized production bundle in `build/` and copies `public/`. |
| `test.js` | `npm test` — runs Jest (watch mode unless on CI or `--watchAll`). |
