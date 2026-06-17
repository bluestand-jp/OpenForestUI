import StateData from "~/data/stateData";
import IngameScene from "~/scenes/IngameScene";
import PlaceholderConversion from "~/PlaceholderConversion";
import Utils from "~/util/Utils";
import Vector2 from "~/util/Vector2";
import { PrmScoreConfig } from "~/data/config/overlayConfig";
import { VisualElement } from "./VisualElement";

/**
 * PRM / EMEA Masters style top bar (see docs/prm-overlay/SPEC.md).
 *
 * A self-contained, config-driven scoreboard: a full-width cyan->indigo->magenta
 * gradient bar carrying, per team, a symmetric row of objective counters
 * (dragons / void grubs / towers), gold (+ a "+X.XK" gold-lead badge under the
 * leader), kills, a team panel (logo / tag / region+seed), and a centered
 * game clock. Layout constants are calibrated to the
 * 1920x1080 reference frame and exposed for tuning; data comes from StateData.
 *
 * Instantiated by IngameScene only when OverlayConfig.PrmScore.Enabled — the
 * legacy ScoreboardVisual is left untouched and remains the default.
 */

// ----- calibrated layout (1920x1080), center mirror axis = 960 -----
const CENTER = 960;
// Shift the WHOLE bar (bg split, team panels, center diamond, objective icons, kills, gold, tags/logos,
// gold-lead badge) +3px right to align with the native HUD (its center measured ~+5px right of 960).
// The center pendant + clock are EXCLUDED (they have their own native alignment via PENDANT_DX).
const BAR_DX = 3;
// Long bar height. Trimmed from the original 70 so the bar covers less of the native HUD's drake-soul
// icons below it (that native stack is the only place dragon TYPES are visible — the spectator API
// gives no per-type data, only an OCR'd count). 56 read as too thin in live feedback; 68 keeps a
// slimmer bar while restoring presence. TUNABLE against the live HUD.
const BAR_H = 62;   // calibrated live. Trimmed 65->61 so the bar's bottom edge stops covering the native HUD drake-soul icons just below it. Note: raising this shortens the center pendant (its visible top = the bar bottom), so PENDANT_BOTTOM is raised in step to keep the pendant tall.
const ROW_Y = Math.round(BAR_H / 2);   // counter / gold / kills row, vertically centered in the bar
const KILLS_Y = ROW_Y - 2;
// Center clock "pendant" that dips below the bar to mask the native center timer + its dark fade.
// Its BOTTOM is anchored to a fixed screen Y (not to the bar) so shortening the bar above does NOT
// uncover the native timer — the pendant just grows taller to keep it covered. Width widened from 92
// so the native fade no longer pokes out the sides. All three are TUNABLE against the live HUD.
const PENDANT_W = 170;
const PENDANT_TRIM_L = 1;   // trim the pendant box's LEFT edge inward (px), independent of center/clock
const PENDANT_TRIM_R = 4;   // trim the pendant box's RIGHT edge inward (px)                 // mask the native center timer/fade WITHOUT covering the flanking drake icons (trimmed from 180)
const PENDANT_BOTTOM = 101;            // pendant bottom (fixed screen Y); raised with BAR_H so the time box also got ~3px taller
const TIME_Y = 84;                     // game-clock text, centered in the (taller) pendant
// The native HUD center sits ~+5px RIGHT of the geometric screen center (960) in 1920x1080 capture:
// the in-game timer "11:23" and the kills both measured at x=965, and live gaps were L:2px / R:11px
// between the pendant and the native objective icons that flank it below the bar. Shift the pendant +
// clock right by this so they sit centered between those native icons. TUNABLE (+right / -left).
const PENDANT_DX = 6;
const PANEL_TAG_Y = ROW_Y;             // team tag vertically centered (region/seed sub hidden, so no upward offset)
const PANEL_SUB_Y = ROW_Y + 11;        // (region/seed sub-line is hidden per request; kept for layout math)

// distance from center (x) for each blue-side element; red mirrors at 1920-x
const X = {
    killsInner: 925,       // kills sit nearest center (pulled in to clear the gold number)
    goldIcon: 793,
    goldText: 808,         // left edge of gold number (blue); red mirrors the icon->number order
    // Red gold reads icon -> number like blue (operator request). The pure mirror icon
    // position (1920 - goldIcon = 1127) would push the wide gold number into the objective
    // row (tower icon ~1166), so the red gold cluster is anchored inward here, fitting
    // between the red kills and the tower icon. NOT a mirror of goldIcon by design.
    goldIconRed: 1078,
    // 4 objective counters (outer->inner for blue) = grub / baron / dragon / tower, matching the
    // native top bar's objective row. Wider 58px pitch (was 52, felt cramped) + shifted left with
    // the team panel so the dense counter / gold / kills run gets breathing room.
    grub: 568,
    baron: 626,
    dragon: 684,
    tower: 742,
    digitGap: 18,          // gap from counter icon to its digit
    // team panel initial/fallback anchors. The logo + name group is centered in the highlight
    // panel at runtime by layoutPanel(); these are just pre-first-frame positions.
    panelLogo: 348,
    panelTag: 404,
    panelSub: 404,
};

// team "highlight" panel (the brighter team-colored block). The identity group (logo + name)
// is centered within it at runtime. Red mirrors at 1920 - x.
const PANEL = { x0: 312, w: 230 };
const PANEL_CX = PANEL.x0 + PANEL.w / 2;

const COLORS = {
    gradCenter: 0x33316d,   // indigo center (kept; the two outer stops follow the team colors)
    white: '#ffffff',
    badge: 0xce4dd6,
};

// Team colors come from the shared overlay channel (state.blueColor / redColor — the same source
// every other visual already reads), defaulting to these when absent. rgb(66,133,244)/rgb(234,67,53)
// match the app's PickBan default team colors, so the bar is blue/red by default and tracks the
// operator's team-color picker with no bar-specific setting (operator choice: reuse the shared colors).
const DEFAULT_BLUE = 0x4285f4;
const DEFAULT_RED = 0xea4335;

const ICON = { w: 32, h: 32 };
const GOLD_ICON = { w: 32, h: 32 };
const LOGO = { w: BAR_H - 10, h: BAR_H - 10 };   // team logo, auto-scaled to fit the (tunable) bar height
const LOGO_GAP = 12;       // logo -> name gap inside the highlight panel

export default class PrmScoreboardVisual extends VisualElement {
    cfg: PrmScoreConfig;
    font: string;

    bg!: Phaser.GameObjects.Graphics;
    panels!: Phaser.GameObjects.Graphics;
    badge!: Phaser.GameObjects.Graphics;

    gameTime!: Phaser.GameObjects.Text;

    blueTag!: Phaser.GameObjects.Text;
    redTag!: Phaser.GameObjects.Text;
    blueSub!: Phaser.GameObjects.Text;
    redSub!: Phaser.GameObjects.Text;

    blueKills!: Phaser.GameObjects.Text;
    redKills!: Phaser.GameObjects.Text;
    blueGold!: Phaser.GameObjects.Text;
    redGold!: Phaser.GameObjects.Text;
    badgeText!: Phaser.GameObjects.Text;

    // counter digit texts
    blueTower!: Phaser.GameObjects.Text;
    redTower!: Phaser.GameObjects.Text;
    blueGrub!: Phaser.GameObjects.Text;
    redGrub!: Phaser.GameObjects.Text;
    blueBaron!: Phaser.GameObjects.Text;
    redBaron!: Phaser.GameObjects.Text;
    blueDragon!: Phaser.GameObjects.Text;
    redDragon!: Phaser.GameObjects.Text;

    icons: Phaser.GameObjects.Sprite[] = [];
    blueLogo: Phaser.GameObjects.Sprite | null = null;
    redLogo: Phaser.GameObjects.Sprite | null = null;
    blueLogoName = '';
    redLogoName = '';
    blueLogoX = X.panelLogo;
    redLogoX = 1920 - X.panelLogo;

    // Resolved team colors (see DEFAULT_BLUE/RED): int forms feed the gradient / panel Graphics fills,
    // hex forms feed the kill-number Text. Re-resolved from state each frame; redraw only on a change.
    blueInt = DEFAULT_BLUE;
    redInt = DEFAULT_RED;
    blueHex = '#4285f4';
    redHex = '#ea4335';
    lastBlueStr = '';
    lastRedStr = '';

    constructor(scene: IngameScene, cfg: PrmScoreConfig) {
        super(scene, new Vector2(CENTER, 0), 'prmScore');
        this.cfg = cfg || ({} as PrmScoreConfig);
        // Use a configured font if present, else "News Cycle" (already loaded by
        // the overlay's default GoogleFonts) which matches the broadcast condensed look.
        this.font = (cfg && cfg.Font) ? cfg.Font : 'News Cycle';

        this.CreateIconListeners();

        // --- background gradient (cyan -> indigo -> magenta) ---
        this.bg = scene.add.graphics();
        this.bg.setDepth(-2);
        this.DrawBackground();
        this.AddVisualComponent(this.bg);

        // --- team-colored panels ---
        this.panels = scene.add.graphics();
        this.panels.setDepth(-1);
        this.DrawPanels();
        this.AddVisualComponent(this.panels);

        // --- gold-lead badge (drawn on demand) ---
        this.badge = scene.add.graphics();
        this.badge.setDepth(1);
        this.AddVisualComponent(this.badge);

        // --- center clock (divider diamond is drawn in DrawPanels) ---
        // Clock nudged 3px left of the pendant center so the digits READ centered in the box
        // (the font's side-bearing makes a true-centered "19:52" sit ~3px right). TUNABLE.
        this.gameTime = this.mkText(CENTER + PENDANT_DX - 3, TIME_Y, '00:00', 19, COLORS.white, 'center');

        // --- team tags + sub (flag/region/seed) ---
        this.blueTag = this.mkText(X.panelTag, PANEL_TAG_Y, '', 32, COLORS.white, 'left');
        this.blueTag.setOrigin(0, 0.5);
        this.redTag = this.mkText(1920 - X.panelTag, PANEL_TAG_Y, '', 32, COLORS.white, 'right');
        this.redTag.setOrigin(1, 0.5);
        this.blueSub = this.mkText(X.panelSub, PANEL_SUB_Y, '', 13, '#e8f6ff', 'left');
        this.blueSub.setOrigin(0, 0.5);
        this.redSub = this.mkText(1920 - X.panelSub, PANEL_SUB_Y, '', 13, '#ffe8fb', 'right');
        this.redSub.setOrigin(1, 0.5);

        // --- kills ---
        this.blueKills = this.mkText(X.killsInner + BAR_DX, KILLS_Y, '0', 42, COLORS.white, 'right');
        this.blueKills.setOrigin(1, 0.5);
        this.redKills = this.mkText(1920 - X.killsInner + BAR_DX, KILLS_Y, '0', 42, COLORS.white, 'left');
        this.redKills.setOrigin(0, 0.5);

        // --- gold ---
        this.blueGold = this.mkText(X.goldText + BAR_DX, ROW_Y, '0.0k', 22, COLORS.white, 'left');
        this.blueGold.setOrigin(0, 0.5);
        // Red gold number sits to the RIGHT of the red gold icon (icon -> number, like blue),
        // using the same icon->number gap as blue.
        this.redGold = this.mkText(X.goldIconRed + (X.goldText - X.goldIcon) + BAR_DX, ROW_Y, '0.0k', 22, COLORS.white, 'left');
        this.redGold.setOrigin(0, 0.5);
        this.badgeText = this.mkText(0, 0, '', 14, COLORS.white, 'center');
        this.badgeText.setOrigin(0.5, 0.5);
        this.badgeText.setDepth(2);

        // --- counter digits: each icon+digit is ONE BLOCK reading icon->number, and the
        // blue/red BLOCKS are mirror-symmetric about 960 (operator request). Blue block:
        // icon @ X.obj, digit @ X.obj+digitGap. To mirror the *block* (not just the icon),
        // the red icon shifts inward to (1920 - X.obj) - digitGap and its digit sits at
        // 1920 - X.obj — so red block [1920-X.obj-digitGap .. ] mirrors blue [.. X.obj+digitGap].
        this.blueTower = this.counterText(X.tower + X.digitGap, 'left');
        this.redTower = this.counterText(1920 - X.tower, 'left');
        this.blueGrub = this.counterText(X.grub + X.digitGap, 'left');
        this.redGrub = this.counterText(1920 - X.grub, 'left');
        this.blueBaron = this.counterText(X.baron + X.digitGap, 'left');
        this.redBaron = this.counterText(1920 - X.baron, 'left');
        this.blueDragon = this.counterText(X.dragon + X.digitGap, 'left');
        this.redDragon = this.counterText(1920 - X.dragon, 'left');

        // --- counter icons (preloaded prm_* textures). Red icons shift inward by digitGap
        // so the icon+digit BLOCK mirrors blue (see counter-digit note above). ---
        this.mkIcon('prm_grub', X.grub, ICON);
        this.mkIcon('prm_grub', (1920 - X.grub) - X.digitGap, ICON);
        this.mkIcon('prm_baron', X.baron, ICON);            // white baron head (matches the prm white icon set)
        this.mkIcon('prm_baron', (1920 - X.baron) - X.digitGap, ICON);
        this.mkIcon('prm_dragon', X.dragon, ICON);
        this.mkIcon('prm_dragon', (1920 - X.dragon) - X.digitGap, ICON);
        this.mkIcon('prm_tower', X.tower, ICON);
        this.mkIcon('prm_tower', (1920 - X.tower) - X.digitGap, ICON);
        this.mkIcon('prm_gold', X.goldIcon, GOLD_ICON);
        this.mkIcon('prm_gold', X.goldIconRed, GOLD_ICON);

        // hide everything until first data arrives
        this.GetActiveVisualComponents().forEach(c => { c.alpha = 0; });

        this.Init();
    }

    private mkText(x: number, y: number, str: string, size: number, color: string, align: string): Phaser.GameObjects.Text {
        const t = this.scene.add.text(x, y, str, {
            fontFamily: this.font, fontSize: size + 'px', color, fontStyle: 'bold', align,
        });
        t.setOrigin(0.5, 0.5);
        this.AddVisualComponent(t);
        return t;
    }

    private counterText(x: number, align: 'left' | 'right'): Phaser.GameObjects.Text {
        const t = this.mkText(x + BAR_DX, ROW_Y, '0', 22, COLORS.white, align);
        t.setOrigin(align === 'left' ? 0 : 1, 0.5);
        return t;
    }

    private mkIcon(key: string, x: number, size: { w: number, h: number }): void {
        const s = this.scene.make.sprite({ x: x + BAR_DX, y: ROW_Y, key, add: true });
        s.setOrigin(0.5, 0.5);
        s.setDisplaySize(size.w, size.h);
        this.icons.push(s);
        this.AddVisualComponent(s);
    }

    /**
     * Resolve the two team colors from the shared overlay channel (state.blueColor / redColor — the
     * same data every other visual reads; default blue/red). Caches the raw strings and returns true
     * only when something changed, so the caller re-skins the gradient / panels / kills just on change.
     */
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
        this.blueHex = '#' + bc.color.toString(16).padStart(6, '0');
        this.redHex = '#' + rc.color.toString(16).padStart(6, '0');
        return true;
    }

    /** Linear-interpolate two 0xRRGGBB colors (t clamped to [0,1]). Used to sample the bar gradient. */
    private lerpHex(a: number, b: number, t: number): number {
        t = t < 0 ? 0 : t > 1 ? 1 : t;
        const ar = (a >> 16) & 0xff, ag = (a >> 8) & 0xff, ab = a & 0xff;
        const br = (b >> 16) & 0xff, bg = (b >> 8) & 0xff, bb = b & 0xff;
        return (Math.round(ar + (br - ar) * t) << 16) | (Math.round(ag + (bg - ag) * t) << 8) | Math.round(ab + (bb - ab) * t);
    }

    private DrawBackground(): void {
        this.bg.clear();
        // left half: team-blue -> indigo ; right half: indigo -> team-red
        this.bg.fillGradientStyle(this.blueInt, COLORS.gradCenter, this.blueInt, COLORS.gradCenter, 1);
        this.bg.fillRect(0, 0, CENTER + BAR_DX, BAR_H);
        this.bg.fillGradientStyle(COLORS.gradCenter, this.redInt, COLORS.gradCenter, this.redInt, 1);
        this.bg.fillRect(CENTER + BAR_DX, 0, CENTER - BAR_DX, BAR_H);
        // center clock pendant: a SEAMLESS downward continuation of the bar. The bar's color is
        // constant vertically and is a 2-segment gradient (blue->indigo on the left half, indigo->red
        // on the right) meeting at the indigo center (split). We redraw that SAME 2-segment gradient
        // over the pendant's x-range as two fillRects meeting at split, so the pendant's top edge
        // matches the bar's bottom edge at EVERY x (no seam, no color break). Uses fillRect (NOT
        // fillRoundedRect): Phaser does not apply a gradient cleanly to a rounded rect, which is what
        // produced the wrong-color seam. Bottom is square, matching the bar.
        const split = CENTER + BAR_DX;
        const pLeft = CENTER + PENDANT_DX - PENDANT_W / 2 + PENDANT_TRIM_L;
        const pRight = CENTER + PENDANT_DX + PENDANT_W / 2 - PENDANT_TRIM_R;
        const pTop = BAR_H - 2, pH = PENDANT_BOTTOM - (BAR_H - 2);
        const cAt = (x: number) => x <= split
            ? this.lerpHex(this.blueInt, COLORS.gradCenter, x / split)
            : this.lerpHex(COLORS.gradCenter, this.redInt, (x - split) / (1920 - split));
        this.bg.fillGradientStyle(cAt(pLeft), COLORS.gradCenter, cAt(pLeft), COLORS.gradCenter, 1);
        this.bg.fillRect(pLeft, pTop, split - pLeft, pH);
        this.bg.fillGradientStyle(COLORS.gradCenter, cAt(pRight), COLORS.gradCenter, cAt(pRight), 1);
        this.bg.fillRect(split, pTop, pRight - split, pH);
    }

    private DrawPanels(): void {
        this.panels.clear();
        // brighter solid team panels behind the tags (simple rounded rects for now)
        this.panels.fillStyle(this.blueInt, 0.9);
        this.panels.fillRect(PANEL.x0 + BAR_DX, 0, PANEL.w, BAR_H);
        this.panels.fillStyle(this.redInt, 0.9);
        this.panels.fillRect(1920 - PANEL.x0 - PANEL.w + BAR_DX, 0, PANEL.w, BAR_H);
        // center divider ornament (small diamond between the two kill numbers)
        this.panels.fillStyle(0xffffff, 0.95);
        const cy = KILLS_Y, r = 6;
        this.panels.beginPath();
        this.panels.moveTo(CENTER + BAR_DX, cy - r); this.panels.lineTo(CENTER + BAR_DX + r, cy);
        this.panels.lineTo(CENTER + BAR_DX, cy + r); this.panels.lineTo(CENTER + BAR_DX - r, cy);
        this.panels.closePath(); this.panels.fillPath();
    }

    UpdateValues(state: StateData): void {
        const sb = state.scoreboard;
        if (sb === undefined || sb.GameTime === undefined || sb.GameTime === null || sb.GameTime === -1) {
            // Hold the last shown values on a transient empty/loading frame (a GameTime=-1 heartbeat
            // during game load, or mock/live heartbeats interleaving) instead of hiding the bar. It is
            // taken down only by explicit lifecycle events (GameEnd / WS disconnect) in IngameScene.
            return;
        }
        const t = Math.round(sb.GameTime);
        this.gameTime.text = (Math.floor(t / 60) >= 10 ? Math.floor(t / 60) : '0' + Math.floor(t / 60)) + ':' + (t % 60 >= 10 ? t % 60 : '0' + (t % 60));

        // Re-skin the bar (gradient + panels) to the current team colors if they changed (shared
        // channel; default blue/red). Kill numbers stay white for legibility on the colored bar.
        if (this.resolveTeamColors(state)) {
            this.DrawBackground();
            this.DrawPanels();
        }

        const blue = sb.BlueTeam, red = sb.RedTeam;

        this.blueTag.text = blue.Name || '';
        this.redTag.text = red.Name || '';
        // Region/seed sub-line hidden per request — only the larger, centered team tag remains.
        this.blueSub.text = '';
        this.redSub.text = '';
        this.layoutPanel(false);
        this.layoutPanel(true);

        this.blueKills.text = (blue.Kills ?? 0) + '';
        this.redKills.text = (red.Kills ?? 0) + '';
        this.blueGold.text = Utils.ConvertGold(blue.Gold);
        this.redGold.text = Utils.ConvertGold(red.Gold);

        this.blueTower.text = (blue.Towers ?? 0) + '';
        this.redTower.text = (red.Towers ?? 0) + '';
        this.blueGrub.text = ((blue as any).VoidGrubs ?? 0) + '';
        this.redGrub.text = ((red as any).VoidGrubs ?? 0) + '';
        this.blueBaron.text = ((blue as any).Baron ?? 0) + '';
        this.redBaron.text = ((red as any).Baron ?? 0) + '';
        // OCR'd dragon count (DragonCount); fall back to the typed Dragons list length if absent.
        this.blueDragon.text = ((blue as any).DragonCount ?? (blue.Dragons ? blue.Dragons.length : 0)) + '';
        this.redDragon.text = ((red as any).DragonCount ?? (red.Dragons ? red.Dragons.length : 0)) + '';

        this.UpdateBadge(blue.Gold, red.Gold);
        this.UpdateLogo(false, blue.Icon);
        this.UpdateLogo(true, red.Icon);

        if (!this.isActive) this.Start();
    }

    private subText(team: any): string {
        const region = team.Region || '';
        const seed = (team.Seed !== undefined && team.Seed !== null && team.Seed !== '') ? ' #' + team.Seed : '';
        return (region + seed).trim();
    }

    /**
     * Center the identity group (team logo + tag/sub) within the highlight panel.
     * Blue: [logo][name] left->right; red mirrors to [name][logo]. Group width is taken from the
     * wider of tag/sub so it stays centered regardless of team-name length.
     */
    private layoutPanel(red: boolean): void {
        const tag = red ? this.redTag : this.blueTag;
        const sub = red ? this.redSub : this.blueSub;
        const nameW = Math.max(tag.width, sub.width);
        const groupW = LOGO.w + LOGO_GAP + nameW;
        const cx = (red ? (1920 - PANEL_CX) : PANEL_CX) + BAR_DX;
        const left = cx - groupW / 2;
        if (red) {
            const nameRight = left + nameW;        // right-aligned name (inner), logo outermost (right)
            tag.setX(nameRight);
            sub.setX(nameRight);
            this.redLogoX = left + nameW + LOGO_GAP + LOGO.w / 2;
            if (this.redLogo) this.redLogo.setX(this.redLogoX);
        } else {
            this.blueLogoX = left + LOGO.w / 2;    // logo outermost (left), name inner
            const nameLeft = left + LOGO.w + LOGO_GAP;
            tag.setX(nameLeft);
            sub.setX(nameLeft);
            if (this.blueLogo) this.blueLogo.setX(this.blueLogoX);
        }
    }

    /** Gold-lead badge under whichever team leads. */
    private UpdateBadge(blueGold: number, redGold: number): void {
        this.badge.clear();
        const diff = Math.round((blueGold || 0) - (redGold || 0));
        if (Math.abs(diff) < 100) { this.badgeText.text = ''; return; }
        const lead = diff > 0;
        const txt = '+' + Utils.ConvertGold(Math.abs(diff));
        this.badgeText.text = txt;
        // Center the badge under the leading side's gold NUMBER using the Text's real rendered center
        // (x = its left edge at origin 0; width = its measured pixel width, BAR_DX already baked in).
        // The old fixed "+30" assumed a wide number, so a short value like "3.2k" left the chip ~12px
        // right of center — visibly off on BOTH sides. Measuring per-frame keeps it centered for any width.
        const leadGold = lead ? this.blueGold : this.redGold;
        const cx = leadGold.x + leadGold.width / 2;
        const y = BAR_H - 11;   // keep the lead chip INSIDE the trimmed bar (ROW_Y+26 poked ~4px below the bottom edge)
        const w = 64, h = 16;
        this.badge.fillStyle(COLORS.badge, 1);
        this.badge.fillRoundedRect(cx - w / 2, y - h / 2, w, h, 4);
        this.badgeText.setPosition(cx, y);
        if (this.isActive) { this.badge.alpha = 1; this.badgeText.alpha = 1; }
    }

    private UpdateLogo(red: boolean, icon: string | undefined): void {
        // Team Icons toggled off => the backend omits Icon (undefined here). Remove the logo so it
        // disappears, instead of the old early-return that held the last logo on screen. Clearing the
        // cached name makes it reload cleanly when Team Icons is turned back on.
        if (!icon) {
            const old = red ? this.redLogo : this.blueLogo;
            if (old) { this.RemoveVisualComponent(old); old.destroy(); }
            if (red) { this.redLogo = null; this.redLogoName = ''; }
            else { this.blueLogo = null; this.blueLogoName = ''; }
            return;
        }
        const cur = red ? this.redLogoName : this.blueLogoName;
        if (icon === cur) return;
        if (red) this.redLogoName = icon; else this.blueLogoName = icon;
        const key = (red ? 'prm_red' : 'prm_blue') + '_logo';
        const url = PlaceholderConversion.MakeUrlAbsolute(icon.replace('Cache', '/cache').replace(/\\/g, '/'));
        if (this.scene.textures.exists(key)) this.scene.textures.remove(key);
        this.scene.load.image(key, url);
        this.scene.load.once('filecomplete-image-' + key, () => this.placeLogo(red, key));
        this.scene.load.start();
    }

    private placeLogo(red: boolean, key: string): void {
        const old = red ? this.redLogo : this.blueLogo;
        if (old) { this.RemoveVisualComponent(old); old.destroy(); }
        const x = red ? this.redLogoX : this.blueLogoX;
        const s = this.scene.make.sprite({ x, y: BAR_H / 2, key, add: true });
        s.setOrigin(0.5, 0.5);
        s.setDisplaySize(LOGO.w, LOGO.h);
        s.alpha = this.isActive ? 1 : 0;
        this.AddVisualComponent(s);
        if (red) this.redLogo = s; else this.blueLogo = s;
    }

    private CreateIconListeners(): void { /* metric icons are preloaded; nothing dynamic */ }

    Load(): void { /* built in constructor */ }

    UpdateConfig(newConfig: PrmScoreConfig): void {
        this.cfg = newConfig || this.cfg;
        if (newConfig && newConfig.Font) this.font = newConfig.Font;
    }

    Start(): void {
        if (this.isActive || this.isShowing) return;
        this.isActive = true;
        this.GetActiveVisualComponents().forEach(c => { c.alpha = 1; });
        // badge stays hidden if there's no lead text
        if (!this.badgeText.text) { this.badge.alpha = 0; this.badgeText.alpha = 0; }
    }

    Stop(): void {
        if (!this.isActive) return;
        this.isActive = false;
        this.GetActiveVisualComponents().forEach(c => { c.alpha = 0; });
    }
}
