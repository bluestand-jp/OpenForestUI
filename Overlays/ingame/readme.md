> 🌐 **English** ・ [日本語](README-ja.md)

# In-game broadcast overlay (Phaser 3 + rexUI)

> Front-end stream overlay for OpenForestUI spectate / replay games

![License](https://img.shields.io/badge/license-MIT-green)

This is the in-game browser source. It is a Phaser 3 (WebGL, transparent canvas)
app, bundled with parcel, that the OpenForestUI desktop app serves at `/frontend`.
On start it opens a WebSocket to `ws://localhost:9001/api`, requests the `Ingame`
`OverlayConfig`, then renders each `GameHeartbeat` state and event (level-ups, item
completions, objective kills/spawns) the backend pushes. Scaffolding originates from
ourcade's `phaser3-typescript-parcel-template`.

## Capabilities

The overlay can render:

- Scoreboard / top bar: legacy `ScoreboardVisual`, or the opt-in broadcast top bar, plus an optional broadcast or comparison bottom bar
- Objective spawn timers (dragon / baron) and Baron / Elder Power Plays (legacy mode)
- Inhibitor respawn timers, info side page (per-player stat tabs), and a center gold-diff graph
- Pop-ups: player level-up, item completion, objective kill / spawn / soul-point

## Layout

| Path | Purpose |
| --- | --- |
| [`src/`](src/) | TypeScript source: scene, visuals, data models, utils. See its README for the full breakdown. |
| `public/` | Static assets (images, masks, backgrounds) copied verbatim into the build and served under `/frontend`. |
| `dist/` | Parcel build output (what the desktop app actually serves). |
| `package.json` | Scripts (`start` / `build` via parcel, `lint` via eslint) and deps (`phaser`, `phaser3-rex-plugins`, `strongly-typed-events`). |

`npm run start` serves on port 10001 with `--public-url /frontend`; `npm run build`
emits to `dist/`. The desktop app serves the built `dist/` itself.

## Prerequisites

You'll need [Node.js](https://nodejs.org/en/), [npm](https://www.npmjs.com/), and [Parcel](https://parceljs.org/) installed.

It is highly recommended to use [Node Version Manager](https://github.com/nvm-sh/nvm) (nvm) to install Node.js and npm.

For Windows users there is [Node Version Manager for Windows](https://github.com/coreybutler/nvm-windows).

A installNode.bat is included to do all these steps for you.
Incase you already have node installed, or wish to install yourself, these are the required steps

Install Node.js and `npm` with `nvm`:

```bash
nvm install node

nvm use node
```

Replace 'node' with 'latest' for `nvm-windows`.

Then install Parcel:

```bash
npm install -g parcel-bundler
```

## Getting Started

Make sure both this repo and OpenForestUI are installed on your local machine.
This project should be included in your install of OpenForestUI.



Incase you did not use installNode.bat Go into your new project folder and install dependencies:

```bash
cd frontend/ingame/ # or 'my-folder-name'
npm install
```

Start development server if using this separately from OpenForestUI:

```
npm run start
```

Copy dist to your web server incase you wish to host this project elsewhere. OpenForestUI uses parcel by default to serve files.

## Dev Server Port

You can change the dev server's port number by modifying the `start` script in `package.json`. We use Parcel's `-p` option to specify the port number.

The script looks like this:

```
parcel src/index.html -p 10001
```

Change 10001 to whatever you want.

## License

[MIT License](https://github.com/ourcade/phaser3-typescript-parcel-template/blob/master/LICENSE)
