> 🌐 **English** ・ [日本語](README-ja.md)

# GameObject — a single unit read from League memory

This directory holds the in-memory unit model used by the [Farsight](../) memory reader.
A `GameObject` is the base type for every moving thing or structure in a League game —
champions, turrets, dragon, baron, herald — and is the C# mirror of one node in the game's
ObjectManager tree.

## Contents

- [`GameObject.cs`](GameObject.cs) — The unit class and its hydration logic.
  - **Fields** read from memory: `ID`, `NetworkID`, `Team`, `Position` (`Vector3`), `Name`,
    `DisplayName`, `Health`/`MaxHealth`, `Mana`/`MaxMana`, and (champions only)
    `CurrentGold`, `GoldTotal`, `EXP`, `Level`.
  - **`LoadFromMemory(baseAddr, buffSize)`** — reads the object's byte block and decodes each
    field at the patch-specific offset. Handles both short (inline ≤16 bytes) and long
    (pointer-indirected) string layouts for `Name`/`DisplayName`.
  - **`LoadChampFromMemory`** — runs only for champions to pull gold/XP/level.
  - **`IsChampion()`** — caches a lookup of `Name` against `FarsightController.Champions`
    (sourced from CommunityDragon champion data in `OpenForestUI.Common`).
  - **`Offsets`** (nested class) — the per-unit field offset table. Each field uses
    `HexStringJsonConverter` so it deserializes from the hex strings in `Config/Farsight.json`
    into `FarsightController.ObjectOffsets`. These values are **patch-specific** and must be
    refreshed when League updates — see [the parent README](../README.md#reverse-engineering-the-offsets-updating-after-a-patch).
