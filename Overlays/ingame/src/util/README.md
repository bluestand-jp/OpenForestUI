> 🌐 **English** ・ [日本語](README-ja.md)

# Overlay utilities (generic helpers)

Small, dependency-free helpers shared across the scene and visual elements:
formatting, font loading, color conversion, geometry, and simple data structures.

## Contents

| File | Role |
| --- | --- |
| [`Utils.ts`](Utils.ts) | `ConvertGold` (formats a gold value as `"X.Yk"`); `LoadFont(name, url)` returns a `Promise` that registers a `FontFace` with the document. |
| [`TextUtils.ts`](TextUtils.ts) | `AutoSizeFont` shrinks a Phaser `Text`'s font size until it fits a width/height box. |
| [`ColorUtils.ts`](ColorUtils.ts) | `GetRGBAString(color, alpha)` builds a CSS `rgba(...)` string from a Phaser color. |
| [`Vector2.ts`](Vector2.ts) | 2D vector type used throughout positions/sizes, with magnitude/normalize/inverse helpers and static `add`/`mul`/`dot`. |
| [`Queue.ts`](Queue.ts) | Generic FIFO `Queue<T>` (head/tail index based); backs `RegionMask`'s per-slot animation queue. |
| [`Dictionary.ts`](Dictionary.ts) | Generic string-keyed `Dictionary<T>` with `add`/`remove`/`get`/`keys`/`values`/`containsKey`. |
