> 🌐 **English** ・ [日本語](README-ja.md)

# Champion-select data models

The data layer of the champion-select pipeline. It separates three concerns: the **raw shapes** deserialized from the League Client (LCU) API, the **normalized shapes** sent to the overlay, and the **persisted configuration** for the pickban overlay.

`StateInfo/Converter` reads the `LCU/` types and produces `DTO/` types; the resulting `DTO/Team` and the `Config/PickBanConfig` are what the overlay actually receives over the WebSocket.

## Subdirectories

| Directory | Purpose |
| --- | --- |
| [`LCU/`](LCU/) | Raw League Client champ-select DTOs (deserialized from LCU JSON) |
| [`DTO/`](DTO/) | Normalized pick/ban/team/version models sent to the overlay |
| [`Config/`](Config/) | Persisted pickban overlay config (teams, scores, toggles, broadcast metadata) |
