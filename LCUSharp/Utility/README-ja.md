> 🌐 [English](README.md) ・ **日本語**

# クライアント探索 & 認証情報

実行中の League Client を特定し、HTTP/WebSocket 通信を開始する前に必要となる接続用認証情報を抽出するヘルパー群。`LeagueClientApi` は `ConnectAsync` / `ReconnectAsync` の際にこれら両方を呼び出す。

## 内容

| ファイル | 役割 |
| --- | --- |
| [`LeagueProcessHandler.cs`](LeagueProcessHandler.cs) | `LeagueClientUx` プロセスをポーリングし、そのインストールディレクトリ（`ExecutablePath`）を解決し、クライアント終了時に `Closed` イベントを発火する（API の `Disconnected` イベントを駆動）。 |
| [`LockFileHandler.cs`](LockFileHandler.cs) | クライアントの `lockfile` を待機し（まだ存在しない場合は `FileSystemWatcher` 経由）、コロン区切りの内容を解析して、Basic 認証と WebSocket に使う `(port, token)` を返す。 |
