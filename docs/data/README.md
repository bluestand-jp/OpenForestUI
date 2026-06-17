> 🌐 **English** ・ [日本語](README-ja.md)

# Archived game-data tables

Static, versioned data tables the app references — currently the per-patch memory
offset archive used by the optional Farsight memory reader. This is reference data,
not runtime config: the app loads a single active offset set per patch
(`Config/Farsight.json`, sourced from `OpenForestUI/Offsets/Offsets-<patch>.json`);
the copies here are the historical catalogue for that scheme.

## Subdirectories

| Directory | Purpose |
|---|---|
| [`offsets/`](offsets/) | Per-patch Farsight memory-reading offsets (`Offsets-<patch>.json`), patch 11.9 → 14.6 |
