> 🌐 [English](README.md) ・ **日本語**

# Champion-select (pick & ban) オーバーレイ

大会放送向けに League of Legends のチャンピオンセレクト／ドラフト段階を描画するブラウザソース用オーバーレイ。React（Create React App、eject 済み）のシングルページアプリで、WebSocket 経由で OpenForestUI の C# バックエンドに接続し、チャンピオンセレクトの状態を受信して、ピック・バン・サモナースペル・チーム名／スコア・ドラフトタイマーを描画する。

このオーバーレイのデータは League Client (LCU) API を起点とする。OpenForestUI デスクトップアプリが `OpenForestUI/ChampSelect/*` でチャンピオンセレクトを監視し、組み込み WebSocket サーバ（`ws://localhost:9001/api`）上の [`PickBanConnector`](../../OpenForestUI/Http/PickBanConnector.cs) を通じてイベントをプッシュする。オーバーレイの HTML/JS は `/frontend` から配信される。

## Provenance

このチャンピオンセレクトオーバーレイは **RCVolus** [`lol-pick-ban-ui`](https://github.com/RCVolus/lol-pick-ban-ui)（`pick-ban-overlay` パッケージ）のフォーク／統合である。eject 済みの CRA ビルドツールと、同プロジェクト由来の "europe" ドラフトレイアウトをそのまま継承している。本リポジトリの MIT ライセンスの下で配布される。

## 接続のしくみ

- `public/frontend-lib.js` はグローバルな `Window.PB` イベントエミッタを公開し、WebSocket を開き（バックエンド URL は `?backend=` クエリ文字列から取得）、自動再接続し、各メッセージを `eventType` をキーに再送出する。
- `src/App.jsx` は `newState`（完全なチャンピオンセレクト状態）と `heartbeat`（config のみ）を購読し、ペイロードを `convertState.js` に通して `<Overlay>` を描画する。
- `convertState.js` は各チームのピック／バンをプレースホルダのスプラッシュ／バンアートで 5 件までパディングし、`/cache/...` の画像 URL を絶対 URL（`http(s)://<backend>`）に書き換える。

## Contents

| File | Role |
| --- | --- |
| [`src/`](src/README-ja.md) | React アプリのソース（App、状態変換、`europe` ドラフトレイアウト） |
| [`public/`](public/README-ja.md) | 静的 HTML テンプレート、`frontend-lib.js` WS クライアント、`robots.txt` |
| [`config/`](config/README-ja.md) | eject 済み CRA の webpack/babel/jest/env 設定 |
| [`scripts/`](scripts/README-ja.md) | eject 済み CRA の `start` / `build` / `test` エントリスクリプト |
| `package.json` | パッケージ `pick-ban-overlay`。スクリプトは `scripts/{start,build,test}.js` を実行 |
| `.env` | `REACT_APP_LCSU_BACKEND` のデフォルトバックエンドホスト |
| `installPB.bat` / `runFrontend.bat` | `npm install` / `npm start` の簡易ラッパー |

## Subdirectories

| Directory | Purpose |
| --- | --- |
| [`config/`](config/README-ja.md) | eject 済み Create React App のビルド設定 |
| [`config/jest/`](config/jest/README-ja.md) | Jest の CSS／ファイルトランスフォーマ |
| [`public/`](public/README-ja.md) | 静的アセットと WebSocket クライアントライブラリ |
| [`scripts/`](scripts/README-ja.md) | CRA の start/build/test スクリプト |
| [`src/`](src/README-ja.md) | React アプリケーションのソース |
| [`src/assets/`](src/assets/README-ja.md) | ロゴ、プレースホルダのスプラッシュ／バンアート、フォント |
| [`src/assets/fonts/`](src/assets/fonts/README-ja.md) | オーバーレイで使用する Web フォント |
| [`src/europe/`](src/europe/README-ja.md) | "europe" ドラフトレイアウトのコンポーネント |
| [`src/europe/style/`](src/europe/style/README-ja.md) | LESS スタイルとドラフトリビールアニメーション |

## ビルド／実行

```
npm install        # または installPB.bat
npm start          # 開発サーバ（port 3000）— runFrontend.bat
npm run build      # 本番バンドルを build/ に出力
```

`npm run build` の出力が、OpenForestUI バックエンドが放送／OBS のブラウザソースに配信するものである。
