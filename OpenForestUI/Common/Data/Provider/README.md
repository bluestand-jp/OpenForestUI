> 🌐 **English** ・ [日本語](README-ja.md)

# Static game-asset provider (Data Dragon / Community Dragon)

Fetches and caches the static League assets the overlays need — champion squares, item and summoner-spell icons, and the current game version — from Riot's Data Dragon and Community Dragon CDNs. Runs early in startup; the rest of the app waits on `DataDragon.FinishLoading` before building controllers.

## Contents

| File | Role |
|------|------|
| [`DataDragon.cs`](DataDragon.cs) | Singleton asset provider. Resolves the latest game version, verifies the local `./Cache/<version>/` against the CDN, downloads any missing champion/item/summoner-spell assets (reporting progress via `FileDownloadComplete` to the startup splash), and exposes the resolved `GameVersion` (CDN URLs, patch). `Extend*` helpers enrich Community Dragon champion/item/spell records with cached local paths. |

## Notes

- Cache lives under `./Cache/<version>/` next to the executable; CDN endpoints come from `ComponentConfig.DataDragonConfig` (CDN, CDragonRaw, locale, patch). `DataDragon.version` is the global source of truth for the active patch when the LCU patch string is unavailable.
