> 🌐 **English** ・ [日本語](README-ja.md)

# Raw LCU champ-select DTOs

Plain C# models matching the JSON shape of the League Client (LCU) `lol-champ-select` session. These are deserialized from the raw LCU WebSocket event payload (`PickBanController.ApplyNewState`) and then fed into `StateInfo/Converter` to be normalized into overlay-facing `DTO` types. They mirror the client's field names; they are not sent to the overlay directly.

## Contents

| File | Role |
| --- | --- |
| `Session.cs` | Top-level champ-select session: `myTeam`/`theirTeam` (`Cell` lists), grouped `actions` (`List<List<Action>>`), and `timer`. |
| `Cell.cs` | One draft slot: `cellId`, `championId`, `summonerId`, `spell1Id`/`spell2Id`. |
| `Action.cs` | A pick or ban action (`type`, `championId`, `actorCellId`, `completed`); plus an `ActionType` helper exposing the `"pick"`/`"ban"` constants. |
| `Timer.cs` | Phase timing fields (`phase`, `adjustedTimeLeftInPhase`, `internalNowInEpochMs`, …) used to compute the countdown. |
| `Summoner.cs` | Minimal summoner record (`displayName`, `summonerId`) used when caching player names. |
