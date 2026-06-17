> 🌐 **English** ・ [日本語](README-ja.md)

# Farsight memory-reading offsets (per-patch archive)

A historical catalogue of `Offsets-<patch>.json` files — the byte offsets the
**OpenForestUI.Farsight** memory reader uses to pull in-process game data the
spectator API omits (current/total gold, EXP, HP/MP, position, items). These are
only meaningful when the optional memory reader is enabled; under the default
Vanguard-compatible configuration the app skips offset loading entirely
(`ConfigController.LoadOffsetConfig` short-circuits when the reader is off).

## Contents

- `Offsets-<version>.json` — one file per League patch, `11.9.1` → `14.6.1`
  (plus hotfix variants like `13.11.2`, `13.21.2`). Filename prefix `Offsets-`
  matches `ComponentConfig.OffsetPrefix`; the app selects the file for the running
  client's patch (`AppStateController.LoadOffsets` → `ConfigController`).
- **Two schema generations** appear in the archive:
  - **Legacy (floh22/LeagueBroadcast era, e.g. `11.9.1`)** — `GameOffsets` holds
    the per-object field offsets and `ObjectOffsets` holds the object-manager/map
    pointers; keys: `Manager`, `Map*`, `ID`, `NetworkID`, `Team`, `Pos`,
    `Mana/MaxMana`, `Health/MaxHealth`, `CurrentGold/GoldTotal`, `EXP`, `Name`,
    `ItemList`, `SpellBook`. `FileVersion: "1.0"`.
  - **Newer (e.g. `14.x`)** — the two sections are swapped/expanded: `GameOffsets`
    holds the manager/map pointers (`Manager`, `MapCount`, `MapRoot`,
    `MapNodeNetId`, `MapNodeObject`) and `ObjectOffsets` holds the per-object
    fields (`DisplayName`, `Level`, etc.). `OffsetVersion` names the patch;
    `FileVersion: "3.0"`.

> Provenance: this offset scheme and the early files originate from the upstream
> LeagueBroadcast project (floh22, MIT). Offsets are reverse-engineered per patch
> and break whenever the game binary's memory layout shifts, which is why there is
> one file per patch.

## How it's consumed

`ConfigController.Farsight` is populated from the active offset JSON, then
`FarsightController.GameOffsets` / `.ObjectOffsets` are set from it
(`AppStateController.cs`). The reader project is
[`../../../OpenForestUI.Farsight/`](../../../OpenForestUI.Farsight/).
