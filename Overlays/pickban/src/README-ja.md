> 🌐 [English](README.md) ・ **日本語**

# Pick & ban オーバーレイのソース

チャンピオンセレクトオーバーレイを描画する React アプリケーション。エントリポイントは React を起動し、`Window.PB` 経由でバックエンドの WebSocket を購読し、受信した状態を正規化して `europe` ドラフトレイアウトに渡す。

## Contents

| File | Role |
| --- | --- |
| `index.js` | `<App>` を `#root` に描画し、サービスワーカーを登録解除する。 |
| `App.jsx` | `Window.PB` から `newState` / `heartbeat` を購読し、`state`/`config` を保持し、`<Overlay>` を描画する（ゲーム中は非表示）。 |
| `convertState.js` | 各チームのピック／バンをプレースホルダアートで 5 件までパディングし、`/cache/...` の画像 URL を絶対バックエンド URL に書き換える。 |
| `serviceWorker.js` | デフォルトの CRA PWA サービスワーカーヘルパー（登録は無効）。 |
| `index.css` | ページの基本リセットと、赤い `.infoBox`「未接続」バナーのスタイル。 |

## Subdirectories

| Directory | Purpose |
| --- | --- |
| [`assets/`](assets/README-ja.md) | ロゴ、プレースホルダのスプラッシュ／バン SVG、フォント |
| [`assets/fonts/`](assets/fonts/README-ja.md) | オーバーレイで使用する TrueType フォント |
| [`europe/`](europe/README-ja.md) | "europe" ドラフトレイアウトのコンポーネントとスタイル |
| [`europe/style/`](europe/style/README-ja.md) | LESS スタイルとドラフトリビールアニメーション |
