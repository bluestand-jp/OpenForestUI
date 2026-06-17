> 🌐 **English** ・ [日本語](README-ja.md)

# Browser-source stream overlays (frontends)

The two web overlays that the OpenForestUI desktop app drives and serves to the
broadcast client (OBS browser source / vMix). They are plain TypeScript web apps:
the app hosts them over its embedded server — HTTP at `/frontend` and a WebSocket
control channel at `ws://localhost:9001/api` — and pushes game state and an
`OverlayConfig` over that socket. Each overlay renders the corresponding stage of a
broadcast.

## Subdirectories

| Directory | Purpose |
| --- | --- |
| [`ingame/`](ingame/) | In-game overlay (Phaser 3 + rexUI, parcel-bundled): scoreboard / top bar, objective timers, gold graph, level-up & item pop-ups, inhibitor timers, info side page. |
| [`pickban/`](pickban/) | Champion-select (pick & ban) overlay — a C# integration of RCVolus `lol-pick-ban-ui`. |

Both are bundled to a `dist/` folder and served by the desktop app; see each
overlay's own readme for build/run details.
