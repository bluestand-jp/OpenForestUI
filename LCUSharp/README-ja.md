> 🌐 [English](README.md) ・ **日本語**

# LCUSharp — League Client (LCU) API クライアント（vendored）

ローカルの **League Client Update (LCU) API** に接続する小規模な .NET 6 ライブラリ。実行中の `LeagueClientUx` プロセスを特定し、その `lockfile` からポートと認証トークンを読み取り、REST リクエストハンドラ・WebSocket イベントストリーム・いくつかの型付きエンドポイントを公開する。OpenForestUI では **champion-select / pick-ban** オーバーレイのデータソースとなり、`PickBanController` が `LeagueEvent` を購読してこのライブラリ経由でクライアント状態を読み取る。

> **出自 / vendored コード。** このディレクトリはサードパーティライブラリ（`LCUSharp` プロジェクト、MIT ライセンス）であり、OpenForestUI ソリューションにソースとして同梱されている。アセンブリバージョンは `1.6.9.x`（`LCUSharp.csproj` 参照）。これは OpenForestUI 独自の成果物では **ない** ため、外部依存として扱うこと。`Newtonsoft.Json` と `Websocket.Client` に依存する。

## 仕組み

1. `LeagueProcessHandler` が `LeagueClientUx` プロセスを待ち受け、そのインストールディレクトリを解決する。
2. `LockFileHandler` がクライアントの `lockfile` を待機／解析し、ポートと Basic 認証トークン（`riot:<token>`）を抽出する。
3. `LeagueClientApi.ConnectAsync()` が `https://127.0.0.1:<port>/` リクエストハンドラと `wss://127.0.0.1:<port>/` イベントハンドラを構築し、クライアントが応答するまで `/riotclient/app-name` をポーリングする（自己署名 TLS 証明書の検証はバイパスされる）。
4. 呼び出し側は `RequestHandler` 経由で REST リクエストを送信し、`EventHandler` 経由でライブクライアントイベントを購読する。

## 内容

| ファイル | 役割 |
| --- | --- |
| [`ILeagueClientApi.cs`](ILeagueClientApi.cs) | 公開ファサードインターフェース。`RequestHandler`、`EventHandler`、2 つのエンドポイント、`Disconnected` イベント、`ReconnectAsync`/`Disconnect` を公開。 |
| [`LeagueClientApi.cs`](LeagueClientApi.cs) | 具象ファサード。静的 `ConnectAsync()` ファクトリ。プロセスを検出し、lockfile を解析し、HTTP + WebSocket を結線し、接続を確認する。 |
| [`LCUSharp.csproj`](LCUSharp.csproj) | net6.0 ライブラリプロジェクト。Newtonsoft.Json と Websocket.Client を参照。 |

## サブディレクトリ

| ディレクトリ | 目的 |
| --- | --- |
| [`Http/`](Http/README-ja.md) | LCU HTTPS API に対する REST リクエストハンドラと認証。 |
| [`Utility/`](Utility/README-ja.md) | プロセス探索と lockfile（ポート + トークン）の解析。 |
| [`Websocket/`](Websocket/README-ja.md) | ライブクライアントイベントへの WAMP スタイル WebSocket 購読。 |
