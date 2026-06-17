import StateData from "~/data/stateData";
import PlayerScoreboardEntry from "~/data/playerScoreboardEntry";
import IngameScene from "~/scenes/IngameScene";
import Vector2 from "~/util/Vector2";
import { PrmScoreConfig } from "~/data/config/overlayConfig";
import { VisualElement } from "./VisualElement";

/**
 * PRM bottom comparison bar (Phase 2 — see docs/prm-overlay/SPEC.md §9b).
 *
 * A 5-row lane matchup grid (TOP/JGL/MID/BOT/SUP). Each row shows, mirrored about
 * center, both players' champion icon + summoner spells + items + KDA + CS + level —
 * all EXACT from /playerlist. The per-lane gold diff that the real broadcast shows in
 * the center is deliberately omitted: this Vanguard-compatible fork only *estimates*
 * per-player gold, and the project policy prefers hiding to approximating.
 *
 * Champion/item/spell icons load from DataDragon at runtime (version auto-resolved).
 * Opt-in via OverlayConfig.PrmScore.BottomBar; instantiated by IngameScene.
 */

const CENTER = 960;
const DDRAGON_CDN = 'https://ddragon.leagueoflegends.com/cdn';
const DDRAGON_FALLBACK = '16.12.1';
const POS_ORDER = ['TOP', 'JUNGLE', 'MIDDLE', 'BOTTOM', 'UTILITY'];

// vertical: 5 rows. Geometry calibrated (1920x1080) so the opaque panel COVERS the
// native spectator detail scoreboard underneath — measured frame x[601..1332],
// y[847..1065] (gold-frame pixel detection). NOTE the native scoreboard is NOT screen-
// centered: its center is ~966 (right frame 1332, left 601), ~6px right of 960. We keep
// the bar screen-centered (960) — better for broadcast — and widen it symmetrically so
// the right edge still clears 1332. Bottom runs to the screen edge (1080): in dev the
// Windows taskbar occludes the scoreboard's bottom rows so we can't measure them, but in
// a borderless-fullscreen broadcast it extends lower — anchoring the panel bottom at the
// screen edge guarantees coverage regardless. The panel (x[580..1340], y[843..1080])
// covers the native frame on every side; we keep that native panel open under it for CS OCR.
const ROW0 = 870;
const ROW_STEP = 46;
const PANEL = { x0: 580, y0: 843, w: 760, h: 237 };

// blue-side column anchors (red mirrors at 1920 - x). Spread across the panel half so the
// row reads as a full scoreboard: champ + spells on the outer edge, items, then KDA / CS
// near the center divider (CS — the value we OCR — sits closest to center, largest).
const COL = {
    champ: 608,
    champSize: 34,
    spell: 642,
    spellSize: 15,
    item0: 672,
    itemStep: 24,
    itemSize: 23,
    itemCount: 6,
    kda: 852,
    cs: 910,
};

const COLORS = {
    panel: 0x141433,
    blueTint: 0x16456b,
    redTint: 0x5b1f57,
    white: '#ffffff',
    sub: '#c9d4ff',
    cs: '#ffe9a8',
};

interface RowRefs {
    blueKda: Phaser.GameObjects.Text; redKda: Phaser.GameObjects.Text;
    blueCs: Phaser.GameObjects.Text; redCs: Phaser.GameObjects.Text;
    role: Phaser.GameObjects.Text;
}

export default class PrmBottomBarVisual extends VisualElement {
    cfg: PrmScoreConfig;
    font: string;
    ddVersion: string;

    bg!: Phaser.GameObjects.Graphics;
    rows: RowRefs[] = [];

    // dynamic icon sprites keyed by a stable slot id ("champ:blue:0", "item:red:2:3", ...)
    iconSprites: Map<string, Phaser.GameObjects.Sprite> = new Map();
    iconLoaded: Map<string, string> = new Map();   // slotId -> last url loaded (skip reloads)

    constructor(scene: IngameScene, cfg: PrmScoreConfig) {
        super(scene, new Vector2(CENTER, 0), 'prmBottom');
        this.cfg = cfg || ({} as PrmScoreConfig);
        this.font = (cfg && cfg.Font) ? cfg.Font : 'News Cycle';
        this.ddVersion = (cfg && cfg.DDragonVersion) ? cfg.DDragonVersion : DDRAGON_FALLBACK;
        this.ResolveDDragonVersion();

        this.bg = scene.add.graphics();
        this.bg.setDepth(-1);
        this.DrawBackground();
        this.AddVisualComponent(this.bg);

        // per-row text scaffolds
        for (let i = 0; i < 5; i++) {
            const y = ROW0 + i * ROW_STEP;
            this.rows.push({
                blueKda: this.mkText(COL.kda, y, '0/0/0', 17, COLORS.sub, 0.5),
                redKda: this.mkText(1920 - COL.kda, y, '0/0/0', 17, COLORS.sub, 0.5),
                blueCs: this.mkText(COL.cs, y, '0', 19, COLORS.cs, 0.5),
                redCs: this.mkText(1920 - COL.cs, y, '0', 19, COLORS.cs, 0.5),
                role: this.mkText(CENTER, y, POS_ORDER[i].slice(0, 3), 13, '#8fa0d0', 0.5),
            });
        }

        this.GetActiveVisualComponents().forEach(c => { c.alpha = 0; });
        this.Init();
    }

    private mkText(x: number, y: number, str: string, size: number, color: string, ox: number): Phaser.GameObjects.Text {
        const t = this.scene.add.text(x, y, str, { fontFamily: this.font, fontSize: size + 'px', color, fontStyle: 'bold' });
        t.setOrigin(ox, 0.5);
        this.AddVisualComponent(t);
        return t;
    }

    private DrawBackground(): void {
        this.bg.clear();
        // Fully opaque: this panel's job is to occlude the native scoreboard beneath it.
        this.bg.fillStyle(COLORS.panel, 1);
        this.bg.fillRect(PANEL.x0, PANEL.y0, PANEL.w, PANEL.h);
        // team tints on each half
        this.bg.fillStyle(COLORS.blueTint, 0.5);
        this.bg.fillRect(PANEL.x0, PANEL.y0, PANEL.w / 2 - 30, PANEL.h);
        this.bg.fillStyle(COLORS.redTint, 0.5);
        this.bg.fillRect(CENTER + 30, PANEL.y0, PANEL.w / 2 - 30, PANEL.h);
        // center divider
        this.bg.fillStyle(0xffffff, 0.15);
        this.bg.fillRect(CENTER - 1, PANEL.y0 + 6, 2, PANEL.h - 12);
    }

    private ResolveDDragonVersion(): void {
        try {
            fetch('https://ddragon.leagueoflegends.com/api/versions.json')
                .then(r => r.json())
                .then((v: string[]) => { if (Array.isArray(v) && v.length) this.ddVersion = v[0]; })
                .catch(() => { /* keep fallback/config */ });
        } catch { /* keep fallback */ }
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
     * Per-lane gold diff chip in the center column (like the reference broadcast's
     * "◀0.1K"). Gold comes from the backend estimator and is displayed as the
     * players' gold (operator decision). Arrow points toward the leading side.
     */
    private applyLaneGoldDiff(i: number, blue?: PlayerScoreboardEntry, red?: PlayerScoreboardEntry): void {
        const r = this.rows[i];
        if (!blue || !red || blue.Gold === undefined || red.Gold === undefined) {
            r.role.text = POS_ORDER[i].slice(0, 3);
            r.role.setColor('#8fa0d0');
            return;
        }
        const diff = (blue.Gold || 0) - (red.Gold || 0);
        const amount = (Math.abs(diff) / 1000).toFixed(1) + 'K';
        // ASCII arrows: News Cycle lacks the ◀/▶ glyphs (they render as fallback junk).
        if (amount === '0.0K') {
            // Lanes effectively even (|diff| rounds to 0.0K): show the value with NO leading-side
            // arrow and a neutral color — an arrow here would falsely imply one side is ahead.
            r.role.text = amount;
            r.role.setColor('#9fb0d8');
        } else if (diff > 0) {
            r.role.text = '< ' + amount;
            r.role.setColor('#6fe4fd');
        } else {
            r.role.text = amount + ' >';
            r.role.setColor('#ff86f5');
        }
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
        if (!p) { kdaT.text = ''; csT.text = ''; return; }

        kdaT.text = `${p.Kills}/${p.Deaths}/${p.Assists}`;
        csT.text = `${p.CreepScore}`;

        // champion icon
        if (p.ChampionID)
            this.icon(`champ:${side}:${i}`, this.champUrl(p.ChampionID), this.mirror(COL.champ, red), y, COL.champSize, COL.champSize);

        // summoner spells (stacked)
        const sx = this.mirror(COL.spell, red);
        if (p.Spells && p.Spells[0]) this.icon(`spell:${side}:${i}:0`, this.spellUrl(p.Spells[0]), sx, y - 6, COL.spellSize, COL.spellSize);
        if (p.Spells && p.Spells[1]) this.icon(`spell:${side}:${i}:1`, this.spellUrl(p.Spells[1]), sx, y + 6, COL.spellSize, COL.spellSize);

        // items (6 slots, laid out toward center... blue items go right from item0; red mirror)
        for (let s = 0; s < COL.itemCount; s++) {
            const id = p.Items && p.Items[s] ? p.Items[s] : 0;
            const ix = this.mirror(COL.item0 + s * COL.itemStep, red);
            const key = `item:${side}:${i}:${s}`;
            if (id > 0) this.icon(key, this.itemUrl(id), ix, y, COL.itemSize, COL.itemSize);
            else this.clearIcon(key);
        }
    }

    private mirror(x: number, red: boolean): number { return red ? 1920 - x : x; }
    private champUrl(id: string): string { return `${DDRAGON_CDN}/${this.ddVersion}/img/champion/${id}.png`; }
    private itemUrl(id: number): string { return `${DDRAGON_CDN}/${this.ddVersion}/img/item/${id}.png`; }
    private spellUrl(key: string): string { return `${DDRAGON_CDN}/${this.ddVersion}/img/spell/${key}.png`; }

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
