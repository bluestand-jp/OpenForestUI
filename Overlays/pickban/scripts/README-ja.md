> 🌐 [English](README.md) ・ **日本語**

# CRA エントリスクリプト

pick & ban オーバーレイの `start` / `build` / `test` エントリポイント。[`../package.json`](../package.json) の `scripts` ブロックから参照される。Create React App から eject されたもので、`NODE_ENV` を設定し、[`../config/env.js`](../config/env.js) を読み込み、[`../config/`](../config/README-ja.md) の設定で webpack/jest を起動する。

## Contents

| File | Role |
| --- | --- |
| `start.js` | `npm start` — development で `webpack-dev-server` を port 3000 で起動する（ブラウザの自動オープンは無効）。 |
| `build.js` | `npm run build` — 最適化された本番バンドルを `build/` に生成し、`public/` をコピーする。 |
| `test.js` | `npm test` — Jest を実行する（CI 上または `--watchAll` でない限り watch モード）。 |
