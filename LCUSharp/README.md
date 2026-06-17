> 🌐 **English** ・ [日本語](README-ja.md)

# LCUSharp — League Client (LCU) API client (vendored)

A small .NET 6 library that connects to the local **League Client Update (LCU) API**: it locates the running `LeagueClientUx` process, reads its `lockfile` for the port and auth token, then exposes a REST request handler, a WebSocket event stream, and a couple of typed endpoints. In OpenForestUI this is the data source for the **champion-select / pick-ban** overlay — `PickBanController` subscribes to `LeagueEvent`s and reads client state through it.

> **Provenance / vendored code.** This directory is a third-party library (the `LCUSharp` project, MIT-licensed) bundled into the OpenForestUI solution as source. Assembly version `1.6.9.x` (see `LCUSharp.csproj`). It is **not** original OpenForestUI work — treat it as an external dependency. It depends on `Newtonsoft.Json` and `Websocket.Client`.

## How it works

1. `LeagueProcessHandler` waits for the `LeagueClientUx` process and resolves its install directory.
2. `LockFileHandler` waits for / parses the client `lockfile` to extract the port and Basic-auth token (`riot:<token>`).
3. `LeagueClientApi.ConnectAsync()` builds an `https://127.0.0.1:<port>/` request handler and a `wss://127.0.0.1:<port>/` event handler, polling `/riotclient/app-name` until the client answers (self-signed TLS cert validation is bypassed).
4. Callers send REST requests via `RequestHandler` and subscribe to live client events via `EventHandler`.

## Contents

| File | Role |
| --- | --- |
| [`ILeagueClientApi.cs`](ILeagueClientApi.cs) | Public facade interface: exposes `RequestHandler`, `EventHandler`, the two endpoints, `Disconnected` event, `ReconnectAsync`/`Disconnect`. |
| [`LeagueClientApi.cs`](LeagueClientApi.cs) | Concrete facade. Static `ConnectAsync()` factory: finds the process, parses the lockfile, wires HTTP + WebSocket, and confirms the connection. |
| [`LCUSharp.csproj`](LCUSharp.csproj) | net6.0 library project; references Newtonsoft.Json and Websocket.Client. |

## Subdirectories

| Directory | Purpose |
| --- | --- |
| [`Http/`](Http/) | REST request handler and authentication against the LCU HTTPS API. |
| [`Utility/`](Utility/) | Process discovery and lockfile (port + token) parsing. |
| [`Websocket/`](Websocket/) | WAMP-style WebSocket subscription to live client events. |
