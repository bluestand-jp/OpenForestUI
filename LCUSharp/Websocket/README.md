> 🌐 **English** ・ [日本語](README-ja.md)

# LCU live event stream (WebSocket)

Subscribes to the League Client's live event feed over `wss://127.0.0.1:<port>/` using the WAMP sub-protocol. The client emits `[8, <uri>, <payload>]` frames; this layer filters for those, deserializes the payload into a `LeagueEvent`, and dispatches it to per-URI subscribers. In OpenForestUI this is how the pick-ban overlay reacts to champion-select state changes in real time.

## How it works

On connect it sends `[5, "OnJsonApiEvent"]` to subscribe to all JSON API events, then (via `Websocket.Client` + System.Reactive) parses each incoming `[...]` message, keeps only client-event frames (`eventNumber == 8`), and invokes both the global `MessageReceived` handler and any handlers registered for that event's `Uri`.

## Contents

| File | Role |
| --- | --- |
| [`ILeagueEventHandler.cs`](ILeagueEventHandler.cs) | Public interface: `Connect`/`Disconnect`, `ChangeSettings(port, token)`, the `MessageReceived` / `ErrorReceived` handlers, and `Subscribe` / `Unsubscribe` / `UnsubscribeAll` by event URI. |
| [`LeagueEventHandler.cs`](LeagueEventHandler.cs) | Implementation. Builds the authenticated `WebsocketClient` (`riot:<token>` credentials, `wamp` sub-protocol, permissive cert callback), routes filtered frames to per-URI subscriber lists. |
| [`LeagueEvent.cs`](LeagueEvent.cs) | DTO for a single client event: `Data` (`JToken`), `EventType`, and `Uri`. `ToString()` returns the JSON serialization. |
