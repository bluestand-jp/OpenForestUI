> 🌐 **English** ・ [日本語](README-ja.md)

# Frontend payload models (sent to the overlay)

The DTOs serialized and pushed over the WebSocket (`ws://localhost:9001/api`) to the ingame browser-source overlay. These are the broadcast-facing shapes — distinct from the raw Riot DTOs in `RIOT/` — and lean heavily on `ShouldSerialize*` gates so each field is emitted only when its overlay feature is enabled (via `IngameController.CurrentSettings` and `ConfigController.Component.Ingame`).

## Contents

- [`FrontEndTeam.cs`](FrontEndTeam.cs) — One team's broadcast stats: name/icon/score, kills/towers/gold/plates/void-grubs/inhibitors, and the top-bar extras `Baron`/`DragonCount` (OCR-sourced, since objective-monster kills aren't in `/eventdata`) plus region/seed/flag metadata. `Dragons` proxies into the team's `dragonsTaken` list for legacy pips.
- [`PlayerScoreboardEntry.cs`](PlayerScoreboardEntry.cs) — One player's row in the bottom comparison scoreboard (10-player roster). Built from a `Player` via `From(...)`: team/position/name/champion/level/KDA/CS, estimated `Gold` (per-player total gold isn't exposed under Vanguard — see `Hub/Team.EstimatePlayerGold`), item-slot IDs, and DataDragon summoner-spell keys.
- [`ScoreboardConfig.cs`](ScoreboardConfig.cs) — Top-level scoreboard payload: blue/red `FrontEndTeam`, game time, series game count, tournament name, and the optional `Players` roster. Player roster + game time serialize only when the custom broadcast scoreboard is on.
- [`LocalFont.cs`](LocalFont.cs) — Tiny `{ Name, Location }` pair describing a locally-served font file for the overlay.
