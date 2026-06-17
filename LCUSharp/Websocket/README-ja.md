> 🌐 [English](README.md) ・ **日本語**

# LCU ライブイベントストリーム（WebSocket）

WAMP サブプロトコルを用いて `wss://127.0.0.1:<port>/` 経由で League Client のライブイベントフィードを購読する。クライアントは `[8, <uri>, <payload>]` フレームを送出し、この層はそれらをフィルタリングし、ペイロードを `LeagueEvent` にデシリアライズして、URI ごとの購読者にディスパッチする。OpenForestUI では、pick-ban オーバーレイがチャンピオンセレクトの状態変化にリアルタイムで反応するための仕組みとなる。

## 仕組み

接続時に `[5, "OnJsonApiEvent"]` を送信してすべての JSON API イベントを購読し、その後（`Websocket.Client` + System.Reactive 経由で）受信した各 `[...]` メッセージを解析し、クライアントイベントフレーム（`eventNumber == 8`）のみを保持して、グローバルな `MessageReceived` ハンドラと、そのイベントの `Uri` に登録された任意のハンドラの両方を呼び出す。

## 内容

| ファイル | 役割 |
| --- | --- |
| [`ILeagueEventHandler.cs`](ILeagueEventHandler.cs) | 公開インターフェース。`Connect`/`Disconnect`、`ChangeSettings(port, token)`、`MessageReceived` / `ErrorReceived` ハンドラ、およびイベント URI 単位の `Subscribe` / `Unsubscribe` / `UnsubscribeAll`。 |
| [`LeagueEventHandler.cs`](LeagueEventHandler.cs) | 実装。認証済みの `WebsocketClient`（`riot:<token>` 認証情報、`wamp` サブプロトコル、寛容な証明書コールバック）を構築し、フィルタリングされたフレームを URI ごとの購読者リストへルーティングする。 |
| [`LeagueEvent.cs`](LeagueEvent.cs) | 単一のクライアントイベントを表す DTO。`Data`（`JToken`）、`EventType`、`Uri`。`ToString()` は JSON シリアライズ結果を返す。 |
