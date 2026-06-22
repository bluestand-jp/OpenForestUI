> 🌐 **English** ・ [日本語](README-ja.md)

# OpenForestUI

A central hub for League of Legends stream overlays to augment and elevate tournament broadcasts — fully open source under the MIT license, free to fork and customize.

It currently includes champion-select and ingame overlays, with a post-game overlay as a possible future feature.

> ⚠️ **Use at your own risk — no warranty, no liability.**
> OpenForestUI is provided "as is", without warranty of any kind (see the [MIT License](LICENSE)). The maintainer and contributors take **no responsibility for any loss, damage, account action, broadcast issue, or other disadvantage** that may arise from using it. You alone are responsible for making sure your use complies with [Riot Games' Terms of Service](https://www.riotgames.com/en/terms-of-service) and any tournament or broadcast rules that apply to you. The optional OCR feature reads pixels from **your own screen** — it does not read game memory for that — but you are still responsible for how you use it.

> **Repository Maintainer:** Negi - BlueStand

## Features

Ingame features currently include:
1. Level-up indicators (with item-name support)
2. Item-purchase indicators
3. Objective timers & power-play pop-ups
4. Dynamic gold graph
5. CS / Gold player tab (gold via on-screen OCR; see [On-screen OCR](#on-screen-ocr-exact-cs--gold) below)
6. Custom scoreboard (a broadcast-style top bar and a comparison-scoreboard bottom bar)
7. Auto-init UI on game start
8. Custom objective timers

It also includes a C# integration of the RCVolus champion-select tool.

Accurate CS/Gold are read from the in-game HUD via a small Python OCR sidecar (`ocr-poc/goldcap.py`), because the spectator Live Client Data API does not expose exact values.

## Tech stack

- **Control app** — C# / .NET 6, WPF (MVVM via CommunityToolkit.Mvvm + Microsoft.Extensions.DependencyInjection, Fluent styling with WPF-UI), with an embedded HTTP/WebSocket server (EmbedIO) that serves the overlays.
- **Ingame overlay** — TypeScript + Phaser 3 (with phaser3-rex-plugins), bundled by Parcel.
- **Champion-select overlay** — TypeScript + React, Create React App (webpack/Babel) toolchain, Less.
- **OCR sidecar** — Python 3: EasyOCR (PyTorch), OpenCV, dxcam, NumPy, Pillow.
- **Game data** — League Client (LCU) API, Live Client Data API, Replay API, and an opt-in in-process memory reader (Farsight).

## Install (Windows)

The easiest way to run OpenForestUI — no build tools, no .NET, no manual Python:

1. Download **`OpenForestUI-<version>-win-x64.msi`** from the [latest release](https://github.com/bluestand-jp/OpenForestUI/releases/latest) and run it. It installs **per-user (no admin)** to `%LOCALAPPDATA%\Programs\OpenForestUI` with a Start Menu shortcut, and bundles its own .NET runtime + Python.
   - Prefer no installer? Download the **`…-win-x64.zip`** instead and extract it to a **short path** (e.g. `C:\OpenForestUI`), then run `OpenForestUI.exe`.
2. Launch **OpenForestUI** from the Start Menu.
3. In OBS, add browser sources:
   - Ingame: `http://localhost:9001/frontend`
   - Champion select: `http://localhost:9001/?backend=ws://localhost:9001/api`
4. *(Optional — exact CS/Gold)* Open **Settings → "Set up OCR now"**. It downloads the OCR dependencies (easyocr / PyTorch / …) **once, automatically** — no manual Python install. See [On-screen OCR](#on-screen-ocr-exact-cs--gold) for the capture requirements.

First launch downloads the latest DataDragon cache automatically (needs an internet connection).

## Build from source

For development, or to build your own customized version:

```bash
# 1. Get the code
git clone <your-fork-url> OpenForestUI && cd OpenForestUI

# 2. Build the C# app (produces OpenForestUI.exe)
dotnet build OpenForestUI.sln -c Debug

# 3. Build the overlays
cd Overlays/ingame  && npm install && npm run build && cd ../..
cd Overlays/pickban && npm install && npm run build && cd ../..

# 4. (Optional) Install the OCR sidecar deps — only needed for exact live CS/Gold
py -m pip install -r requirements.txt

# 5. Run it
#    Launch OpenForestUI.exe, then add the OBS browser sources below.
```

- **OBS — ingame source:** `http://localhost:9001/frontend?backend=localhost`
- **OBS — champion-select source:** `http://localhost:9001/?backend=ws://localhost:9001/api`

First launch downloads the latest DataDragon cache automatically (needs an internet connection).

## Requirements

* Windows 10 20H1 (May 2020 Update, Build 19041) or later
* [.NET 6 SDK](https://dotnet.microsoft.com/download/dotnet/6.0) — to build the app
* [Node.js](https://nodejs.org/) + npm — to build the overlays
* Python 3.9–3.12 with the deps in [`requirements.txt`](requirements.txt) — **only** for live CS/Gold OCR; the app runs without it (values fall back to the coarser spectator API)
* An internet connection on first run (DataDragon cache download)

## On-screen OCR (exact CS / Gold)

The spectator Live Client Data API floors CS to multiples of 10 and never exposes team gold or objective-monster counts, so OpenForestUI reads those numbers straight off the HUD with a small Python sidecar (`ocr-poc/goldcap.py`). This is **optional** — skip it and those values fall back to the coarser API numbers.

Because it captures **your actual screen** (DXGI Desktop Duplication), the OCR only works when the HUD is visible and laid out the way the readers were calibrated for:

- **Resolution 1920×1080.** The capture regions (ROIs) are calibrated for 1080p (borderless fullscreen). Other resolutions auto-scale but may misalign, since the LoL HUD does not scale perfectly linearly — recalibrate if so.
- **Spectator interface/HUD scale fixed at the calibrated setting.** The reference layout is captured at the maximum interface scale (100); a different scale shifts the panels off the ROIs.
- **The observer top bar and the detail scoreboard must stay visible and unobstructed.** Gold, tower, and objective counts are read from the top bar; per-player CS is read from the bottom-center detail scoreboard (default zoom/position). Keep both shown for the whole game.
- **Nothing may cover the numbers** — no other windows, pop-ups, the mouse cursor, or stream overlays over those regions, or the read is corrupted (it then hides rather than shows a wrong value).
- **Primary monitor.** Desktop-duplication capture grabs the primary display by default; run the spectator client there.

If your setup differs, recalibrate the ROIs with `goldcap.py --grab` / `--roi`. Full details, the read pipeline, and troubleshooting live in [`ocr-poc/README.md`](ocr-poc/README.md).

## Usage

- Components can be enabled/disabled independently — enable champion-select or ingame as you need.
- On first run, the latest DataDragon cache is downloaded automatically.
- If League is not installed at the default location, add the folder containing the "Riot Games" / "League of Legends" install to `Config/Component.json -> LeagueInstall` (comma-separated). This is needed for the Replay API.

### Overlay configuration

- `Config/Ingame.json` configures the ingame overlay — edit it to suit your needs.
- Add any fonts you want to the comma-separated `"GoogleFonts"` list.
- Swap images/videos in `Frontend/ingame` to change resources.

### Development / verification harness

`ocr-poc/overlay-harness/` renders the ingame overlay against a controlled mock state and screenshots it headlessly (no live game), so the overlay can be tuned/verified in isolation. See [`docs/prm-overlay/SPEC.md`](docs/prm-overlay/SPEC.md).

## Repository layout

The repository is organized into a few top-level areas:

- **`OpenForestUI/`** — the .NET 6 WPF control-center app (`OpenForestUI.exe`): MVVM dashboard shell, data ingest, and the embedded overlay server.
- **`OpenForestUI.Common/`** & **`OpenForestUI.Farsight/`** — shared low-level library (HTTP/REST, DTOs, utils, logging) and the opt-in memory reader.
- **`LCUSharp/`** — vendored League Client (LCU) API client used as the champion-select data source.
- **`Overlays/`** — the two browser-source overlays served to the broadcast client (`ingame` Phaser overlay, `pickban` React overlay).
- **`docs/`** — reference docs and design specs (local API availability, memory offsets, overlay layout specs).
- **`ocr-poc/`** — the Python OCR sidecar and the headless overlay render harness.

For a complete, clickable index of **every** directory and its README, see [docs/REPO-MAP.md](docs/REPO-MAP.md).

## License

Distributed under the **MIT License**. See [`LICENSE`](LICENSE) for details.

OpenForestUI is a fork derived from [LeagueBroadcast](https://github.com/floh22/LeagueBroadcast) by Lars Eble (MIT). The champion-select overlay is a port of [lol-pick-ban-ui](https://github.com/RCVolus/lol-pick-ban-ui); the ingame overlay scaffolding is based on the [phaser3-typescript-parcel-template](https://github.com/ourcade/phaser3-typescript-parcel-template) by ourcade (MIT). All upstream copyright notices are retained.

OpenForestUI is an independent project and is not endorsed by or affiliated with Riot Games. Its port of lol-pick-ban-ui and its author are in no way affiliated or partnered with Riot Community Volunteers.
