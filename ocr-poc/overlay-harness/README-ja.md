> 🌐 [English](README.md) ・ **日本語**

# オーバーレイレンダー検証ハーネス

完全に制御された state に対して ingame オーバーレイをレンダリングし、ヘッドレスで
（ライブゲーム無しで）スクリーンショットを撮ることで、放送オーバーレイを要素ごとに
リファレンスフレームと比較できるようにする — 放送用トップ/下部バー vs
[`../../docs/prm-overlay/prm_reference.png`](../../docs/prm-overlay/prm_reference.png)、
比較スコアボード vs
[`../../docs/lck-scoreboard/lck_reference.png`](../../docs/lck-scoreboard/lck_reference.png)。
[`../../docs/prm-overlay/SPEC.md`](../../docs/prm-overlay/SPEC.md) §8 と
[`../../docs/lck-scoreboard/SPEC.md`](../../docs/lck-scoreboard/SPEC.md) §9 を参照。

## 使い方

```sh
# 1. オーバーレイをビルド（PowerShell、git-bash は不可 — bash は --public-url /frontend を壊す）
#    cd ../../Overlays/ingame ; npm run build

# 2. dist + モック WebSocket を :9001 で serve
node mock-server.js --port 9001 [--state mock-state.json]

# 3. Edge ヘッドレス（Chrome DevTools Protocol）でスクリーンショット
node shoot.js "http://127.0.0.1:9001/index.html?backend=127.0.0.1" out.png 8000
```

## 内容

- [`mock-server.js`](mock-server.js) — `Overlays/ingame/dist` + `/frontend/*` アセットを
  serve し、`/api` で WS サーバを実行する。オーバーレイの `OverlayConfig` リクエストに、
  永続化された `Config/Ingame.json` を `config-overrides.json` とマージして応答し、
  続いて選択されたモック state を載せた `GameHeartbeat` を 500 ms ごとに push する。
- [`shoot.js`](shoot.js) — `--remote-debugging-port` 付きで Edge を起動し、WS 接続 +
  config + heartbeat + イントロアニメーションを（リアルタイムで）待ってから、CDP
  `Page.captureScreenshot` で **透過** PNG をキャプチャする（リファレンスの上に合成
  できるように）。`--screenshot` はモダンな Chromium ヘッドレスから削除されたため、
  CDP が必須。
- [`config-overrides.json`](config-overrides.json) — ハーネス専用のオーバーレイ config
  オーバーライド（実 config に触れずに `PrmScore`: Enabled、BottomBar、トーナメント名、
  DDragon バージョンを有効化）。
- [`mock-state.json`](mock-state.json) — オーバーレイに供給される制御された
  `StateData`（放送リファレンス値: blue/red 7-9、towers 3/4、10人ロスター、など）。
- [`mock-state-empty-infopage.json`](mock-state-empty-infopage.json) —
  [`../../docs/feature-completion/DESIGN.md`](../../docs/feature-completion/DESIGN.md)
  に記録された empty-info-page クラッシュ（タブが 0 個のタイトル付き InfoPage）の
  回帰フィクスチャ。

`ws` は `Overlays/ingame/node_modules` からロードされる（インストール不要）。一時的な
出力（`out_*.png`, `sidebyside.png`, `edge-profile*/`, `*.log`, `scan_api.py` のような
アドホックな `*.py` 計測スクリプト）は gitignore 対象 — コミットされるハーネスファイルは
`*.js` / `*.json` / `README.md` / `.gitignore`。
