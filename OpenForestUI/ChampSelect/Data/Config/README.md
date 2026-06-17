> 🌐 **English** ・ [日本語](README-ja.md)

# Pickban overlay configuration

The persisted, user-editable configuration for the champion-select overlay. `PickBanConfig` is a `JSONConfig` (saved/loaded by `OpenForestUI.Common`'s config system, file name `PickBan`, version `1.0`) and is exposed to the overlay via `StateData.config` and the periodic `heartbeat` event. It holds broadcast presentation settings: which team is which, names/scores/colors, feature toggles, and optional broadcast top-bar metadata.

## Contents

| File | Role |
| --- | --- |
| `PickBanConfig.cs` | Root config object; `JSONConfig` with serialize/default/version handling. Wraps a single `FrontendConfig`. |
| `FrontendConfig.cs` | Overlay display settings: `scoreEnabled`, `spellsEnabled`, `coachesEnabled`, `blueTeam`/`redTeam` (`TeamConfig`), and `patch`. Ships sensible defaults via `CreateDefaultConfig()`. |
| `TeamConfig.cs` | Per-team broadcast metadata: `name`, `nameTag`, `score`, `coach`, `color`, plus optional broadcast fields `region` (league badge), `seed` (standing), and `flag` (ISO country code). |
