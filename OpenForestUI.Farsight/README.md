> 🌐 **English** ・ [日本語](README-ja.md)

# Farsight — League of Legends memory reader

`OpenForestUI.Farsight` is the in-process memory-reading library. It attaches to a
running League of Legends client and walks the game's **ObjectManager** to read per-unit
values the spectator Live Client Data API (port 2999) rounds or omits — exact gold, XP,
level, and positions for every champion, plus turrets and epic monsters.

It is **opt-in and off by default** (`FarsightController.ShouldRun = false`) for Vanguard
compatibility: with Riot's anti-cheat active, an external process cannot enumerate the
client's modules and attachment fails cleanly, so the app falls back to API-only mode.
The flag is flipped on by `ConfigController`/`BroadcastController` only when the user
enables `UseMemoryReader`. Note that as of the current Phase-3 ingame pipeline the snapshot
is still produced when enabled, but its output is no longer plumbed into broadcast State.

## How it works

1. `BroadcastController` constructs a `FarsightController` and calls `Connect(process)`,
   which hands the League process to `Memory.Initialize`.
2. Each frame `CreateSnapshot()` reads the ObjectManager root, does a breadth-first walk
   of the object tree (red-black-style node triplet at offsets 0/8/16), filters by NetID
   range, and dereferences each live object pointer.
3. Every pointer is hydrated into a `GameObject` (`LoadFromMemory`) and classified into the
   `Snapshot` buckets (champions / turrets / dragon / baron / herald), with the next dragon
   type inferred from `Dragon_Indicator_*` display names.

All struct/field offsets are **patch-specific** and are NOT hard-coded here. They are loaded
at runtime from `Config/Farsight.json` (deserialized by `OpenForestUI/Ingame/Data/Config/FarsightConfig.cs`)
into `FarsightController.GameOffsets` (ObjectManager tree layout) and
`GameObject.Offsets` (per-unit field layout). See [Reverse-engineering the offsets](#reverse-engineering-the-offsets-updating-after-a-patch)
below for how to refresh them after a League patch.

## Contents

| File | Role |
| --- | --- |
| [`FarsightController.cs`](FarsightController.cs) | Entry point. Owns the `ShouldRun` opt-in flag, connects to the process, walks the ObjectManager tree in `CreateSnapshot`, classifies objects, and defines the `Offsets` class for the tree layout (`Manager`, `MapRoot`, node NetID/object offsets). Maintains an object blacklist (test cubes, respawn markers, etc.). |
| [`Memory.cs`](Memory.cs) | Low-level Win32 wrapper around `OpenProcess` / `ReadProcessMemory` / `WriteProcessMemory` / `VirtualQueryEx`. Vanguard-aware `Initialize` catches the access-denied failure and degrades to API-only mode. Ported from [C0reExternal-Base-v2](https://github.com/C0reTheAlpaca/C0reExternal-Base-v2/blob/master/Memory.cs). |
| [`Snapshot.cs`](Snapshot.cs) | Plain data container for one read frame: champion list, dragon/baron/herald, turret set, NetID→object map, index→NetID map, and the next dragon type. |
| `OpenForestUI.Farsight.csproj` | net6.0 class library; references `OpenForestUI.Common` (logging, byte-buffer extensions, `CDragonChampion`, hex-JSON converter). |

## Subdirectories

| Directory | Purpose |
| --- | --- |
| [`Object/`](Object/) | The `GameObject` unit model and its per-unit memory offsets |

## Reverse-engineering the offsets (updating after a patch)

Because League stores its data unencrypted, the structure can be reversed and read directly.
The steps below are abbreviated — they assume some programming experience and the right tools.
If you produce updated offsets and they are not yet in the repo, please open a Pull Request;
this is a volunteer-driven open-source project.

### Setup

League's anti-cheat detects tools by program name and icon, so any tool used while League is
open must be rebuilt yourself or have its window name and icon changed with
[Resource Hacker](http://www.angusj.com/resourcehacker/).

**Prerequisites**
- (Optional) [Resource Hacker](http://www.angusj.com/resourcehacker/)
- A modified [Cheat Engine](https://www.cheatengine.org/) or [ReClass.NET](https://github.com/ReClassNET/ReClass.NET)
- [LeagueDumper](https://github.com/tarekwiz/LeagueDumper)
- [LoL Offset Dumper](https://www.unknowncheats.me/forum/league-of-legends/386218-lol-offset-dump.html) and a pattern list (those below are current as of 26.08.2021, but change over time)

### What we are looking for

Every champion and unit lives in the **ObjectManager**, a tree of GameObjects. So we need
(1) the ObjectManager location and (2) the structure of a single GameObject.

### Step 1 — ObjectManager and Local Player

Base offsets are usually posted on UnknownCheats shortly after each patch; there is a central
Offsets/Patterns thread that is updated every patch — use it (and please do not spam it with
help requests). If you must produce them yourself, follow the LeagueDumper and LoL Offset
Dumper instructions. Current patterns:

```
ADDRESS, oLocalPlayer,        "A1 ? ? ? ? 85 C0 74 07 05 ? ? ? ? EB 02 33 C0 56", 1
ADDRESS, oObjManager,         "A1 ?? ?? ?? ?? C7 40 ?? ?? ?? ?? ?? C3", 1
ADDRESS, oObjManagerBackup,   "8B 0D ? ? ? ? E8 ? ? ? ? FF 77", 1
ADDRESS, oGameTime,           "F3 0F 11 05 ? ? ? ? 8B 49", 1
ADDRESS, oHudInstance,        "A1 ? ? ? ? F3 0F 10 44 24 08", 1
ADDRESS, oHudInstanceBackup,  "8B 0D ? ? ? ? 6A 00 8B 49 34 E8 ? ? ? ? B0", 1
ADDRESS, oUnderMouseObject,   "8B 0D ? ? ? ? 89 0D ? ? ? ? 3B 44 24 30", 2
```

### Step 2 — GameObject

1. Open a custom or practice-tool game.
2. Open ReClass (or Cheat Engine — at least the same capability).
3. Create a new class at `[<League of Legends.exe> + LocalPlayerOffset]`; the values should
   start updating. Add ~13000–14000 bytes; if the whole list turns to zeros you overshot the
   allocated range (access violation) — delete it and add a bit less.
4. In ReClass: red = offsets, green = absolute address, blue = ASCII, the four black hex
   columns = raw values, decimals follow the green slash, a red arrow marks pointers, and
   detected strings render in blue text.
5. Use the old offsets to inform where to look; you can change values in a live game to find them.

**Tips**
- Health/MaxHealth and Mana/MaxMana usually sit `0x10` apart — find the pair, then buy items
  or take damage to tell which is which.
- The champion name often renders as a string on the far right of the ReClass list (frequently
  wrong, but handy for a quick glance).
- Use a Practice Tool lobby so you can raise XP. Look for the **experience** value, not the
  level (e.g. level 2 = 280 XP, level 3 = 660); table on the
  [LoL wiki](https://leagueoflegends.fandom.com/wiki/Experience_(champion)).
