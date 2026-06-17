> 🌐 **English** ・ [日本語](README-ja.md)

# OpenForestUI.Common — shared low-level library

Cross-cutting code shared by the OpenForestUI desktop app, the Farsight memory reader, and the overlay backends: file logging, HTTP/REST helpers, JSON converters, small utility extensions, and the data models used to describe champions, summoner spells, items, and the game's XP-to-level table. This is a `net6.0` class library (`OpenForestUI.Common.csproj`) with no project dependencies — everything else in the solution references it.

## Contents

- `Log.cs` — Process-wide singleton logger. Buffers messages in a `StringBuilder`, flushes to `Logs/Log-<timestamp>.log` every 5 s, and on `ProcessExit`. Exposes static `Info`/`Warn`/`Verbose`/`Write` gated by a `LogLevel` enum, and writes a `CrashLogs/` report (with system details and a randomized flavor line) on unhandled exceptions.
- `OpenForestUI.Common.csproj` — Library project. References `Newtonsoft.Json`, `System.Text.Json`, and `Microsoft.Win32.Registry`.

## Subdirectories

| Directory | Purpose |
| --- | --- |
| [`Data/`](Data/) | Data models: app-facing DTOs and Riot/Community Dragon static-data types |
| [`Http/`](Http/) | HTTP/REST helpers — REST client, file downloader, text fetch |
| [`Utils/`](Utils/) | Generic extension methods, JSON converters, circular buffer, version parsing |
