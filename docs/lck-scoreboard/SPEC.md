# In‑Game Comparison Scoreboard Overlay — Spec & Build Instruction

> **Goal:** reproduce the **pro-broadcast bottom comparison scoreboard
> _layout_** in this fork's ingame overlay, **while keeping the existing broadcast top
> bar unchanged** (`PrmScoreboardVisual`). The user wants **broadcast top bar +
> comparison scoreboard** together.
>
> **Layout fidelity first. Colors/theme are explicitly deferred** — use neutral
> placeholder colors and centralize them as named constants so a later color pass is
> a one‑file change, not a re‑layout.
>
> **Reproduce, don't break, reuse.** New code is **opt‑in**; the legacy
> `ScoreboardVisual`, the top-bar `PrmScoreboardVisual`/`PrmBottomBarVisual`, the WS
> contract, the per‑player data channel, and the OCR pipeline must all keep working
> unchanged. This file is the `/goal` target — verify recursively against §8/§9/§10.

> **Reference image:** **saved at `docs/lck-scoreboard/lck_reference.png`** (a pro match,
> 1917×1077 ≈ 1920×1080, the event/series title, patch 26.11). All geometry in §7 is
> approximate and **must be calibrated against that file** in the harness (§9), the
> same way the top bar was calibrated against `docs/prm-overlay/prm_reference.png`.

## Implementation status
- **Implemented & harness‑verified (2026‑06‑14).** New `LckScoreboardVisual.ts` +
  `BottomStyle?: 'prm'|'lck'` on both config sides; wired into `IngameScene` as a sibling
  of `prmBottom`. Frontend `npm run build` and `dotnet build OpenForestUI.sln` both **0
  errors**. Harness (`ocr-poc/overlay-harness`, 10‑player mock, `Enabled=true`,
  `BottomBar=true`, `BottomStyle='lck'`) renders **broadcast top bar + comparison
  scoreboard together**;
  OCR‑region occlusion x[601..1332] y[847..1065] measured **100% opaque**.
- **Reference‑decoded layout (overrides the §7 placeholder anchors, as §2/§9/§11 allow):**
  reading inner→outer per side, the order is **champion(+level, nearest center) → CS →
  KDA → 6‑item grid**; the outer champion *splash* + edge dmg number and the inner pink
  per‑player gold are the out‑of‑scope clusters and are omitted. The lone green lane
  gold‑diff is rendered **centered between the two champions** (per §2/§7), `…G`, arrow to
  the leader. Calibrated constants (1920×1080), blue side (red mirrors `1920−x`):
  rows `ROW0=879 STEP=42`; `champ=920` (size 34, frame 38); `cs=886`; `kda=824`;
  `item0=610 step=35 size=28` ×6; `goldDiff=960` (13px); panel `x[560..1360] y[843..1080]`;
  title centered `y≈851` (from `TournamentName`); patch lower‑left (`PATCH x.y` from the
  resolved DataDragon version). Colors remain placeholder neutrals in one `COLORS` block.
- **Operator decisions baked in (2026‑06‑14):**
  - edge dmg/vision/KP numbers are **out of scope** (not built);
  - the gold display is **only the center lane gold‑diff** (blue−red) — **no per‑player
    gold column** (the fork has only a total‑earned estimate, not current gold; §3).

## Table of Contents
1. [Scope & non‑goals](#1-scope--non-goals)
2. [Reference decode — comparison scoreboard layout](#2-reference-decode--comparison-scoreboard-layout)
3. [Metric → data‑source map](#3-metric--data-source-map)
4. [Architecture / design](#4-architecture--design)
5. [Frontend changes (file‑by‑file)](#5-frontend-changes-file-by-file)
6. [Backend changes (config flag only — no new data)](#6-backend-changes-config-flag-only--no-new-data)
7. [Layout spec (1920×1080 geometry)](#7-layout-spec-19201080-geometry)
8. [Operating‑logic invariants to preserve (do NOT break)](#8-operating-logic-invariants-to-preserve-do-not-break)
9. [Verification harness](#9-verification-harness)
10. [Acceptance criteria](#10-acceptance-criteria)
11. [Open questions / calibrate against reference](#11-open-questions--calibrate-against-reference)

---

## 1. Scope & non‑goals

**In scope**
- A new **`LckScoreboardVisual`** (Phaser `VisualElement`) reproducing the comparison
  scoreboard **layout**: a centered band with a tournament title, a 5‑row
  role matchup (TOP/JGL/MID/BOT/SUP), mirrored blue (left) / red (right), each row
  showing **champion · items · KDA · CS**, with a **center lane gold‑diff** (blue−red),
  plus a patch label.
- A **config discriminator** so the existing opt‑in bottom bar can render either the
  top-bar style (existing) or the new comparison-scoreboard style — letting the
  **broadcast top bar + comparison scoreboard** coexist.
- Reuse of the **existing per‑player data** (`state.scoreboard.Players` →
  `PlayerScoreboardEntry`) and the **existing DataDragon icon loader** pattern.

**Non‑goals (this effort)**
- **Colors / exact theme / fonts** — deferred. Placeholder neutral palette only,
  centralized in a `COLORS`/`STYLE` const block.
- **The outer‑edge numbers** (`25/50/26/39/127` left, `42/56/45/43/141` right) — these
  are damage / vision / KP. **Out of scope per operator — do not build them.**
- **A per‑player gold column** (the pink `1.8K/769…` per player) — **out of scope per
  operator: show only the lane gold‑diff.** The fork has only a total‑earned *estimate*
  (not the current gold the board shows) — see §3.
- Player **webcams**, **sponsor**, and the bottom‑left **native champion detail
  panel** and **minimap** — broadcast/operator assets and the game's own HUD, not overlay.
- Any **new live data source** (damage, vision/ward, exact current per‑player gold). See §3.
- Touching the broadcast top bar, the legacy scoreboard, or backend data shape.

---

## 2. Reference decode — comparison scoreboard layout

From `lck_reference.png` (a pro match). The reproduction target is **only the
center‑bottom scoreboard band**, between the two caster
webcams. (The far left/right vertical player strips, the bottom‑left item panel, and
the minimap are the **native LoL spectator HUD / game**, not overlay — leave them.)

**Band structure (top → bottom):**
- **Title strip:** the event/series title centered along the top of the band.
- **5 rows**, one per role, lane order **TOP, JGL, MID, BOT, SUP** (top→bottom).
- A **branding/patch block** (the event name + patch, e.g. … / PATCH 26.11) at the band's lower‑left.

**Per‑row columns (blue/left half; red mirrors about screen center).** Verified from
the saved reference, reading **outer → inner** (toward center):

| Column | Source | Status |
|---|---|---|
| ~~edge number~~ | dmg/vision/KP | ❌ **out of scope — skip** |
| **item row** (6 slots) | `Items[0..6]` | ✅ |
| **KDA** (`0/1/2`) | `Kills/Deaths/Assists` | ✅ |
| **CS** (`295 … 30`) | `CreepScore` | ✅ |
| **champion portrait** (+ level) | `ChampionID` (DataDragon) + `Level` | ✅ |
| ~~gold (per player)~~ (pink `1.8K/769…`) | current gold (not available) | ❌ **out of scope — lane diff only (§3)** |

**Center (between the two champions):** a small **lane gold value** with a `G` suffix
(`534G / 235G / 224G …`) = the lane **gold difference**. ✅ maps to `blue.Gold − red.Gold`
(the estimate's designed use; the broadcast top bar's bottom bar already computes this).
**This is the only gold shown.**

So the reproduced row, blue outer→inner, is:
`items → KDA → CS → champion`, then center `lane gold‑diff` (blue−red), mirrored on red.
**No per‑player gold number is rendered** (operator decision). Exact inner/outer ordering
and spacing are **calibrated against the reference in §9**.

---

## 3. Metric → data‑source map

All per‑player data already arrives over WS in `state.scoreboard.Players`
(`PlayerScoreboardEntry`, built backend‑side in `State.UpdateScoreboard` for the
top bar's Phase 2). **No new backend data is required.**

| Scoreboard element | Source field | Status |
|---|---|---|
| Champion portrait | `ChampionID` → DataDragon `img/champion/{id}.png` | ✅ (loader exists) |
| Level | `Level` | ✅ |
| Items (0..6) | `Items: number[]` → `img/item/{id}.png` (0 = empty) | ✅ |
| Summoner spells (if shown) | `Spells: string[2]` → `img/spell/{key}.png` | ✅ |
| KDA | `Kills` / `Deaths` / `Assists` | ✅ |
| CS | `CreepScore` | ✅ |
| **Center lane gold‑diff** (`534G…`) | `blue.Gold − red.Gold` (cf. `PrmBottomBarVisual.applyLaneGoldDiff`) | ✅ **the gold display** |
| ~~Per‑player gold~~ (pink `1.8K…`) | current gold (unavailable) | ❌ **out of scope** |
| Tournament title | `state.scoreboard.TournamentName` (or `PrmScore.TournamentName`) | ✅ (top bar Phase 1) |
| Patch label | LCU game‑version (already fetched; the top bar's bottom bar shows "PATCH 26.11") | ✅ |

**Gold — what's available, precisely.** `Team.EstimatePlayerGold` (OpenForestUI/Ingame/
Data/Hub/Team.cs:132) estimates **total‑earned gold** from `/playerlist` fields
(CS/kills/assists + passive curve), ~±5‑10%, explicitly designed as a **gold‑difference
indicator** (no memory reader under Vanguard; `/playerlist` has no per‑player gold). So:
- ✅ **Center lane gold‑diff is well supported** — build it from `blue.Gold − red.Gold`
  (this is what the estimate is for, and what the top bar already shows). **This is the
  only gold the comparison scoreboard renders.**
- ❌ **The per‑player pink number** (`1.8K/769/213` — low values) is almost certainly
  **current/unspent gold**, which the fork does **not** have (its estimate is
  total‑earned, ~10‑15K, a different magnitude). **Operator decision: do not build a
  per‑player gold column.**

**Hard constraint (strict‑accuracy, project memory):** damage and ward/vision are not
exposed by `/playerlist` and are **not** in the DTO — that is why the edge numbers are
out of scope. Do not invent unavailable metrics.

---

## 4. Architecture / design

Follow the **exact pattern** of the top bar's bottom bar — see
[`docs/prm-overlay/SPEC.md`](../prm-overlay/SPEC.md) §5/§9b and
[`Overlays/ingame/src/visual/PrmBottomBarVisual.ts`](../../Overlays/ingame/src/visual/PrmBottomBarVisual.ts)
(the template).

1. **New visual `LckScoreboardVisual.ts`** extending `VisualElement`
   ([`visual/VisualElement.ts`](../../Overlays/ingame/src/visual/VisualElement.ts)).
   - Construct scaffolds in the ctor, `alpha = 0`, then `Init()`.
   - Implement `Load`, `UpdateValues(state)`, `UpdateConfig(cfg)`, `Start`, `Stop`.
   - **Copy the DataDragon icon loader verbatim** from `PrmBottomBarVisual`
     (`icon/placeIcon/clearIcon/hash/champUrl/itemUrl/spellUrl/ResolveDDragonVersion`,
     lines ~225‑268). (Optional, lower priority: extract into a shared `visual/ddIcons.ts`
     used by both visuals — only if it does **not** risk regressing the working top bar;
     otherwise duplicate — safety first.)
   - Reuse the same data read: `const players = state.scoreboard?.Players`; filter
     `Team === 'ORDER'` (blue) / `'CHAOS'` (red); `sortByRole` with
     `['TOP','JUNGLE','MIDDLE','BOTTOM','UTILITY']`.
   - Center lane gold‑diff: copy `applyLaneGoldDiff` (top-bar lines ~169‑186) — `blue.Gold
     − red.Gold`, formatted `…G`, arrow toward the leader. **This is the only gold cell.**

2. **Config discriminator** (minimal, additive). Add `BottomStyle?: 'prm' | 'lck'`
   to the existing `PrmScoreConfig` (C# + TS):
   - `BottomBar === true` keeps gating *whether* a bottom bar shows (live toggle).
   - `BottomStyle === 'lck'` → instantiate `LckScoreboardVisual` (the comparison
     scoreboard); otherwise (`'prm'` / absent) → `PrmBottomBarVisual` (unchanged default).
   - Operator runs **`PrmScore.Enabled=true` (broadcast top bar) + `BottomBar=true` +
     `BottomStyle='lck'` (comparison scoreboard)** simultaneously.

3. **Wiring in `IngameScene`**
   ([`scenes/IngameScene.ts`](../../Overlays/ingame/src/scenes/IngameScene.ts)):
   mirror the existing `prmBottom` handling exactly (fields ~37‑41; per‑tick
   `UpdateValues` ~308‑320; `UpdateConfig` create/re‑enable ~382‑415). Add a sibling
   `lckScoreboard: LckScoreboardVisual | null = null` and select which to
   construct/feed by `BottomStyle`, keeping the same live gate
   (`if (cfg?.PrmScore?.BottomBar === true) this.<bar>.UpdateValues(state)`).

**No new DTO. No WS‑contract change. No change to top-bar/legacy visuals.**

---

## 5. Frontend changes (file‑by‑file)

1. **`Overlays/ingame/src/data/config/overlayConfig.ts`** — add to
   `interface PrmScoreConfig`:
   ```ts
   // Bottom-scoreboard style when BottomBar is on. 'prm' (default/absent) =
   // PrmBottomBarVisual; 'lck' = LckScoreboardVisual.
   BottomStyle?: 'prm' | 'lck';
   ```

2. **`Overlays/ingame/src/visual/LckScoreboardVisual.ts`** — NEW. Structure mirrors
   `PrmBottomBarVisual`:
   - Constants: `CENTER = 960`, `POS_ORDER`, DataDragon CDN + fallback `'16.12.1'`.
   - A `STYLE`/`COLORS` const block with **placeholder neutral colors** (§7) — the
     single place a later color pass edits.
   - `PANEL` + per‑column anchors per §7; `mirror(x, red) => red ? 1920 - x : x`.
   - Row scaffolds (title, KDA, CS, center gold‑diff texts) + dynamic champion/item
     icon sprites. (**No per‑player gold text — lane diff only.**)
   - `UpdateValues`: **hold last roster** on a transient empty frame (`<2` players) —
     copy the top bar's guard; `Start()` on first valid roster.
   - `Start/Stop`: alpha‑toggle all components **and** all icon sprites (copy the top bar).

3. **`Overlays/ingame/src/scenes/IngameScene.ts`**:
   - import `LckScoreboardVisual`; add `lckScoreboard: LckScoreboardVisual | null = null`.
   - In `UpdateConfig`, where `prmBottom` is created (BottomBar branch ~382 + re‑enable
     ~412): choose by `message.config.PrmScore?.BottomStyle`.
   - In per‑tick `UpdateValues` (~317): feed whichever bottom bar exists, keeping the
     `BottomBar === true` gate.
   - Ensure the `GameEnd`/disconnect hide path that stops `prmBottom` also stops
     `lckScoreboard`.

4. **Assets:** none new — champion/item/spell icons via DataDragon; title/patch use
   existing fonts (`PrmScore.Font`, default `News Cycle`).

---

## 6. Backend changes (config flag only — no new data)

1. **`OpenForestUI/Ingame/Data/Config/IngameConfig.cs`** — add to the nested
   `PrmScoreConfig` (~line 37‑44):
   ```csharp
   public string BottomStyle;   // "prm" (default/null) | "lck"
   ```
   Additive to a nested optional object ⇒ backward‑compatible (old `Component.json`
   loads with `BottomStyle == null` ⇒ treated as "prm"). Bump `FileVersion` /
   `UpdateConfigVersion` **only if** the migration framework requires it for new
   fields (the existing top-bar fields were added additively, so a null default
   suffices — verify).
   Optionally set it in `CreateDefault` (leave `null`/"prm").

2. **No change** to `PlayerScoreboardEntry`, `State.UpdateScoreboard`,
   `ScoreboardConfig`, `Team.EstimatePlayerGold`, or WS serialization — the comparison
   scoreboard consumes the **identical** roster the top bar's bottom bar already receives.

3. Build target **.NET 6** (`dotnet 6.0.428`). `dotnet build OpenForestUI.sln -c Debug`
   must be **0 errors**.

> **Dual‑write pitfall (project memory):** a new config field must be added on **both**
> the C# (`IngameConfig.PrmScoreConfig`) and TS (`overlayConfig.ts PrmScoreConfig`)
> sides or it silently won't reach the overlay.

---

## 7. Layout spec (1920×1080 geometry)

> **Starting estimates — calibrate against `lck_reference.png` in the harness (§9).**
> Mirror: red `x = 1920 − blueX`. Center `= 960`.

**Band / panel**
- Centered horizontal band along the screen bottom (between the webcams). Start from
  the top bar's envelope and widen toward the reference look: `PANEL ≈ { x0: 470, y0:
  under title, w: 980, h: ~250 }`, bottom anchored at the screen edge (1080).
- **OCR‑safe occlusion (mandatory, §8):** the opaque panel **must fully cover the
  native spectator scoreboard region** x[601..1332], y[847..1065] (the top bar covers
  x[580..1340], y[843..1080]) so the raw native board never shows on broadcast.

**Rows**
- 5 rows; `ROW0 ≈ 905`, `ROW_STEP ≈ 44` (tune so 5 fit under the title). TOP/JGL/MID/BOT/SUP.

**Blue‑side column anchors (red mirrors); inner = toward center**
| slot | approx x | size | notes |
|---|---|---|---|
| champion | ~520 | 36 | + level text overlaid (corner) |
| items 0..5 | from ~565, step ~26 | ~24 | fixed 6‑slot grid; empty slot = blank |
| KDA | ~770 | 17px text | `K/D/A` |
| CS | ~845 | 19px text | `CreepScore` |
| center lane gold‑diff | 960 | 15px text | `blue.Gold − red.Gold`, `…G`, arrow to leader — **only gold cell** |

**Title & patch**
- Title `TournamentName` (fallback `PrmScore.TournamentName`), centered band‑top (~`y 875`).
- Patch label from LCU game‑version, band lower‑left (reuse the top bar's bottom bar approach).

**Colors (PLACEHOLDER — deferred):** centralize in one `COLORS` block (e.g. `panel
0x141433`, neutral blue/red tints, white text, muted sub). **Do not chase the broadcast
hues now.**

---

## 8. Operating‑logic invariants to preserve (do NOT break)

Core of the `/goal` directive ("壊さず・再現し・利用する"). Re‑verify each recursively.

1. **Opt‑in / isolation.** The comparison scoreboard renders **only** when
   `PrmScore.BottomBar === true && BottomStyle === 'lck'`. Off (default) ⇒
   byte‑for‑byte the legacy/top-bar path. **Never** modify `ScoreboardVisual`,
   `PrmScoreboardVisual`, or `PrmBottomBarVisual` behavior; the broadcast top bar must
   keep rendering unchanged.
2. **Reuse the data channel.** Read `state.scoreboard.Players` only; no new WS message
   or DTO. Field names are the cross‑boundary contract — don't rename.
3. **Hold‑last‑roster.** On `<2` players (loading / gap), **return without hiding**
   (copy `PrmBottomBarVisual.UpdateValues`). Lifecycle hide stays owned by `IngameScene`
   (GameEnd / disconnect) + the `BottomBar` toggle.
4. **Lifecycle & show/hide.** `Init()` in ctor, `Start()` on first valid roster,
   alpha‑toggle components **and** icon sprites in `Start/Stop` (copy the top bar exactly).
5. **DataDragon version.** Resolve via `versions.json` + baked fallback `16.12.1`; lazy
   hashed‑texture load, skip reloads, stale‑guard placement (copy the top bar's loader).
6. **OCR pipeline unaffected.** The native spectator scoreboard is the source the fork
   **OCRs** for CS/gold (`OcrGoldController`); `AutoInitUI` is intentionally **off** so
   that board stays **open underneath**. The comparison scoreboard must **cover it
   visually** (§7) but **must not** toggle the native HUD off or change OCR inputs. After
   implementing,
   confirm CS values still update live.
7. **Gold is an estimate (total‑earned), used only for the lane gold‑diff;** current
   per‑player gold, damage, and ward are unavailable — omit, don't approximate (§3).
8. **Build green & app intact.** `dotnet build` 0 errors; `npm run build` succeeds;
   `/frontend` still serves (C# server unchanged).

---

## 9. Verification harness

Use the existing rig — [`ocr-poc/overlay-harness/`](../../ocr-poc/overlay-harness/)
(see `docs/prm-overlay/SPEC.md` §8):
1. **First** run the harness with the **known‑good top-bar bottom bar** to prove the rig.
2. Feed a **10‑player mock roster** (reuse the top bar's Phase‑2 mock state) and a config
   with `PrmScore.Enabled=true`, `BottomBar=true`, `BottomStyle='lck'`.
3. Headless screenshot:
   `msedge --headless=new --screenshot --window-size=1920,1080 "http://localhost:PORT/index.html?backend=127.0.0.1"`.
4. **Composite vs `docs/lck-scoreboard/lck_reference.png`** and iterate
   **element‑by‑element** (panel envelope → rows → columns → gold‑diff → title/patch).
   Calibrate the §7 anchors here.
5. Confirm every DataDragon champ/item icon loads (no broken textures).

---

## 10. Acceptance criteria

- [ ] `dotnet build OpenForestUI.sln` = **0 errors**; `npm run build` (Overlays/ingame) succeeds.
- [ ] With `PrmScore.Enabled=true, BottomBar=true, BottomStyle='lck'`: the **broadcast
      top bar renders unchanged** AND a **comparison scoreboard** renders the live
      10‑player roster — 5 mirrored role rows of **champion(+level) · items · KDA · CS**,
      a **center lane gold‑diff**, plus tournament title + patch label.
- [ ] **Edge dmg/vision/KP numbers are absent**, and **no per‑player gold column** is
      rendered (both out of scope).
- [ ] The comparison scoreboard panel **fully covers** the native scoreboard region
      (x[601..1332], y[847..1065]); the native board is not visible on broadcast.
- [ ] **CS/gold OCR still updates** live (native board untouched underneath).
- [ ] **Off by default:** with `BottomStyle` absent / `BottomBar` false, the legacy +
      top-bar paths unchanged (no regression).
- [ ] Gold is shown **only** as the **center lane gold‑diff** (from the estimate).
- [ ] Colors are obvious neutral placeholders, centralized for a later pass.
- [ ] Harness composite vs `lck_reference.png` matches in **layout** (positions/sizes);
      color mismatch is expected/acceptable now.

---

## 11. Open questions / calibrate against reference

- **Exact geometry & fonts** — calibrate §7 anchors against `lck_reference.png`; font
  face is deferred with the color pass.
- **Always‑on vs situational** — the comparison scoreboard appears during live play; the
  top bar's bottom bar is a toggle. Default to the existing **`BottomBar` live‑toggle**
  pattern; confirm the operator's desired trigger.
- **Resolved (operator):** edge numbers = out of scope; gold = **center lane gold‑diff
  only** (no per‑player gold column).
- **Out of scope:** webcams / sponsor / native detail panel / minimap (broadcast/game).
