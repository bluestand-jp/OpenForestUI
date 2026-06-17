import StateData from "~/data/stateData";
import PlayerScoreboardEntry from "~/data/playerScoreboardEntry";
import IngameScene from "~/scenes/IngameScene";
import Vector2 from "~/util/Vector2";
import { PrmScoreConfig } from "~/data/config/overlayConfig";
import { VisualElement } from "./VisualElement";

/**
 * LCK "Road to MSI" style bottom scoreboard (see docs/lck-scoreboard/SPEC.md).
 *
 * A centered band carrying a tournament title, five role rows (TOP/JGL/MID/BOT/SUP),
 * mirrored blue (left) / red (right). Each row shows, reading inner->outer toward the
 * panel edge: champion portrait (+ level) nearest the center, then CS, KDA, then a
 * 6-slot item row. The single gold value rendered is the per-lane gold DIFFERENCE
 * (blue.Gold - red.Gold) in the center gap between the two champions, with an arrow
 * toward the leading side. A patch label sits at the band's lower-left.
 *
 * Layout calibrated against docs/lck-scoreboard/lck_reference.png. Out of scope per
 * operator: the outer edge dmg/vision/KP numbers and the per-player (current) gold
 * column — the fork only has a total-earned estimate, used solely for the lane diff.
 *
 * Reproduce, don't break, reuse: this is OPT-IN (PrmScore.BottomBar === true &&
 * BottomStyle === 'lck'); it never touches the PRM top bar, the PRM bottom bar, or the
 * legacy scoreboard, reads the same per-player data channel, and the opaque panel keeps
 * COVERING the native spectator scoreboard (left open underneath for CS/gold OCR).
 *
 * Champion/item icons load from DataDragon at runtime (version auto-resolved). Colors
 * are placeholder neutrals centralized in COLORS for a later theme pass.
 */

const CENTER = 960;
const DDRAGON_CDN = 'https://ddragon.leagueoflegends.com/cdn';
const DDRAGON_FALLBACK = '16.12.1';
const POS_ORDER = ['TOP', 'JUNGLE', 'MIDDLE', 'BOTTOM', 'UTILITY'];
// Trinkets (Warding Totem / Farsight / Oracle Lens). Pinned to the rightmost item slot regardless of
// gold (they have ~0 gold anyway), so they stay out of the gold-sorted item run.
const TRINKET_IDS = new Set<number>([3340, 3363, 3364]);

// vertical: 5 rows. Row centers calibrated from the reference CS column (879/921/963/
// 1005/1047). The opaque panel COVERS the native spectator scoreboard underneath —
// required frame x[601..1332], y[847..1065] (PRM covers x[580..1340]). We use x[560..1360]
// (matching the LCK band's width between the caster webcams) and run the bottom to the
// screen edge (1080): in dev the taskbar occludes the lower rows so we can't measure them,
// but borderless-fullscreen broadcast extends lower — anchoring at 1080 guarantees cover.
// The native panel is kept OPEN under it for CS/gold OCR (we only occlude it visually).
const ROW_STEP = 42;
const PANEL = { x0: 560, y0: 843, w: 800, h: 237 };
// 5 rows centered vertically within the panel (derived, so it stays centered if the panel changes).
const ROW0 = Math.round(PANEL.y0 + PANEL.h / 2 - 2 * ROW_STEP);
const TITLE_Y = 851;
// Tournament title is hidden for now (operator request). Flip to true to render it again
// (centered band-top, from state TournamentName / cfg.TournamentName).
const SHOW_TITLE = false;
// Patch label is hidden for now (operator request). Flip to true to render it again
// (band lower-left, "PATCH x.y" from the resolved DataDragon version).
const SHOW_PATCH = false;
const PATCH = { x: 574, y: 1068 };

// blue-side column anchors (red mirrors at 1920 - x); inner = toward center. The champion
// square sits NEAREST the center (the reference packs the per-player gold into the gap
// between champions — that pink number is out of scope, so the center gap carries only the
// lane gold-diff). CS is inner of KDA; the 7-slot item grid (6 items + trinket) is the
// outermost reproduced element (the edge dmg/vision/KP numbers and the outer champion
// splash are out of scope). Sizes kept compact so 7 items + KDA + CS + champ fit each half
// without crowding the center — preserving clean left/right symmetry about 960.
const COL = {
    champ: 922,
    champSize: 30,
    champFrame: 34,
    cs: 868,
    csSize: 16,
    kda: 812,
    kdaSize: 16,
    item0: 600,
    itemStep: 28,
    itemSize: 24,
    itemCount: 7,
    goldDiff: CENTER,
    goldDiffSize: 12,
    levelSize: 10,
};

// Depths: panel/frames behind (-1), dynamic icon sprites at 0, text above icons (5),
// level badge above its portrait (6) — so the centered gold-diff is never hidden behind
// the flanking champion portraits.
const DEPTH = { bg: -1, icon: 0, text: 5, level: 6 };

// Team colors come from the shared overlay channel (state.blueColor / redColor) — the same source the
// PRM top bar uses, defaulting to blue/red — so this board's tints match the bar.
const DEFAULT_BLUE = 0x4285f4;
const DEFAULT_RED = 0xea4335;

// PLACEHOLDER neutral palette (colors deferred — single place to edit in a later pass).
const COLORS = {
    panel: 0x0a0e27,
    blueTint: 0x16456b,
    redTint: 0x5b1f57,
    divider: 0xffffff,
    frame: 0x8fa0d0,
    white: '#ffffff',
    cs: '#ffe9a8',
    kda: '#dfe6ff',
    gold: '#8fe39a',     // green lane gold-diff (matches the reference)
    title: '#ffffff',
    patch: '#c9d4ff',
    level: '#ffffff',
};

interface RowRefs {
    blueKda: Phaser.GameObjects.Text; redKda: Phaser.GameObjects.Text;
    blueCs: Phaser.GameObjects.Text; redCs: Phaser.GameObjects.Text;
    blueLvl: Phaser.GameObjects.Text; redLvl: Phaser.GameObjects.Text;
    gold: Phaser.GameObjects.Text;
}

export default class LckScoreboardVisual extends VisualElement {
    cfg: PrmScoreConfig;
    font: string;
    ddVersion: string;

    bg!: Phaser.GameObjects.Graphics;
    title: Phaser.GameObjects.Text | null = null;
    patch: Phaser.GameObjects.Text | null = null;
    rows: RowRefs[] = [];

    // dynamic icon sprites keyed by a stable slot id ("champ:blue:0", "item:red:2:3", ...)
    iconSprites: Map<string, Phaser.GameObjects.Sprite> = new Map();
    iconLoaded: Map<string, string> = new Map();   // slotId -> last url loaded (skip reloads)
    // item id -> total gold (DataDragon item.json, loaded async). Drives the per-row item sort; empty
    // until loaded, in which case items keep their source order (the trinket is still pinned by id).
    itemGold: Map<number, number> = new Map();
    // Resolved team colors (shared channel; default blue/red), used to tint each half + the champion
    // frames so this board matches the PRM top bar. Re-resolved from state; redraw only on a change.
    blueInt = DEFAULT_BLUE;
    redInt = DEFAULT_RED;
    lastBlueStr = '';
    lastRedStr = '';

    constructor(scene: IngameScene, cfg: PrmScoreConfig) {
        super(scene, new Vector2(CENTER, 0), 'lckScoreboard');
        this.cfg = cfg || ({} as PrmScoreConfig);
        this.font = (cfg && cfg.Font) ? cfg.Font : 'News Cycle';
        this.ddVersion = (cfg && cfg.DDragonVersion) ? cfg.DDragonVersion : DDRAGON_FALLBACK;
        this.ResolveDDragonVersion();

        this.bg = scene.add.graphics();
        this.bg.setDepth(DEPTH.bg);
        this.DrawBackground();
        this.AddVisualComponent(this.bg);

        // title (centered, band-top) + patch label (band lower-left). Both hidden for now
        // (SHOW_TITLE / SHOW_PATCH) — flip the flags to render them again.
        if (SHOW_TITLE)
            this.title = this.mkText(CENTER, TITLE_Y, this.titleText(), 16, COLORS.title, 0.5);
        if (SHOW_PATCH)
            this.patch = this.mkText(PATCH.x, PATCH.y, '', 11, COLORS.patch, 0);

        // per-row text scaffolds
        for (let i = 0; i < 5; i++) {
            const y = ROW0 + i * ROW_STEP;
            this.rows.push({
                blueKda: this.mkText(COL.kda, y, '0/0/0', COL.kdaSize, COLORS.kda, 0.5),
                redKda: this.mkText(1920 - COL.kda, y, '0/0/0', COL.kdaSize, COLORS.kda, 0.5),
                blueCs: this.mkText(COL.cs, y, '0', COL.csSize, COLORS.cs, 0.5),
                redCs: this.mkText(1920 - COL.cs, y, '0', COL.csSize, COLORS.cs, 0.5),
                blueLvl: this.mkLevel(this.mirror(COL.champ, false), y, false),
                redLvl: this.mkLevel(this.mirror(COL.champ, true), y, true),
                gold: this.mkText(COL.goldDiff, y, '', COL.goldDiffSize, COLORS.gold, 0.5),
            });
        }

        this.GetActiveVisualComponents().forEach(c => { c.alpha = 0; });
        this.Init();
    }

    private mkText(x: number, y: number, str: string, size: number, color: string, ox: number): Phaser.GameObjects.Text {
        const t = this.scene.add.text(x, y, str, { fontFamily: this.font, fontSize: size + 'px', color, fontStyle: 'bold' });
        t.setOrigin(ox, 0.5);
        t.setDepth(DEPTH.text);
        this.AddVisualComponent(t);
        return t;
    }

    // Champion level, overlaid at the portrait's OUTER bottom corner, stroked for legibility and
    // mirrored per side (blue = bottom-left, red = bottom-right) so the two are symmetric about center.
    private mkLevel(champX: number, y: number, red: boolean): Phaser.GameObjects.Text {
        const half = COL.champSize / 2;
        const x = red ? champX + half - 1 : champX - half + 1;
        const t = this.scene.add.text(x, y + half - 1, '',
            { fontFamily: this.font, fontSize: COL.levelSize + 'px', color: COLORS.level, fontStyle: 'bold' });
        t.setOrigin(red ? 1 : 0, 1);
        t.setStroke('#000000', 3);
        t.setDepth(DEPTH.level);
        this.AddVisualComponent(t);
        return t;
    }

    private DrawBackground(): void {
        this.bg.clear();
        // Fully opaque: this panel's job is to occlude the native scoreboard beneath it.
        this.bg.fillStyle(COLORS.panel, 1);
        this.bg.fillRect(PANEL.x0, PANEL.y0, PANEL.w, PANEL.h);
        // Team-colored halves: each team's color strongest at its OUTER edge, fading to the dark
        // center — echoes the PRM top bar's team-color -> indigo gradient. Drawn over the opaque base.
        const blueW = CENTER - PANEL.x0 - 28;
        this.bg.fillGradientStyle(this.blueInt, COLORS.panel, this.blueInt, COLORS.panel, 0.5);
        this.bg.fillRect(PANEL.x0, PANEL.y0, blueW, PANEL.h);
        const redX = CENTER + 28;
        this.bg.fillGradientStyle(COLORS.panel, this.redInt, COLORS.panel, this.redInt, 0.5);
        this.bg.fillRect(redX, PANEL.y0, (PANEL.x0 + PANEL.w) - redX, PANEL.h);
        // (center divider removed per request)
        // champion portrait frames, tinted per team (5 rows x 2 sides)
        for (let i = 0; i < 5; i++) {
            const y = ROW0 + i * ROW_STEP;
            const f = COL.champFrame;
            this.bg.lineStyle(1, this.blueInt, 0.9);
            this.bg.strokeRect(this.mirror(COL.champ, false) - f / 2, y - f / 2, f, f);
            this.bg.lineStyle(1, this.redInt, 0.9);
            this.bg.strokeRect(this.mirror(COL.champ, true) - f / 2, y - f / 2, f, f);
        }
    }

    // Resolve the team colors from the shared overlay channel (state.blueColor / redColor; default
    // blue/red), same as the PRM bar. Returns true only on a change so the caller redraws once.
    private resolveTeamColors(state: StateData): boolean {
        const b = state.blueColor || '';
        const r = state.redColor || '';
        if (b === this.lastBlueStr && r === this.lastRedStr) return false;
        this.lastBlueStr = b;
        this.lastRedStr = r;
        const bc = b ? Phaser.Display.Color.RGBStringToColor(b) : Phaser.Display.Color.IntegerToColor(DEFAULT_BLUE);
        const rc = r ? Phaser.Display.Color.RGBStringToColor(r) : Phaser.Display.Color.IntegerToColor(DEFAULT_RED);
        this.blueInt = bc.color;
        this.redInt = rc.color;
        return true;
    }

    private ResolveDDragonVersion(): void {
        try {
            fetch('https://ddragon.leagueoflegends.com/api/versions.json')
                .then(r => r.json())
                .then((v: string[]) => { if (Array.isArray(v) && v.length) this.ddVersion = v[0]; })
                .catch(() => { /* keep fallback/config */ })
                .finally(() => this.LoadItemData());   // load item gold once the version is resolved
        } catch { this.LoadItemData(); }
    }

    // Fetch DataDragon item.json once and cache id -> total gold, so each player's items can be sorted
    // most-expensive-first. Gold values are locale-independent, so en_US is fine.
    private LoadItemData(): void {
        try {
            fetch(`${DDRAGON_CDN}/${this.ddVersion}/data/en_US/item.json`)
                .then(r => r.json())
                .then((j: any) => {
                    const data = (j && j.data) ? j.data : {};
                    for (const id in data) {
                        const total = (data[id] && data[id].gold) ? data[id].gold.total : 0;
                        this.itemGold.set(Number(id), total || 0);
                    }
                })
                .catch(() => { /* leave empty -> items keep source order */ });
        } catch { /* ignore */ }
    }

    private titleText(): string {
        return (this.cfg && this.cfg.TournamentName) ? this.cfg.TournamentName : '';
    }

    // "PATCH 26.11" from the resolved DataDragon version's major.minor (the closest real,
    // already-fetched version data — there is no separate game-patch field in the DTO).
    private patchText(): string {
        const parts = (this.ddVersion || '').split('.');
        if (parts.length >= 2 && parts[0] && parts[1]) return 'PATCH ' + parts[0] + '.' + parts[1];
        return '';
    }

    UpdateValues(state: StateData): void {
        const sb = state.scoreboard;
        const players: PlayerScoreboardEntry[] = (sb && (sb as any).Players) ? (sb as any).Players : [];
        if (!players || players.length < 2) {
            // Hold the last roster on a transient frame with no players (loading / data gap) rather
            // than hiding the bar. Lifecycle hide stays with IngameScene (GameEnd / disconnect) and
            // the BottomBar config toggle.
            return;
        }

        // Re-skin the panel halves + champ frames to the current team colors if they changed (shared
        // channel; default blue/red) — keeps this board in step with the PRM top bar.
        if (this.resolveTeamColors(state)) this.DrawBackground();

        // title prefers the live tournament name, falling back to the configured one
        if (SHOW_TITLE && this.title) {
            const tn = (sb && (sb as any).TournamentName) ? (sb as any).TournamentName : this.titleText();
            if (this.title.text !== tn) this.title.text = tn;
        }
        if (SHOW_PATCH && this.patch) {
            const px = this.patchText();
            if (this.patch.text !== px) this.patch.text = px;
        }

        const blue = this.sortByRole(players.filter(p => p.Team === 'ORDER'));
        const red = this.sortByRole(players.filter(p => p.Team === 'CHAOS'));

        for (let i = 0; i < 5; i++) {
            this.applyRow(i, false, blue[i]);
            this.applyRow(i, true, red[i]);
            this.applyLaneGoldDiff(i, blue[i], red[i]);
        }

        if (!this.isActive) this.Start();
    }

    /**
     * The single gold value the LCK board shows: per-lane gold DIFFERENCE in the center
     * gap between the two champions (blue.Gold - red.Gold), rounded to 0.1k units (k with one
     * decimal, e.g. 3060 -> "3.1k", 60 -> "0.1k"), with an ASCII arrow toward the leading side
     * (News Cycle lacks the unicode arrows). Gold is the backend total-earned estimate.
     */
    private applyLaneGoldDiff(i: number, blue?: PlayerScoreboardEntry, red?: PlayerScoreboardEntry): void {
        const g = this.rows[i].gold;
        if (!blue || !red || blue.Gold === undefined || red.Gold === undefined) { g.text = ''; return; }
        const diff = Math.round((blue.Gold || 0) - (red.Gold || 0));
        const amount = (Math.abs(diff) / 1000).toFixed(1) + 'k';
        // Lanes effectively even (|diff| rounds to 0.0k): show the value with NO arrow — an arrow
        // here would falsely imply one side leads. Otherwise the arrow points toward the leader.
        g.text = amount === '0.0k' ? amount : (diff > 0 ? '< ' + amount : amount + ' >');
    }

    private sortByRole(list: PlayerScoreboardEntry[]): PlayerScoreboardEntry[] {
        const known = POS_ORDER.map(pos => list.find(p => p.Position === pos)).filter(Boolean) as PlayerScoreboardEntry[];
        // if positions are missing/duplicated, fall back to input order
        return known.length === list.length ? known : list;
    }

    private applyRow(i: number, red: boolean, p: PlayerScoreboardEntry | undefined): void {
        const r = this.rows[i];
        const side = red ? 'red' : 'blue';
        const y = ROW0 + i * ROW_STEP;
        const kdaT = red ? r.redKda : r.blueKda;
        const csT = red ? r.redCs : r.blueCs;
        const lvlT = red ? r.redLvl : r.blueLvl;
        if (!p) {
            kdaT.text = ''; csT.text = ''; lvlT.text = '';
            this.clearIcon(`champ:${side}:${i}`);
            for (let s = 0; s < COL.itemCount; s++) this.clearIcon(`item:${side}:${i}:${s}`);
            return;
        }

        kdaT.text = `${p.Kills}/${p.Deaths}/${p.Assists}`;
        csT.text = `${p.CreepScore}`;
        lvlT.text = (p.Level !== undefined && p.Level !== null) ? `${p.Level}` : '';

        // champion portrait (innermost, near center) + level overlay
        if (p.ChampionID)
            this.icon(`champ:${side}:${i}`, this.champUrl(p.ChampionID), this.mirror(COL.champ, red), y, COL.champSize, COL.champSize);

        // Item row is MIRRORED about center (operator request: blue mirrors red, like the rest of the
        // board): highest-gold item nearest the center champion, trinket pinned to the OUTERMOST slot.
        // Non-trinket items are sorted by DataDragon gold (until item.json loads they keep source order;
        // the trinket is pinned by id regardless). Both sides use the same slot->position formula and
        // red's x is reflected via mirror(), so blue is an exact reflection of red.
        const present = (p.Items || []).filter(id => id > 0);
        const trinketId = present.find(id => TRINKET_IDS.has(id)) ?? 0;
        const regulars = present.filter(id => id !== trinketId)
            .sort((a, b) => (this.itemGold.get(b) ?? 0) - (this.itemGold.get(a) ?? 0));
        for (let s = 0; s < COL.itemCount; s++) {
            const id = (s === COL.itemCount - 1) ? trinketId : (regulars[s] ?? 0);
            const posIndex = COL.itemCount - 1 - s;
            const ix = this.mirror(COL.item0 + posIndex * COL.itemStep, red);
            const key = `item:${side}:${i}:${s}`;
            if (id > 0) this.icon(key, this.itemUrl(id), ix, y, COL.itemSize, COL.itemSize);
            else this.clearIcon(key);
        }
    }

    private mirror(x: number, red: boolean): number { return red ? 1920 - x : x; }
    private champUrl(id: string): string { return `${DDRAGON_CDN}/${this.ddVersion}/img/champion/${id}.png`; }
    private itemUrl(id: number): string { return `${DDRAGON_CDN}/${this.ddVersion}/img/item/${id}.png`; }

    /** Load (if needed) a DataDragon image into a keyed texture and place/refresh its sprite. */
    private icon(slotId: string, url: string, x: number, y: number, w: number, h: number): void {
        if (this.iconLoaded.get(slotId) === url) {
            const s = this.iconSprites.get(slotId);
            if (s) { s.setPosition(x, y); s.setDisplaySize(w, h); if (this.isActive) s.alpha = 1; }
            return;
        }
        this.iconLoaded.set(slotId, url);
        const texKey = 'dd_' + this.hash(url);
        const place = () => this.placeIcon(slotId, texKey, x, y, w, h);
        if (this.scene.textures.exists(texKey)) { place(); return; }
        this.scene.load.image(texKey, url);
        this.scene.load.once('filecomplete-image-' + texKey, place);
        this.scene.load.start();
    }

    private placeIcon(slotId: string, texKey: string, x: number, y: number, w: number, h: number): void {
        // stale guard: only place if this slot still wants this texture
        const old = this.iconSprites.get(slotId);
        if (old) { this.RemoveVisualComponent(old); old.destroy(); }
        if (!this.scene.textures.exists(texKey)) return;
        const s = this.scene.make.sprite({ x, y, key: texKey, add: true });
        s.setOrigin(0.5, 0.5);
        s.setDisplaySize(w, h);
        s.setDepth(DEPTH.icon);
        s.alpha = this.isActive ? 1 : 0;
        this.iconSprites.set(slotId, s);
        this.AddVisualComponent(s);
    }

    private clearIcon(slotId: string): void {
        const s = this.iconSprites.get(slotId);
        if (s) { this.RemoveVisualComponent(s); s.destroy(); this.iconSprites.delete(slotId); }
        this.iconLoaded.delete(slotId);
    }

    private hash(s: string): string {
        let h = 0;
        for (let i = 0; i < s.length; i++) { h = (h << 5) - h + s.charCodeAt(i); h |= 0; }
        return (h >>> 0).toString(36);
    }

    Load(): void { /* built in constructor */ }
    UpdateConfig(newConfig: PrmScoreConfig): void {
        this.cfg = newConfig || this.cfg;
        if (newConfig && newConfig.DDragonVersion) this.ddVersion = newConfig.DDragonVersion;
        if (newConfig && newConfig.Font) this.font = newConfig.Font;
    }
    Start(): void {
        if (this.isActive) return;
        this.isActive = true;
        this.GetActiveVisualComponents().forEach(c => { c.alpha = 1; });
        this.iconSprites.forEach(s => { s.alpha = 1; });
    }
    Stop(): void {
        if (!this.isActive) return;
        this.isActive = false;
        this.GetActiveVisualComponents().forEach(c => { c.alpha = 0; });
        this.iconSprites.forEach(s => { s.alpha = 0; });
    }
}
