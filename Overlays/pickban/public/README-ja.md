> 🌐 [English](README.md) ・ **日本語**

# 静的 public アセット

本番の `build/`（CRA の `public/` フォルダ）へそのままコピーされるファイル。HTML シェルと、オーバーレイを OpenForestUI バックエンドにつなぐ WebSocket クライアントを保持する。

## Contents

| File | Role |
| --- | --- |
| `index.html` | HTML テンプレート（`<div id="root">`、タイトル "LoL CS UI"、Google Web フォントリンク）。React バンドルより前に `frontend-lib.js` を読み込む。 |
| `frontend-lib.js` | グローバルな `Window.PB` を定義する — イベントエミッタ型の WebSocket クライアント。`PB.start()` でバックエンド（`?backend=` クエリ変数）に接続し、500 ms ごとに自動再接続し、メッセージを `eventType`（例: `newState`、`heartbeat`）ごとに送出し、`/cache` 画像 URL 向けに `PB.toAbsoluteUrl()` を公開する。[`../src/App.jsx`](../src/App.jsx) から利用される。 |
| `robots.txt` | デフォルトの「何も disallow しない」robots ファイル。 |
