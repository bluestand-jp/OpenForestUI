> 🌐 **English** ・ [日本語](README-ja.md)

# Ingame data models (Riot Live Client Data API)

Plain C# DTOs that mirror the JSON returned by the spectator **Live Client Data API** (`https://127.0.0.1:2999/liveclientdata/*`). `IngameDataProvider` deserializes the endpoint responses straight into these types, which the ingame state machine (`Ingame/State/State.cs`) then folds into the broadcast model. Field names match the Riot JSON exactly so Newtonsoft can bind them without attributes.

## Contents

| File | Endpoint / role |
| --- | --- |
| [`GameMetaData.cs`](GameMetaData.cs) | `/gamestats` — `gameMode`, `gameTime`, map name/number/terrain. Debug-valued default ctor. |
| [`Player.cs`](Player.cs) | One entry of `/playerlist`. Holds champion/items/runes/spells/scores plus broadcast-side extras: optional Farsight `GameObject`, level/inventory diff trackers (`LastLevel`, `LastItemBySlot`, `LastCountBySlot`) for milestone/item-completion detection, and CS-per-minute helpers. Note: `summonerName` is the full `Name#Tag` Riot ID; match `/eventdata` names against `riotIdGameName`. |
| [`Score.cs`](Score.cs) | A player's `scores` block (kills/deaths/assists/CS/ward score/plates). `Update` can optionally skip CS. |
| [`Item.cs`](Item.cs) | One inventory item (`itemID`, `price`, `slot`, `count`, …). Feeds the item-completion and gold-estimate logic. |
| [`Rune.cs`](Rune.cs) / [`RuneList.cs`](RuneList.cs) | A rune and the keystone + primary/secondary trees of a player's `runes`. |
| [`Summoner.cs`](Summoner.cs) / [`SummonerList.cs`](SummonerList.cs) | A summoner spell (display/raw names) and a player's two spell slots. |
| [`Turret.cs`](Turret.cs) | Memory-reader turret model (name, position, health, last-damaged-by map) used to credit turret kills. |
