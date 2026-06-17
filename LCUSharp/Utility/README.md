> 🌐 **English** ・ [日本語](README-ja.md)

# Client discovery & credentials

Helpers that locate the running League Client and extract the connection credentials needed before any HTTP/WebSocket traffic can flow. `LeagueClientApi` calls both during `ConnectAsync` / `ReconnectAsync`.

## Contents

| File | Role |
| --- | --- |
| [`LeagueProcessHandler.cs`](LeagueProcessHandler.cs) | Polls for the `LeagueClientUx` process, resolves its install directory (`ExecutablePath`), and raises a `Closed` event when the client exits (drives the API's `Disconnected` event). |
| [`LockFileHandler.cs`](LockFileHandler.cs) | Waits for the client `lockfile` (via `FileSystemWatcher` if not yet present), then parses its colon-delimited contents to return the `(port, token)` used for Basic auth and the WebSocket. |
