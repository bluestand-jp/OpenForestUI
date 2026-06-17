> 🌐 **English** ・ [日本語](README-ja.md)

# Shared overlay event base types

The common base classes for the typed messages the app broadcasts to the browser-source overlays over the WebSocket server (`ws://localhost:9001/api`). Concrete events (e.g. `GameHeartbeat`, `GameStart`, `ObjectiveSpawn`) live in the `Ingame.Events` / `ChampSelect.Events` namespaces and serialize using the `eventType` discriminator defined here.

## Contents

| File | Role |
|------|------|
| [`LeagueEvent.cs`](LeagueEvent.cs) | Abstract base for every overlay event. Carries the `eventType` string the overlays switch on; subclasses pass their type name to the constructor. |
| [`OverlayConfig.cs`](OverlayConfig.cs) | Abstract `LeagueEvent` (`eventType = "OverlayConfig"`) carrying a `FrontEndType` so a config push is routed to the correct overlay (ingame vs pickban). |

## Notes

- `FrontEndType` is defined in [`../../Http/IngameWSClient.cs`](../../Http/IngameWSClient.cs). Events are sent via `EmbedIOServer.SocketServer?.SendEventToAllAsync(...)`.
