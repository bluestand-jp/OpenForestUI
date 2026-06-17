> 🌐 **English** ・ [日本語](README-ja.md)

# Overlay image & font assets

Static art bundled into the overlay (imported directly by the React components, so webpack hashes/inlines them). Live champion art arrives at runtime as `/cache` URLs from the backend; the files here are the fallbacks and branding shown before/when no champion data is present.

## Contents

| File | Role |
| --- | --- |
| `BlueEssence.png` | Center logo shown in the draft middle-box. Imported by [`../europe/Overlay.jsx`](../europe/Overlay.jsx). |
| `ban_placeholder.svg` | Empty-ban-slot square. Used by `convertState.js` to fill bans up to 5. |
| `top_/jung_/mid_/bot_/sup_splash_placeholder.svg` | Per-role pick-slot splash placeholders (indexed 0-4). Used by `convertState.js` to fill picks up to 5. |

## Subdirectories

| Directory | Purpose |
| --- | --- |
| [`fonts/`](fonts/) | TrueType fonts referenced by the overlay's `@font-face` rules |
