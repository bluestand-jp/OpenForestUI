> 🌐 **English** ・ [日本語](README-ja.md)

# App data layer (configuration + asset provider)

Groups the desktop app's data concerns: the strongly-typed JSON configuration models and their loader, and the static game-asset provider that downloads/caches Data Dragon assets. Consumed across the controllers and view-models in [`../`](../).

## Subdirectories

| Directory | Purpose |
|-----------|---------|
| [`Config/`](Config/) | JSON config schema (`Component`, team configs) + the provider that reads/migrates/writes them. |
| [`Provider/`](Provider/) | Data Dragon / Community Dragon static-asset downloader and cache (`DataDragon`). |
