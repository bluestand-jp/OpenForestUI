> 🌐 **English** ・ [日本語](README-ja.md)

# Utility extensions & helpers

Generic, dependency-free helpers reused across the solution: byte-buffer extensions for the Farsight memory reader, custom JSON converters, a fixed-capacity ring buffer, and a comparable version type. `StringVersion.cs` is adapted from [GoldDiff](https://github.com/Johannes-Schneider/GoldDiff) (MIT).

## Contents

- `ByteUtils.cs` — `byte[]` extension methods for reading primitives out of raw buffers (`ToIntPtr`/`ToInt`/`ToUInt`/`ToShort`/`ToFloat`), sub-array extraction, ASCII checks/decoding, and in-place writes. Core helpers for the Farsight memory reader.
- `CircularBuffer.cs` — Generic fixed-capacity `CircularBuffer<T>` (`PushBack`/`PushFront`, indexer, `IEnumerable<T>`); overwrites the oldest element when full. Interface inspired by Boost's circular_buffer.
- `StringVersion.cs` — `IEquatable`/comparable dotted version type (Major/Minor/Patch) with `Parse`/`TryParse`, calling-assembly version accessors, and a bundled `StringVersionConverter` for JSON. *(Adapted from GoldDiff.)*
- `NumberToStringJsonConverter.cs` — `System.Text.Json` converter that reads a JSON number into a `string` (and writes it back as a number); used by `SummonerSpell.ID`.
- `HexStringJsonConverter.cs` — Newtonsoft converter that serializes an `int` as a `0x…` hex string and parses it back (used for memory offsets in config).
- `StringUtils.cs` — `AllIndexesOf(value)` string extension returning every index of a substring.
- `DictionaryUtils.cs` — `KeyByValue(val)` reverse lookup: returns the first key whose value equals `val`.
