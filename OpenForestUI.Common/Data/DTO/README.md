> 🌐 **English** ・ [日本語](README-ja.md)

# Champion & summoner-spell DTOs

Data-transfer objects for champions and summoner spells. Each file pairs a Community Dragon source type (deserialized from CDragon JSON by the app's `DataDragon` provider, kept in a static `All` set) with the slimmer shape that gets sent to the overlays / used internally.

## Contents

- `Champion.cs`
  - `CDragonChampion` — Champion entry parsed from Community Dragon (`id`, `alias`, `name`, splash/loading/square image paths). Static `All` set holds every loaded champion.
  - `Champion` — Lightweight app-side champion DTO (`id`, `key`, `name`, image fields).
- `SummonerSpell.cs`
  - `SummonerSpell` — Summoner spell parsed from CDragon; `id` is coerced from a JSON number via [`NumberToStringJsonConverter`](../../Utils/NumberToStringJsonConverter.cs). Static `All` set; `AsFrontEndSummonerSpell()` projects to the overlay shape.
  - `FrontEndSummonerSpell` — Flat DTO (`id`/`key`/`name`/`icon`/`iconPath`) serialized to the frontend overlays.
