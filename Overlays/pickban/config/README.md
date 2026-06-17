> 🌐 **English** ・ [日本語](README-ja.md)

# Build configuration (ejected Create React App)

Webpack, Babel, Jest and environment configuration for the pick & ban overlay. These files were produced by `react-scripts eject` (the overlay is forked from RCVolus `lol-pick-ban-ui`) and are driven by the entry points in [`../scripts/`](../scripts/).

## Contents

| File | Role |
| --- | --- |
| `webpack.config.js` | Main webpack factory (`development` / `production`); JS/JSX via Babel, LESS via `less-loader`, asset/url loaders, HTML + CSS-extract plugins. |
| `webpackDevServer.config.js` | Dev-server settings (host, HTTPS, watch, history fallback, public folder). |
| `env.js` | Loads `.env*` files and exposes `NODE_ENV`, `PUBLIC_URL` and `REACT_APP_*` vars for webpack's `DefinePlugin`. |
| `paths.js` | Resolves project paths (`src`, `public/index.html`, `build`, `package.json`, etc.) and the module file-extension order. |
| `modules.js` | Derives extra module-resolution paths from `tsconfig.json` / `jsconfig.json` (`baseUrl`). |
| `pnpTs.js` | Yarn PnP + TypeScript module-resolution shim (`ts-pnp`). |

## Subdirectories

| Directory | Purpose |
| --- | --- |
| [`jest/`](jest/) | Jest transformers for CSS and non-JS file imports |
