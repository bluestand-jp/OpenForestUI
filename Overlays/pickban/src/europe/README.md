> 🌐 **English** ・ [日本語](README-ja.md)

# "Europe" draft layout

The React components that render the actual champion-select overlay in the European-style "europe" layout: a center column (logo, patch text, draft timer) flanked by the blue and red teams' picks and bans. This is the only layout shipped by the fork.

## Contents

| File | Role |
| --- | --- |
| `Overlay.jsx` | Top-level layout component. Plays the staged reveal animation when champ select starts, renders the timer/logo middle-box and both teams (picks + bans + name/score/coach), applies team colors via CSS vars, and shows "not connected" banners. |
| `Pick.jsx` | One pick slot: champion loading splash, summoner-spell icons (when enabled and not actively picking), and player display name. |
| `Ban.jsx` | One ban slot: the banned champion's square icon. |

## Subdirectories

| Directory | Purpose |
| --- | --- |
| [`style/`](style/) | LESS stylesheets (CSS modules) and draft-reveal animations |
