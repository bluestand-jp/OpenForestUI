> 🌐 **English** ・ [日本語](README-ja.md)

# Normalized champ-select DTOs (overlay-facing)

The cleaned, overlay-facing draft model produced by `StateInfo/Converter` from the raw `LCU/` types. These are what the pickban overlay consumes: each team's picks and bans, the currently active slot, summoner spells/names, and Data Dragon version/CDN metadata. Champion and summoner-spell payloads come from `OpenForestUI.Common` (`Champion`, `FrontEndSummonerSpell`) resolved via the `DataDragon` provider.

## Contents

| File | Role |
| --- | --- |
| `PickBan.cs` | Base type holding a `Champion`; parent of `Pick` and `Ban`. |
| `Pick.cs` | A draft pick: slot `id`, two summoner spells (`FrontEndSummonerSpell`), `isActive`, and `displayName`. |
| `Ban.cs` | A draft ban: champion plus `isActive`. |
| `Team.cs` | One side of the draft: `bans`, `picks`, and `isActive`; implements value `Equals`/`GetHashCode` so `State` can diff teams between updates. |
| `Meta.cs` | Data Dragon metadata wrapper: `cdn` URL plus a `Version`. |
| `Version.cs` | Current Data Dragon `champion` and `item` versions (read from the `DataDragon` provider). |
