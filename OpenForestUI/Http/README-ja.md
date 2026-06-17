> 🌐 [English](README.md) ・ **日本語**

# 組み込みオーバーレイ Web/WebSocket サーバー

このディレクトリには、2 つのブラウザソースオーバーレイ（`ingame` と `pickban` の TypeScript オーバーレイ）にデータとアセットを配信する組み込みの HTTP + WebSocket サーバーが含まれる。オーバーレイはライブイベントのために `ws://localhost:9001/api` に接続し、静的バンドルを `/frontend`（ingame）と `/`（pickban）から読み込む。[EmbedIO](https://github.com/unosquare/embedio) 上に構築されている。

## 内容

- **`EmbedIOServer.cs`** — `WebServer` を保持する。`CreateWebServer` が各モジュールを配線する: `/api` の `WSServer`、`/cache` ファイルモジュール、`/debug-ocr` のライブ OCR パイプライン点検ツール（自動ポーリングする HTML ページと、`OcrGoldController.GetDebugJson()` を基にした `/debug-ocr/data` の JSON スナップショット）、そして `ingame`（`/frontend`）と `pickban`（`/`）のバンドル用の静的フォルダ。JSON/HTML は UTF-8 BOM なしで出力される。ネストされた `IngameIndexNoStoreModule` は ingame オーバーレイの `index.html` を `no-store` で配信するため、再デプロイ後に OBS の組み込みブラウザが常に最新の `main.<hash>.js` を再取得する。一方、ハッシュ付きの JS/CSS/画像はキャッシュ対応の静的モジュールへフォールスルーする。
- **`WSServer.cs`** — `/api` の `WebSocketModule`。接続中の `IngameWSClient` を追跡し、`OverlayConfig` ハンドシェイクメッセージを受けるとクライアントを登録して現在の ingame オーバーレイ状態を送信する。チャンピオンセレクトがアクティブなとき新規クライアントへピック＆バンの `NewState` をプッシュし、（Newtonsoft.Json でシリアライズした）`LeagueEvent` を全クライアントへブロードキャストする。`ConfigController.Ingame.ConfigUpdate` 時にクライアントへ config を再プッシュする。
- **`IngameWSClient.cs`** — 1 つの WebSocket 接続と、それが購読した `[Flags]` の `FrontEndType`（`ChampSelect` / `Ingame` / `PostGame`）をラップする。これにより `UpdateFrontEnd` は、クライアントが要求した `type` フラグを持つ `OverlayConfig` だけを転送する。
- **`PickBanConnector.cs`** — チャンピオンセレクト状態（`State` イベント）を WebSocket ブロードキャストへ橋渡しする。2 つのモードをサポートする: **direct**（イベントを即時転送）と **delayed**（`ITickable` でイベントをキューに溜め、放送遅延に合わせて `PickBan.DelayValue` の後にリリースする）。モードは `ConfigController.Component.PickBan.UseDelay` から選択される。delayed パスは決して完了しなかったチャンピオンセレクトのリメイクも抑制する。
