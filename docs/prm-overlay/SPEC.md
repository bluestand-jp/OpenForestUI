# Broadcast Top‑Bar Overlay — Spec & Design

> Goal: reproduce a pro/esports broadcast top bar in this fork's ingame overlay,
> with strictly‑accurate data sourced from the Live Client Data API (port 2999),
> the LCU (client), and operator config — preferring to hide a value over showing
> an approximation.
>
> Reference frame: `docs/prm-overlay/prm_reference.png` (decoded from the user's
> `Brave Screenshot 2026.06.12 - 01.49.02.09.jxr`, 1920×1080).

## Implementation status (2026-06-12)

**Phase 1 (top bar) — implemented & harness-verified.**
- Frontend `Overlays/ingame/src/visual/PrmScoreboardVisual.ts` (config-driven, opt-in
  via `OverlayConfig.PrmScore.Enabled`); icons extracted from the reference to
  `public/images/prm/` (white-on-transparent); flags generated for fr/de.
- Backend (C#, compiles clean on .NET 6): `HordeKill` void-grub counting +
  `InhibKilled` credit (`State.cs`), `Team.voidgrubs`/`inhibsDestroyed`,
  `FrontEndTeam` {VoidGrubs,Inhibitors,Region,Seed,Flag}, `ScoreboardConfig.TournamentName`,
  `TeamConfig` {region,seed,flag}, `IngameComponentConfig.TournamentName`,
  `IngameConfig.PrmScore` defaults.
- Verification harness `ocr-poc/overlay-harness/` (mock-WS feeds reference state →
  Edge headless CDP screenshot → composite vs `prm_reference.png`). Side-by-side:
  `ocr-poc/overlay-harness/sidebyside.png`.

**Phase 2 (bottom comparison bar) — implemented & harness-verified.**
- Backend: `PlayerScoreboardEntry` DTO (champ/KDA/CS/items/spells/level/position; per-player
  gold deliberately omitted), `ScoreboardConfig.Players` roster built in `State.UpdateScoreboard`.
- Frontend `PrmBottomBarVisual.ts`: 5-row lane grid (sorted TOP/JGL/MID/BOT/SUP), mirrored
  per side, champion/item/summoner-spell icons from DataDragon (version auto-resolved via
  versions.json, fallback 16.12.1), team-tinted background, role labels in center (in place of
  the omitted gold diff), tournament + patch labels. Opt-in via `PrmScore.BottomBar`.
- Verified: harness fed a real 10-player roster (from `/playerlist`); all DataDragon icons
  loaded; `ocr-poc/overlay-harness/dark_bottom.png` / `comp_bottom.png`.

Remaining: Phase 3 (side elements / webcams / sponsor are operator assets). Cosmetic polish:
top-bar league crest, panel trapezoids, `0:07` chip; bottom-bar column-position tuning to the
reference, and an operator UI for region/seed/flag/tournament fields.

## Table of Contents
1. [Reference decode](#1-reference-decode)
2. [Metric → data‑source map](#2-metric--data-source-map)
3. [Void‑grub event verification](#3-void-grub-event-verification)
4. [Color & layout spec](#4-color--layout-spec)
5. [Architecture / design](#5-architecture--design)
6. [Backend changes](#6-backend-changes)
7. [Frontend changes](#7-frontend-changes)
8. [Verification harness](#8-verification-harness)
9. [Scope & phasing](#9-scope--phasing)
10. [Open questions / assumptions to re‑confirm](#10-open-questions--assumptions-to-re-confirm)

---

## 1. Reference decode

Decoded from the HDR `.jxr` via WIC (`WmpBitmapDecoder` → PNG), then brightened
crops (`ocr-poc/*.png`). Match shown is **region #2 vs region #1**, game
clock **19:52**, score **7–9**.

### Top bar regions (left → right)
- **Tournament banner** (x≈10): the broadcast/series name | day, white text on
  dark slate; a small **`0:07` chip** sits at its lower‑left (purpose uncertain —
  auxiliary/real‑time clock; low priority).
- **League crest** (x≈390): broadcaster logo.
- **Blue team panel** (x≈410–560): team logo · the team tag (large white) ·
  country flag (FR) · region+seed **(region) #2**.
- **Center scoreboard** (x≈560–1360): symmetric counters around the score.
- **Red team panel** (x≈1360–1540): region+seed **(region) #1** · flag (DE) ·
  the team tag (large) · team logo.
- **Objective timer chip** (top‑right, x≈1860): **`4:22`** with a dragon icon =
  next‑dragon countdown (19:52 + 4:22 ≈ 24:14 next spawn).

### Center counters (per team, center → outward)
Confirmed by game‑logic constraints at 19:52 and 10× brightened icon crops
(`docs/prm-overlay/ic_tower.png`, `ic_grub.png`, `ic_shield.png`):

| Slot (center→out) | Icon | Blue | Red | Metric |
|---|---|---|---|---|
| 1 (innermost) | big number | **7** | **9** | **Kills** (blue=cyan, red=pink) |
| 2 | swirl/coin | **36.2K** | **38.5K** | **Gold** (+ lead badge under leader) |
| 3 | turret silhouette ("hourglass") | **3** | **4** | **Towers** (7 total ⇒ only towers fits at 20 min) |
| 4 | horned grub head | **0** | **0** | **Void Grubs** |
| 5 (outermost) | shield w/ crest | **0** | **3** | **Dragons (count)** — see §10 (Dragons vs Plates) |

- **Gold‑lead badge**: magenta rounded chip "**+2.3K**" beneath the *leading*
  team's gold (here red, 38.5−36.2≈2.3K).
- **Center divider**: a sword/star "✦" between the two kill numbers.
- **Game clock** "19:52" centered just below the kills on a recessed plate.

---

## 2. Metric → data-source map

| UI element | Source | Status in fork |
|---|---|---|
| Game clock | `gameTime` | ✅ exists |
| Kills | `Team.kills` (sum of /playerlist) | ✅ |
| Gold | `Team.GetGold` (OCR exact / estimate) | ✅ |
| Gold lead | `blueGold − redGold` | derive in FE (data present) |
| Towers | `Team.towers` (event‑count / OCR in replay) | ✅ |
| Dragons (count + types) | `Team.dragonsTaken` (List) | ✅ (count = `.length`) |
| **Void Grubs** | `/eventdata` **`HordeKill`** → new `Team.voidgrubs` | ❌ **ADD** |
| Inhibitors (count) | `stateData.inhibitors` (InhibitorInfo) | ✅ (derivable) |
| Team tag | `TeamConfigViewModel.{Blue,Red}.NameTag` | ✅ (operator) |
| Team logo | `ExtendedTeamConfig.IconLocation` | ✅ (operator) |
| **Region** | new `TeamConfig.region` | ❌ **ADD (config)** |
| **Seed (#1/#2)** | new `TeamConfig.seed` | ❌ **ADD (config)** |
| **Country flag** | new `TeamConfig.flag` (ISO code) | ❌ **ADD (config)** |
| **Tournament name** | new broadcast/series config | ❌ **ADD (config)** |
| Next‑dragon timer | `nextDragon` | ✅ (ObjectiveTimerVisual) |

**LCU cross‑check (per user directive):** the top bar has minimal LCU
dependency — objective counts come from the in‑GAME Live Client Data API (2999),
not the LCU (client). LCU contributes: **patch version** (`LocalGameVersion`,
already fetched via `/lol-patch/v1/game-version` — used for the bottom bar's
"PATCH 26.11") and **rosters/summoner names** (`/lol-champ-select` — bottom bar).
Team region/seed/flag/tournament are **operator‑configured** (no API supplies
them). This is documented so the cross‑check is explicit, not hand‑waved.

---

## 3. Void-grub event verification

Confirmed against two independent real codebases (GitHub) + the live API schema:
- **Event name: `EventName == "HordeKill"`**, with `KillerName` + `Assisters`
  (a `MonsterKill`, identical shape to `HeraldKill`/`BaronKill`/`DragonKill`).
  (Sources: bonepl/ChromaLeague `EventType.ts`, AlsoSylv/Irelia `types.rs`.)
- Bonus: modern `DragonKill` carries a **`DragonType`** field directly (e.g.
  "Fire") — more robust than parsing `VictimName`. `AtakhanKill` also exists (2025).
- **Accuracy caveat (Irelia):** from an *active player's* viewpoint the API emits
  one `HordeKill` per grub for the ally team but only **one** event when the enemy
  clears a set. Spectator/replay (no active player) behavior is **unverified** —
  must validate against live data before trusting exact per‑team grub counts.
  Until validated, treat grubs like towers (monotonic event count, credit
  `GetKillerTeam(KillerName)`), and **log every `HordeKill`** seen so the real
  spectator behavior is observable. Fall back to HUD OCR if counts prove wrong
  (same pattern as towers in replay).

Verified on this machine: `GET https://127.0.0.1:2999/liveclientdata/eventdata`
returns `TurretKilled` ids in the new `Turret_TChaos_…` form and `KillerName`
populated for both teams in spectator/replay — consistent with crediting by killer.

---

## 4. Color & layout spec

Horizontal gradient sampled from the reference top strip (text‑free row):

| x | hex | role |
|---|---|---|
| 420 (blue panel) | `#6FE4FD` | cyan (blue team) |
| 560 | `#35A9F0` | |
| 720 | `#3867AB` | |
| 860–960 (center) | `#33316D` | dark indigo |
| 1060 | `#322674` | |
| 1200 | `#6E40A1` | purple |
| 1360 | `#D76CDF` | magenta |
| 1500 (red panel) | `#FCACFD` | pink (red team) |

- Bar: full‑width, ~70 px tall, top‑anchored. Gradient cyan→indigo→magenta.
- Kills: blue cyan, red pink; all counters white.
- Gold‑lead badge: magenta rounded rect, white text, under the leader.
- Banner / timer chips: dark slate (`#53546​2`-ish) rounded, white text.
- Fonts: condensed bold sans (kills heavier/larger). Exact face TBD; use a close
  configurable Google font (candidate: "Saira Condensed" / "Oswald") pending
  the bottom‑bar typography pass.

---

## 5. Architecture / design

Principle (per project memory): **coder fully customizable, ecosystem‑independent,
strictly accurate.** So the top bar is built as a **config‑driven generic
counter‑slot scoreboard**, not hard‑coded to any one broadcast:

- New overlay config section **`PrmScore`** (parallel to `Score`) describing:
  gradient stops, team panels (logo/tag/flag/region/seed positions+fonts),
  an **ordered list of metric "slots"** each `{ metric, icon, position, font }`
  where `metric ∈ {kills,gold,towers,voidgrubs,dragons,inhibitors,barons,plates,goldlead}`,
  the center clock, divider, and the gold‑lead badge.
- New Phaser visual **`PrmScoreboardVisual`** renders it. Instantiated in
  `IngameScene.UpdateConfig` only when `PrmScore` is present (so the legacy
  `ScoreboardVisual` is untouched and still default).
- This means *any* broadcast's top bar (towers+grubs+dragon‑pips, alternate
  objective sets, etc.) is reproducible by editing slots — matching the fork's
  customization ethos.

---

## 6. Backend changes

1. `Ingame/Data/Hub/Team.cs`: add `public int voidgrubs;`.
2. `Ingame/State/State.cs`
   - `UpdateEvents` switch: `case "HordeKill": GetKillerTeam(e.KillerName)?.voidgrubs++` ; add a `default:` log of unknown `EventName` (discoverability).
   - `ApplyHistoricalBaseline`: also count `HordeKill` (mid‑game catch‑up + replay).
   - `UpdateScoreboard`: set `currentTeam.VoidGrubs`, `.Inhibitors`, `.Region`, `.Seed`, `.Flag`, scoreboard `.TournamentName`.
3. `Ingame/Data/Frontend/FrontEndTeam.cs`: add `VoidGrubs`, `Inhibitors`, `Region`, `Seed`, `Flag`.
4. `Ingame/Data/Frontend/ScoreboardConfig.cs`: add `TournamentName`.
5. `ChampSelect/Data/Config/TeamConfig.cs`: add `region`, `seed`, `flag`.
6. (Optional) `RiotEvent.cs`: parse `DragonType` if present.

All additive — no existing field removed. Build target: .NET 6 (`dotnet 6.0.428`).

## 7. Frontend changes

1. `data/frontEndTeam.ts`: add `VoidGrubs, Inhibitors, Region, Seed, Flag`.
2. `data/scoreboardConfig.ts`: add `TournamentName`.
3. `data/config/overlayConfig.ts`: add `PrmScore?: PrmScoreConfig` + interfaces.
4. `visual/PrmScoreboardVisual.ts`: new visual (gradient bg, panels, slots, badge, clock, banner).
5. `scenes/IngameScene.ts`: instantiate `PrmScoreboardVisual` when `cfg.PrmScore` present; forward `UpdateValues`/`UpdateConfig`.
6. Assets: flag images (ISO code → `frontend/images/flags/*.png`), metric icons (tower/grub/dragon/inhib), league/region crests.

## 8. Verification harness

`ocr-poc/overlay-harness/` (Node, no new deps):
- `mock-server.js`: ws server on :9001 that answers the `OverlayConfig` request
  with `mock-config.json` and pushes `GameHeartbeat` with `mock-state.json`
  (reference values: blue/red 7/9, 36.2K/38.5K, towers 3/4, grubs 0/0, dragons 0/3,
  region #2 / region #1, banner, 19:52). Also serves the built overlay + `frontend/` assets.
- Screenshot: `msedge --headless=new --screenshot --window-size=1920,1080
  "http://localhost:PORT/index.html?backend=127.0.0.1"`.
- Compare to `prm_reference.png` (overlay crop). Iterate element‑by‑element.
- **First validate the harness against the existing `ScoreboardVisual`** before
  building the new visual (proves the rig, not the new code).

## 9. Scope & phasing

- **Phase 1 (this effort): the top bar** — signature always‑on element; ~60%
  already exists in `ScoreboardVisual`. Full data wiring + visual match + harness.
- **Phase 2: bottom comparison bar** — 5×2 player cards (champ/KDA/CS/gold/items),
  webcams, sponsor (KitKat), patch label. Needs LCU rosters + /playerlist per‑player.
- **Phase 3: in‑game side elements & animations** — objective pop‑ups, soul, etc.

## 9b. Phase 2 decode — bottom comparison bar

Decoded from `ocr-poc/bb_*.png`. This is a **situational** overlay (analysis / pause),
not always-on like the top bar.

Layout (left → right):
- **Far left:** sponsor (KitKat `@kitkatgaming`), a small item/objective summary row,
  and a **player webcam** slot (featured blue player, vertical name).
- **Center:** a **5-row lane matchup grid** (top/jgl/mid/adc/sup). Each row:
  - blue player: champ icon · 2 summoner spells · items · KDA (e.g. `1/3/2`) · CS (`202`)
  - center: **per-lane gold-diff** with a direction arrow (e.g. `◀0.1K`)
  - red player (mirrored): CS (`182`) · KDA · items · spells · champ icon
- **Far right:** `PATCH 26.11` (vertical) + featured red player webcam; the minimap
  is the game, not overlay.

**Data plan / gaps:**
- Per-player **champ / KDA / CS / items / summoner spells / level** = EXACT from
  `/liveclientdata/playerlist` (+ DataDragon icons). The fork parses playerlist already
  but the frontend `StateData` carries **no per-player roster** — Phase 2 needs a NEW
  per-player data channel (backend → overlay) and DataDragon icon loading in the overlay.
- Per-lane **gold diff** needs **per-player gold**, which this Vanguard-compatible fork
  only **estimates** (no memory reader). Under the strict-accuracy policy (prefer hiding
  to approximating), the gold-diff column should be **hidden/omitted** unless OCR or a
  memory reader supplies exact per-player gold. (Team-total gold IS OCR-exact — that's the
  top bar.)
- Webcams + sponsor + patch = operator-supplied assets / LCU patch version (already fetched).

Phase 2 is therefore a separate, larger build (new data contract + 10-player grid + icon
plumbing) with a deliberate accuracy gap on per-lane gold.

## 10. Open questions / assumptions to re-confirm

- **Outermost counter = Dragons vs Plates vs Inhibitors.** Inhibitors ruled out
  (red 3 inhibs impossible with 4 towers). Defaulting to **Dragons (count)** — the
  marquee objective, absent elsewhere in the bar, and red=3/blue=0 = soul point.
  Because slots are config‑driven, this is a one‑line change if visual diff proves
  it's plates. **Re‑confirm during visual verification.**
- **Spectator `HordeKill` symmetry** (see §3) — validate vs live grub kills.
- **`0:07` top‑left chip** purpose — deferred (auxiliary clock).
- Exact broadcast font — pending bottom‑bar typography pass.
