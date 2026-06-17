> 🌐 **English** ・ [日本語](README-ja.md)

# Shared data models

Plain data classes shared across the solution. Split into two groups: app-facing DTOs that overlays and providers serialize, and Riot/Community Dragon static-data types used to interpret game data (e.g. converting raw XP read by Farsight into a champion level, or item gold values).

## Subdirectories

| Directory | Purpose |
| --- | --- |
| [`DTO/`](DTO/) | App/overlay-facing DTOs (champion, summoner spell) plus their Community Dragon source types |
| [`RIOT/`](RIOT/) | Riot static-data models: XP→level table, item data and gold cost |
