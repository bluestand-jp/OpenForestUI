> 🌐 **English** ・ [日本語](README-ja.md)

# Persisted JSON configuration (schema + loader)

The app's settings layer. Defines the strongly-typed config models that serialize to/from the `Config/*.json` files on disk, plus the provider that reads, version-migrates, and writes them. `ConfigController` (in [`../../Controllers`](../../Controllers)) owns the live instances and hot-reloads them via `FileSystemWatcher`.

## Contents

| File | Role |
|------|------|
| [`JSONConfig.cs`](JSONConfig.cs) | Abstract base for every config file. Declares the contract — `Name`, `FileVersion`, serialize/deserialize, `RevertToDefault`, `UpdateConfigVersion` — plus `Reload()` and a `ConfigUpdate` event for hot-reload subscribers. |
| [`JSONConfigProvider.cs`](JSONConfigProvider.cs) | Singleton that reads/writes config files under `./Config` (creating the folder and `Teams/` subfolder as needed), restores defaults on a missing/empty/corrupt file, and runs version migration when `FileVersion` is stale. Also handles per-team config files (`ReadTeam`/`WriteTeam`). |
| [`ComponentConfig.cs`](ComponentConfig.cs) | The main `Component.json` schema (current version `1.6`). Nested sections: `DataDragon`, `PickBan`, `Ingame` (objectives, team-info toggles, `ObjectiveTimingsConfig` patch timings, `UseMemoryReader`, `TournamentName`), `Replay`, `PostGame`, and `App` (log level, frontend port, League install paths). Carries the default values for a fresh install. |
| [`ExtendedTeamConfig.cs`](ExtendedTeamConfig.cs) | Per-team saved config (`Config/Teams/<TeamName>.json`): a `TeamConfig` plus the team's icon location. Used by the Saved Teams feature; has no "default" so `RevertToDefault`/`GETDefaultString` throw. |

## Notes

- `ObjectiveTimingsConfig` holds objective spawn/respawn times (seconds of game time) as **patch data**, not code, so a season retune is a `Component.json` edit. Defaults track patch 26.x.
- `IngameComponentConfig.UseMemoryReader` defaults `false` (Vanguard-safe); offsets (`Farsight.json`) are only loaded when it is on.
