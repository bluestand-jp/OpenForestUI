> 🌐 **English** ・ [日本語](README-ja.md)

# App core (controllers, config, data, overlay events)

The heart of the OpenForestUI desktop app: the controllers that orchestrate the whole pipeline, the configuration/data layer they read from, and the base types for the events broadcast to the overlays. This is where live game data from the LCU, the spectator Live Client Data API, the Replay API, the Farsight memory reader, and the Python OCR sidecar is ingested, turned into overlay state, and pushed to the browser sources over the embedded WebSocket/HTTP server.

(Note: this is the app's own `Common` namespace, distinct from the standalone `OpenForestUI.Common` library project that holds shared HTTP/DTO/utility code.)

## Subdirectories

| Directory | Purpose |
|-----------|---------|
| [`Controllers/`](Controllers/) | Singletons that drive the app lifecycle and data ingestion: `BroadcastController`, `AppStateController`, `IngameController`, `PickBanController`, `OcrGoldController`, `MockController`, and more. |
| [`Data/`](Data/) | Configuration models + loader (`Config/`) and the Data Dragon static-asset provider (`Provider/`). |
| [`Events/`](Events/) | Base types (`LeagueEvent`, `OverlayConfig`) for the typed messages broadcast to the overlays. |
