> 🌐 **English** ・ [日本語](README-ja.md)

# Replay API data models (Riot `/replay/*`)

Plain C# DTOs for the in-client **Replay API** (`https://127.0.0.1:2999/replay/*`), reachable only during `.rofl` playback. `ReplayDataProvider` GETs/POSTs these types to read the playback clock and to drive the in-game camera, HUD visibility, and post-processing while rendering a broadcast. JSON binding uses `System.Text.Json` `[JsonPropertyName]` attributes to match Riot's lowercase property names.

## Contents

| File | `/replay/*` resource / role |
| --- | --- |
| [`Game.cs`](Game.cs) | `/replay/game` — just `processID`. A successful GET is the signal that a replay is active (see `ReplayDataProvider.IsReplayActive`). |
| [`Playback.cs`](Playback.cs) | `/replay/playback` — `time`, `length`, `speed`, `paused`, `seeking`. Source of the replay clock that overrides `gameTime`, and the target for seek/pause POSTs. |
| [`Render.cs`](Render.cs) | `/replay/render` — the full render state: camera, fog/depth-of-field, skybox, and every `interface*` HUD toggle. Used to hide native HUD elements so our overlay can replace them. |
| [`Vector3.cs`](Vector3.cs) / [`Color.cs`](Color.cs) | `{x,y,z}` and `{r,g,b,a}` primitives used by `Render` and the sequence types. |
| [`Sequence.cs`](Sequence.cs) | `/replay/sequence` keyframe track (camera + fog/skybox params over time). Also declares the per-property keyframe wrappers (`DepthFogColor`, `HeightFogEnabled`, …). |
| [`SequenceVector3.cs`](SequenceVector3.cs) / [`SequenceVectorEntry.cs`](SequenceVectorEntry.cs) | A single keyframe (`blend`, `time`, `value`) for a scalar and for a `Vector3` value respectively. |
| [`Recording.cs`](Recording.cs) | `/replay/recording` — codec/path/resolution/fps and start/current/end time for client-side video recording. |
| [`InterfaceState.cs`](InterfaceState.cs) | Small internal flag (`TeamfightOpen`); not a Riot DTO. |
