> 🌐 **English** ・ [日本語](README-ja.md)

# Riot static-data models

Static game-data models used to interpret raw values coming from the game (notably the Farsight memory reader and Community Dragon). These encode Riot's fixed tables — the XP-per-level curve and item gold costs — so the rest of the app can turn raw numbers into meaningful state.

## Contents

- `ChampionLevel.cs` — Riot's XP→level table (levels 1–18 with cumulative XP thresholds). `ChampionLevel.EXPToLevel(exp)` binary-searches the table to convert a raw XP value (e.g. read from memory) into a champion level.
- `ItemData.cs`
  - `ItemData` — App-side item record (`itemID`, gold `ItemCost`, `specialRecipe`, `sprite`, `name`).
  - `CDragonItem` — Item parsed from Community Dragon (`id`, `name`, build `from` components, `price`/`priceTotal`, `iconPath`); static `All` and `Full` sets hold loaded items.
- `ItemCost.cs` — Gold cost pair for an item: `total` (buy) and `sell`.
