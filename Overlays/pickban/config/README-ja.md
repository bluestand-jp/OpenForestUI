> 🌐 [English](README.md) ・ **日本語**

# ビルド設定（eject 済み Create React App）

pick & ban オーバーレイ向けの Webpack・Babel・Jest・環境設定。これらのファイルは `react-scripts eject` によって生成されたもので（オーバーレイは RCVolus `lol-pick-ban-ui` のフォーク）、[`../scripts/`](../scripts/README-ja.md) のエントリポイントから駆動される。

## Contents

| File | Role |
| --- | --- |
| `webpack.config.js` | メインの webpack ファクトリ（`development` / `production`）。JS/JSX を Babel で、LESS を `less-loader` で処理し、asset/url ローダ、HTML + CSS 抽出プラグインを備える。 |
| `webpackDevServer.config.js` | 開発サーバ設定（host、HTTPS、watch、history フォールバック、public フォルダ）。 |
| `env.js` | `.env*` ファイルを読み込み、webpack の `DefinePlugin` 向けに `NODE_ENV`・`PUBLIC_URL`・`REACT_APP_*` 変数を公開する。 |
| `paths.js` | プロジェクトのパス（`src`、`public/index.html`、`build`、`package.json` など）とモジュールのファイル拡張子の解決順序を解決する。 |
| `modules.js` | `tsconfig.json` / `jsconfig.json`（`baseUrl`）から追加のモジュール解決パスを導出する。 |
| `pnpTs.js` | Yarn PnP + TypeScript のモジュール解決シム（`ts-pnp`）。 |

## Subdirectories

| Directory | Purpose |
| --- | --- |
| [`jest/`](jest/README-ja.md) | CSS および非 JS ファイルのインポート向け Jest トランスフォーマ |
