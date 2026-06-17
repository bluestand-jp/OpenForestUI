> 🌐 **English** ・ [日本語](README-ja.md)

# Typed LCU endpoints

Strongly-typed wrappers over individual League Client REST routes. Each endpoint takes an `ILeagueRequestHandler` and turns C# method calls into the corresponding HTTP requests, hiding the relative URLs and query parameters from callers.

## Contents

| File | Role |
| --- | --- |
| [`EndpointBase.cs`](EndpointBase.cs) | `internal abstract` base holding the shared `ILeagueRequestHandler` used by every endpoint. |

## Subdirectories

| Directory | Purpose |
| --- | --- |
| [`ProcessControl/`](ProcessControl/) | `process-control/v1/process/*` — quit, restart, restart-to-repair / restart-to-update. |
| [`RiotClient/`](RiotClient/) | `riotclient/*` — show/minimize/flash/kill/launch the client UX and get/set the UX zoom scale. |
