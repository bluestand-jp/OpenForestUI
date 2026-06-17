> 🌐 **English** ・ [日本語](README-ja.md)

# Champ-select state & conversion

The heart of the champion-select pipeline: it converts raw LCU session data into the normalized overlay model, holds the current draft state as a singleton, and decides which pick/ban slot is "active". `Common/Controllers/PickBanController` calls into here on every LCU event and on each tick.

## Contents

| File | Role |
| --- | --- |
| `CurrentState.cs` | Thin wrapper around a raw LCU `Session` plus an `isChampSelectActive` flag — the input handed to `Converter.ConvertState`. |
| `Converter.cs` | Pure conversion logic: flattens grouped LCU actions, builds normalized `Team`s (picks/bans, spells via `DataDragon`, names via `AppStateController`), computes the countdown timer (`ConvertTimer`) and the phase label (`ConvertStateName`, e.g. "BAN PHASE 1"/"PICK PHASE 2"/"FINAL PHASE"). Outputs `StateConversionOutput`. |
| `State.cs` | Static state hub: holds the singleton `StateData`, applies diffed updates (`NewState`), and exposes `EventHandler`s (`StateUpdate`, `NewAction`, `ChampSelectStarted`, `ChampSelectEnded`) plus Data Dragon version/CDN/config accessors. `TriggerUpdate` fans out to overlays. |
| `StateData.cs` | The current draft state (`blueTeam`/`redTeam`, `timer`, `state`, `champSelectActive`, `leagueConnected`, `config`). Also defines the nested `CurrentAction` type and `GetCurrentAction`/`RefreshAction`, which derive and re-resolve the currently active pick/ban slot. |
