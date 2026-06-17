> 🌐 **English** ・ [日本語](README-ja.md)

# Embedded overlay web/WebSocket server

This directory contains the embedded HTTP + WebSocket server that delivers data and assets to the two
browser-source overlays (the `ingame` and `pickban` TypeScript overlays). Overlays connect to
`ws://localhost:9001/api` for live events and load their static bundles from `/frontend` (ingame) and
`/` (pickban). Built on [EmbedIO](https://github.com/unosquare/embedio).

## Contents

- **`EmbedIOServer.cs`** — owns the `WebServer`. `CreateWebServer` wires the modules: the
  `WSServer` at `/api`; a `/cache` file module; the `/debug-ocr` live OCR-pipeline inspector
  (an auto-polling HTML page plus a `/debug-ocr/data` JSON snapshot sourced from
  `OcrGoldController.GetDebugJson()`); and static folders for the `ingame` (`/frontend`) and
  `pickban` (`/`) bundles. JSON/HTML are emitted without a UTF-8 BOM. The nested
  `IngameIndexNoStoreModule` serves the ingame overlay's `index.html` with `no-store` so OBS's
  embedded browser always refetches the latest `main.<hash>.js` after a redeploy, while hashed
  JS/CSS/images fall through to the cached static module.
- **`WSServer.cs`** — the `/api` `WebSocketModule`. Tracks connected `IngameWSClient`s; on the
  `OverlayConfig` handshake message it registers the client and sends the current ingame overlay
  state; pushes pick&ban `NewState` to new clients when champ select is active; and broadcasts
  `LeagueEvent`s (serialized via Newtonsoft.Json) to all clients. Re-pushes config to clients on
  `ConfigController.Ingame.ConfigUpdate`.
- **`IngameWSClient.cs`** — wraps one WebSocket connection plus the `[Flags]` `FrontEndType`
  (`ChampSelect` / `Ingame` / `PostGame`) it subscribed to, so `UpdateFrontEnd` only forwards an
  `OverlayConfig` whose `type` flag the client requested.
- **`PickBanConnector.cs`** — bridges champion-select state (`State` events) to the WebSocket
  broadcast. Supports two modes: **direct** (events forwarded immediately) and **delayed**, an
  `ITickable` that queues events and releases them after `PickBan.DelayValue` to match a broadcast
  delay. Mode is chosen from `ConfigController.Component.PickBan.UseDelay`; the delayed path also
  suppresses never-completed champ-select remakes.
