> 🌐 **English** ・ [日本語](README-ja.md)

# LCU HTTP request layer

The REST plumbing for talking to the League Client over `https://127.0.0.1:<port>/`. It builds an authenticated `HttpClient` (Basic `riot:<token>`, self-signed cert accepted), serializes/deserializes JSON via Newtonsoft.Json, and assembles relative URLs with query strings. Typed wrappers over specific client routes live under [`Endpoints/`](Endpoints/).

## Contents

| File | Role |
| --- | --- |
| [`RequestHandler.cs`](RequestHandler.cs) | `internal abstract` base. Creates the `HttpClient` with manual cert handling + a permissive cert-validation callback, builds query-parameter strings, prepares `HttpRequestMessage`s, and reads response bodies. |
| [`LeagueRequestHandler.cs`](LeagueRequestHandler.cs) | Concrete handler. Sets the `Basic riot:<token>` header and `BaseAddress`; `ChangeSettings(port, token)` rebuilds the client on reconnect. Sends requests and (optionally) deserializes the JSON response into a typed object. |
| [`ILeagueRequestHandler.cs`](ILeagueRequestHandler.cs) | Public interface exposing `Port`, `Token`, `ChangeSettings`, and the `GetJsonResponseAsync` / `GetResponseAsync<T>` request methods. |

## Subdirectories

| Directory | Purpose |
| --- | --- |
| [`Endpoints/`](Endpoints/) | Typed wrappers over specific LCU routes (riotclient UX control, process control). |
