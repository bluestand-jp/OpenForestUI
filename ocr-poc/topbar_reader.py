"""Top-bar team-gold reader — finalized reference logic (Python PoC; to be ported to C#).

Pipeline per team, per frame:
  crop fixed ROI -> grayscale+Otsu -> upscale -> EasyOCR(allowlist 0-9 . k)
  -> parse "NN.Nk" (or digits*100) -> RANGE CHECK [MIN,MAX]
  -> monotonic/bounded gate with hold-last-good -> tri-state (Known / Stale / Unknown)

Design notes:
- Range check alone rejects every failure mode observed across 6 real frames
  ('.' misreads inflate the value 10-100x, always > MAX).  The gate adds protection
  against the (unobserved but possible) in-range blip and provides Stale/Unknown.
- A read is NEVER shown unless it passed range + gate => the overlay hides rather than lies.
- ROI is for 1920x1080 fixed HUD; treat as per-setup calibration (R4).
"""
import cv2, numpy as np, re, os

# Canonical digit templates (clean 0-9 harvested from the fixed HUD font) for the segmentation +
# per-glyph NCC classifier used by read_cs. None if the asset is missing -> read_cs falls back to
# EasyOCR. Built by ocr-poc/_harvest.py; ship digit_templates.npz beside this file. Do not hand-edit.
_TMPL_PATH = os.path.join(os.path.dirname(os.path.abspath(__file__)), 'digit_templates.npz')
try:
    _z = np.load(_TMPL_PATH)
    _DIGIT_TMPL = {d: _z['top_%s' % d].astype(np.float32) for d in '0123456789'}
except Exception:
    _DIGIT_TMPL = None
_CS_MIN_CONF = 0.40    # min per-glyph NCC to accept a CS read (else None so the CountGate holds)

MIN_GOLD, MAX_GOLD = 2000, 99999          # plausible team-gold range
ROI_1080 = {'blue': (803, 862, 14, 48),   # x0,x1,y0,y1
            'red':  (1105, 1156, 14, 48)}  # x0 tightened past the gold-coin icon (was 1098):
                                           # the coin survived the red emphasis and was mis-read
                                           # as '4' (e.g. '17.9k'->'47.9k').

# Team TOWER (turret-takedown) count, read off the same observer top bar. The digit sits just
# right of the turret-chevron icon (the one immediately outward from the gold on each side).
# Calibrated against a 1920x1080 LoL replay capture (see ocr-poc/out/roi_blue.png / roi_red.png).
# 28px wide leaves room for a 2-digit count (max 11). Same team-colour emphasis as the gold digits.
TOWER_ROI_1080 = {'blue': (739, 767, 16, 44),
                  'red':  (1185, 1213, 16, 44)}
MAX_TOWERS = 11                            # 3 lanes x 3 turrets + 2 nexus turrets = 11 per team

# Per-team objective counts (grub / baron / dragon / tower), read off the observer top bar's
# objective row. Objective-MONSTER kills (Dragon/Horde/Baron/Herald) are NOT emitted in the
# spectator/replay /eventdata (verified on live-spectate AND .rofl), so this OCR is the only
# count source. Calibrated 2026-06-13 against the borderless-fullscreen 1920x1080 HUD; the digit
# centres are blue grub523/baron600/dragon670/tower749, red tower1191/dragon1258/baron1336/grub1413
# (gold excluded). Slot meaning of the baron column is Herald before 20:00, Baron after. Each digit
# gets its OWN crop+Otsu (whole-strip Otsu distorted '2'->'9'); full digit height y10..37.
# Validated 8/8 on an independent client-match screenshot.
OBJ_ROI_1080 = {
    'blue_grub':   (507, 540, 10, 37),
    'blue_baron':  (584, 617, 10, 37),
    'blue_dragon': (654, 687, 10, 37),
    'blue_tower':  (733, 766, 10, 37),
    'red_tower':   (1176, 1208, 10, 37),
    'red_dragon':  (1242, 1275, 10, 37),
    'red_baron':   (1320, 1353, 10, 37),
    'red_grub':    (1399, 1432, 10, 37),
}
OBJ_MAX = {'grub': 9, 'baron': 9, 'dragon': 14, 'tower': 11}   # plausible caps; reject gold/garbage misreads

# Per-player CS, read off the spectator DETAIL scoreboard (bottom-center panel) at default
# 1X zoom / default position. The Live Client Data API floors creepScore to multiples of 10
# in spectator/replay; this panel shows the exact value. 10 cells: B0..B4 = left team top->
# bottom (== /playerlist[0..4] = ORDER by position), R0..R4 = right team (== [5..9] = CHAOS).
# Calibrated 2026-06-13 against a 1920x1080 replay (see ocr-poc/cs_ocr_poc.py); proven
# API == floor(OCR/10)*10 on all 10 cells.
_CS_ROW_Y0, _CS_PITCH, _CS_H = 862, 42, 30
_CS_LEFT_X, _CS_RIGHT_X = (864, 918), (1022, 1074)
CS_ROI_1080 = {}
for _i in range(5):
    _y = _CS_ROW_Y0 + _i * _CS_PITCH
    CS_ROI_1080['B%d' % _i] = (_CS_LEFT_X[0], _CS_LEFT_X[1], _y, _y + _CS_H)
    CS_ROI_1080['R%d' % _i] = (_CS_RIGHT_X[0], _CS_RIGHT_X[1], _y, _y + _CS_H)
MAX_CS = 599                               # plausible cap; rejects 4-digit misreads

def parse_gold(raw: str):
    """'55.8k'->55800 ; '320'->32000 (digits*100, last digit = 0.1k) ; junk->None.
    Team gold renders as 'NN.Nk' (<=3 significant digits). A 4th digit is a spurious
    'k'->digit / leakage misread (e.g. '11.3k' -> '1131'); drop the trailing extra."""
    s = raw.replace('k', '').replace('K', '')
    m = re.match(r'^(\d{1,3}\.\d)$', s)
    if m:
        return round(float(m.group(1)) * 1000)
    d = re.sub(r'\D', '', s)
    if not d:
        return None
    if len(d) >= 4 and int(d) * 100 > 99999:   # trailing spurious digit -> keep first 3
        d = d[:3]
    return int(d) * 100

def _emphasis(crop, color):
    """Single-channel image where the team-colored gold text pops from the tinted background.
    Grayscale loses red text on the red side; channel-difference recovers it."""
    if color not in ('blue', 'red'):
        return cv2.cvtColor(crop, cv2.COLOR_BGR2GRAY)
    b, g, r = (c.astype(np.int16) for c in cv2.split(crop))
    ch = (2 * b - r - g) if color == 'blue' else (2 * r - g - b)
    return np.clip(ch, 0, 255).astype(np.uint8)

def _crop_pad_digits(bw, min_area):
    """Normalise a thresholded digit crop before the upscale+OCR: drop sub-digit blobs (an
    adjacent icon's edge, the gold '.' separator, noise) under `min_area` px, tight-crop to the
    surviving digit(s), then re-pad with a symmetric margin (~half the digit height) so the
    glyph(s) sit centred with whitespace. An off-centre / low-sitting digit was distorted by the
    8x cubic upscale (a '2' read as '0'); centring + margin fixes it. Multi-digit numbers share
    one bbox (kept together, inter-digit spacing preserved). Returns the padded uint8 image, or
    None if no blob survived. NOTE: mutates `bw` in place (callers don't reuse it)."""
    n, lab, st, _ = cv2.connectedComponentsWithStats(bw, 8)
    for c in range(1, n):
        if st[c, cv2.CC_STAT_AREA] < min_area:
            bw[lab == c] = 0
    ys, xs = np.where(bw > 0)
    if len(xs) == 0:
        return None
    dig = bw[ys.min():ys.max() + 1, xs.min():xs.max() + 1]
    pad = dig.shape[0] // 2 + 4
    return cv2.copyMakeBorder(dig, pad, pad, pad, pad, cv2.BORDER_CONSTANT, value=0)


def ocr_crop(reader, crop, color='gray'):
    g = cv2.GaussianBlur(_emphasis(crop, color), (3, 3), 0)
    _, bw = cv2.threshold(g, 0, 255, cv2.THRESH_BINARY + cv2.THRESH_OTSU)
    # Drop the '.' separator (+ noise) so it's never misread as a digit, then tight-crop + centre
    # the NN.Nk glyphs. digits*100 reconstructs the 0.1k value ('60.7k' -> '607' -> 60700).
    bw = _crop_pad_digits(bw, 15)
    if bw is None:
        return ''
    up = cv2.resize(bw, None, fx=8, fy=8, interpolation=cv2.INTER_CUBIC)
    return ''.join(reader.readtext(up, allowlist='0123456789k', detail=0, paragraph=False)).replace(' ', '')


def read_count(reader, crop, color, maxval=MAX_TOWERS):
    """OCR a single small team counter (tower/grub/baron/dragon, 0..maxval) from a top-bar ROI.
    Team-colour emphasis (digits are team-coloured); per-crop Otsu (NOT a shared whole-strip
    threshold — that distorts small digits, e.g. '2'->'9'); drop sub-digit flecks (an adjacent
    icon's edge that bleeds into the ROI); tight-crop to the digit(s) and re-pad with a symmetric
    margin so the glyph is centred with whitespace (a low-sitting, top-padded '2' was read as '0');
    8x cubic upscale; digit allowlist.
    Returns the int, or None if nothing plausible was read (out of [0,maxval] => None)."""
    g = cv2.GaussianBlur(_emphasis(crop, color), (3, 3), 0)
    _, bw = cv2.threshold(g, 0, 255, cv2.THRESH_BINARY + cv2.THRESH_OTSU)
    bw = _crop_pad_digits(bw, 12)          # drop icon-edge flecks (<12px), centre the digit(s)
    if bw is None:
        return None
    up = cv2.resize(bw, None, fx=8, fy=8, interpolation=cv2.INTER_CUBIC)
    txt = ''.join(reader.readtext(up, allowlist='0123456789', detail=0, paragraph=False)).replace(' ', '')
    d = re.sub(r'\D', '', txt)
    if not d:
        return None
    v = int(d)
    return v if 0 <= v <= maxval else None


def _ncc_score(a, b):
    a = a.astype(np.float32) - a.mean(); b = b.astype(np.float32) - b.mean()
    den = float(np.linalg.norm(a) * np.linalg.norm(b))
    return float((a * b).sum() / den) if den > 1e-6 else -1.0


def _classify_glyph(glyph):
    """Nearest canonical digit by aspect-matched NCC (each template resized to this glyph's size).
    Whole-glyph (NOT sliding) -- avoids the partial/spurious matches that make naive template
    matching confuse 4/7 and emit phantom 1s. Returns (digit_str, score)."""
    best, bs = None, -2.0
    for d, t in _DIGIT_TMPL.items():
        tr = cv2.resize(t, (max(1, glyph.shape[1]), max(1, glyph.shape[0])), interpolation=cv2.INTER_AREA)
        s = _ncc_score(glyph, tr)
        if s > bs:
            bs, best = s, d
    return best, bs


def _split_cuts(col, n):
    """n-1 cut columns for an n-digit merged blob: near equal divisions, refined to the local
    projection valley (thinnest column = between digits)."""
    w = len(col); cuts = []
    for i in range(1, n):
        c = int(round(i * w / n)); lo = max(1, c - w // (2 * n)); hi = min(w - 1, c + w // (2 * n))
        cuts.append(lo + int(np.argmin(col[lo:hi])) if hi > lo else c)
    return cuts


def _segment_glyphs(crop):
    """CS cell -> per-digit grayscale glyph boxes (left->right). Connected components, then split a
    merged blob (width >> single-digit width) at projection valleys -- EasyOCR drops a merged CS
    digit ('44'->'4', '80'->'8'); explicit segmentation recovers each."""
    gray = cv2.cvtColor(crop, cv2.COLOR_BGR2GRAY)
    g = cv2.GaussianBlur(gray, (3, 3), 0)
    _, bw = cv2.threshold(g, 0, 255, cv2.THRESH_BINARY + cv2.THRESH_OTSU)
    if bw.mean() > 127:
        bw = 255 - bw
    n, lab, st, _ = cv2.connectedComponentsWithStats(bw, 8)
    bl = [(st[c, cv2.CC_STAT_LEFT], st[c, cv2.CC_STAT_TOP], st[c, cv2.CC_STAT_WIDTH], st[c, cv2.CC_STAT_HEIGHT])
          for c in range(1, n) if st[c, cv2.CC_STAT_AREA] >= 12 and st[c, cv2.CC_STAT_HEIGHT] >= 8]
    if not bl:
        return []
    bl.sort()                                                # left -> right
    sw = 0.72 * float(np.median([h for _, _, _, h in bl]))   # expected single-digit width from height
    # Trim edge slivers: a digit from the ADJACENT scoreboard column (KDA) bleeds into the ROI's
    # left edge (blue) / right edge (red) as a detached blob. Digits within a number are kerned tight
    # (gap << sw); a bled neighbour sits ~a full digit-gap away. Drop boundary blobs separated from
    # the rest by > 0.8*sw. This -- NOT "trailing 0" -- is why 3-digit CS failed late-game: the extra
    # blob made a 4-digit value that exceeded MAX_CS, so read_cs rejected the whole cell as None.
    while len(bl) >= 2 and (bl[1][0] - (bl[0][0] + bl[0][2])) > 0.8 * sw:
        bl.pop(0)
    while len(bl) >= 2 and (bl[-1][0] - (bl[-2][0] + bl[-2][2])) > 0.8 * sw:
        bl.pop()
    glyphs = []
    for x, y, w, h in bl:                                    # already sorted; edge slivers trimmed
        if w > 1.4 * sw:                                      # merged -> split at projection valleys
            ndig = max(2, int(round(w / sw)))
            col = bw[y:y+h, x:x+w].sum(axis=0).astype(np.float32)
            xs = [0] + _split_cuts(col, ndig) + [w]
            for a, b in zip(xs[:-1], xs[1:]):
                if b - a >= 2:
                    glyphs.append(gray[y:y+h, x+a:x+b])
        else:
            glyphs.append(gray[y:y+h, x:x+w])
    return glyphs


def read_cs(reader, crop):
    """Per-player CS off the detail scoreboard. The HUD font is fixed, so segment (connected
    components + projection-valley split of merged blobs) then classify each WHOLE glyph by NCC vs
    canonical templates -- EasyOCR drops/merges adjacent CS digits ('44'->'4', '80'->'8'; measured
    8/8 vs EasyOCR 6/8 out-of-sample). Returns int in [0,MAX_CS], or None when a glyph's classifier
    confidence is low (so the CountGate holds). Falls back to EasyOCR if the template asset is absent."""
    if _DIGIT_TMPL is None:
        return _read_cs_easyocr(reader, crop)
    glyphs = _segment_glyphs(crop)
    if not glyphs:
        return None
    ds, conf = [], 1.0
    for gph in glyphs:
        d, s = _classify_glyph(gph)
        if d is None:
            return None
        ds.append(d); conf = min(conf, s)
    if conf < _CS_MIN_CONF:
        return None
    v = int(''.join(ds))
    return v if 0 <= v <= MAX_CS else None


def _read_cs_easyocr(reader, crop):
    """Fallback CS reader (EasyOCR + crop-pad) -- used only if digit_templates.npz is missing."""
    gray = cv2.cvtColor(crop, cv2.COLOR_BGR2GRAY)
    gray = cv2.GaussianBlur(gray, (3, 3), 0)
    _, bw = cv2.threshold(gray, 0, 255, cv2.THRESH_BINARY + cv2.THRESH_OTSU)
    if bw.mean() > 127:
        bw = 255 - bw
    bw = _crop_pad_digits(bw, 12)
    if bw is None:
        return None
    up = cv2.resize(bw, None, fx=8, fy=8, interpolation=cv2.INTER_CUBIC)
    txt = ''.join(reader.readtext(up, allowlist='0123456789', detail=0, paragraph=False)).replace(' ', '')
    d = re.sub(r'\D', '', txt)
    if not d:
        return None
    v = int(d)
    return v if 0 <= v <= MAX_CS else None


class TeamCsReader:
    """Reads all 10 per-player CS cells off the detail scoreboard (CS_ROI_1080). Per-cell
    change-detection (CS ticks slowly) + a per-cell CountGate (median-of-K; CS is not strictly
    monotonic across replay seeks, so CountGate, not TeamGate). Emits {cell: {state,value,age_ms}}
    in B0..B4,R0..R4 order. The authoritative coarse value is the API; C# only ACCEPTS an OCR
    cell when it rounds to the API's value, so a misread or panel-closed garbage is rejected."""
    CHANGE_MAD = 2.0

    def __init__(self, reader, roi=CS_ROI_1080):
        self.reader = reader; self.roi = roi
        self.gate = {k: CountGate() for k in roi}
        self._prev = {}   # cell -> (gray_crop, (val, raw))

    def candidate(self, img, cell):
        x0, x1, y0, y1 = self.roi[cell]
        crop = img[y0:y1, x0:x1]
        gray = cv2.cvtColor(crop, cv2.COLOR_BGR2GRAY)
        prev = self._prev.get(cell)
        if prev is not None:
            pgray, presult = prev
            if pgray.shape == gray.shape and \
               float(np.mean(np.abs(gray.astype(np.int16) - pgray.astype(np.int16)))) < self.CHANGE_MAD:
                return presult
        val = read_cs(self.reader, crop)
        result = (val, '' if val is None else str(val))
        if val is not None:                # cache only successful reads (see TeamTowerReader note)
            self._prev[cell] = (gray, result)
        return result

    def update(self, img, t_ms):
        res = {}
        for cell in self.roi:
            cand, raw = self.candidate(img, cell)
            g = self.gate[cell].feed(cand, t_ms)
            res[cell] = {**g, 'raw': raw}
        return res


class TeamGate:
    """Median-of-K + strictly-monotonic / bounded gate with hold-last-good.
    Feed range-checked candidates (or None for a failed/rejected read).
    - Top-bar team gold is CUMULATIVE EARNED gold => strictly non-decreasing. So any downward
      candidate is a mis-read and is rejected (this kills leading-digit-drop mis-reads like
      '18.6k'->'8.6k'). Upward jumps are bounded (teamfight margin); over-large jumps are held.
    - median of last K *valid* reads suppresses a single-frame in-range blip.
    - first lock waits for K reads and uses their median (no latching on one early mis-read).
    - None this frame => hold (Stale, then Unknown by age)."""
    K = 3
    MAX_RATE = 400      # gold/sec sustained plausible (per team)
    JUMP_BASE = 2500    # max single-step jump up (teamfight burst margin)
    STALE_MAX_MS = 5000 # hold a value this long before going Unknown

    def __init__(self):
        self.good = None; self.t_good = None; self.buf = []

    @staticmethod
    def _median(xs):
        s = sorted(xs); n = len(s)
        return s[n // 2] if n % 2 else (s[n // 2 - 1] + s[n // 2]) // 2

    def feed(self, cand, t_ms):
        if cand is None:                       # this frame produced no valid read -> hold
            return self._hold(t_ms)
        self.buf.append(cand); self.buf = self.buf[-self.K:]
        prop = self._median(self.buf)          # robust to a single outlier in the window
        if self.good is None:
            if len(self.buf) < self.K:         # don't latch on one early misread; wait for K reads
                return self._hold(t_ms)
            return self._accept(prop, t_ms)    # first lock = median of K (outlier-robust)
        dt = max(0.0, (t_ms - self.t_good) / 1000.0)
        up_lim = self.good + self.JUMP_BASE + self.MAX_RATE * dt
        if self.good <= prop <= up_lim:        # monotonic up, within plausible rate -> accept
            return self._accept(prop, t_ms)
        return self._hold(t_ms)                # downward (gold never decreases) or implausible jump -> hold

    def _accept(self, v, t):
        self.good = v; self.t_good = t
        return {'state': 'Known', 'value': v, 'age_ms': 0}

    def _hold(self, t):
        if self.good is None:
            return {'state': 'Unknown', 'value': None, 'age_ms': None}
        age = t - self.t_good
        if age <= self.STALE_MAX_MS:
            return {'state': 'Stale', 'value': self.good, 'age_ms': age}
        return {'state': 'Unknown', 'value': None, 'age_ms': age}


class TeamGoldReader:
    MAX_TEAM_DIFF = 25000   # the two teams' gold never differ by more than this; a candidate
                            # that breaks it (vs the other team's tracked value) is a misread.

    CHANGE_MAD = 2.5   # mean-abs-diff below this => crop unchanged => skip the (expensive) OCR

    def __init__(self, reader, roi=ROI_1080):
        self.reader = reader; self.roi = roi
        self.gate = {'blue': TeamGate(), 'red': TeamGate()}
        self._prev = {}   # team -> (gray_crop, result_tuple)  for change-detection

    def candidate(self, img, team):
        x0, x1, y0, y1 = self.roi[team]
        crop = img[y0:y1, x0:x1]
        gray = cv2.cvtColor(crop, cv2.COLOR_BGR2GRAY)
        # Change detection: team gold ticks only every few seconds; if the crop is unchanged
        # from last frame, reuse the previous OCR result and skip the neural inference entirely.
        # This turns ~8 easyocr calls/sec into "only when the digits actually change".
        prev = self._prev.get(team)
        if prev is not None:
            pgray, presult = prev
            if pgray.shape == gray.shape and \
               float(np.mean(np.abs(gray.astype(np.int16) - pgray.astype(np.int16)))) < self.CHANGE_MAD:
                return presult
        raw = ocr_crop(self.reader, crop, color=team)
        val = parse_gold(raw)
        in_range = val is not None and MIN_GOLD <= val <= MAX_GOLD
        result = ((val if in_range else None), raw, val)
        # Cache only a successful (in-range) read so a failed read is re-attempted next frame rather
        # than frozen by change-detection (matters when the crop is static, e.g. a paused replay).
        if in_range:
            self._prev[team] = (gray, result)
        return result

    def update(self, img, t_ms):
        cands = {t: list(self.candidate(img, t)) for t in ('blue', 'red')}
        cb, cr = cands['blue'][0], cands['red'][0]
        gb, gr = self.gate['blue'].good, self.gate['red'].good
        # Initial lock: if neither team is locked yet and the two candidates implausibly disagree,
        # we cannot tell which is misreading -> hold BOTH (Unknown) until they agree. Prevents
        # latching a systematic mis-read (e.g. '17.9k'->'47.9k') before an anchor exists.
        if cb is not None and cr is not None and abs(cb - cr) > self.MAX_TEAM_DIFF \
           and gb is None and gr is None:
            cands['blue'][0] = cands['red'][0] = None
        # Once one team is locked, it anchors the other: reject the other's wildly-divergent reads.
        for team, other in (('blue', 'red'), ('red', 'blue')):
            c = cands[team][0]; og = self.gate[other].good
            if c is not None and og is not None and abs(c - og) > self.MAX_TEAM_DIFF:
                cands[team][0] = None
        res = {}
        for team in ('blue', 'red'):
            cand, raw, parsed = cands[team]
            g = self.gate[team].feed(cand, t_ms)
            res[team] = {**g, 'raw': raw, 'parsed': parsed, 'accepted_cand': cand}
        return res


class CountGate:
    """Median-of-K vote for a small bounded counter (tower takedowns) read straight off the HUD.
    Unlike TeamGate (gold), this does NOT assume monotonic growth: a replay seek legitimately
    moves the count DOWN, and the HUD is authoritative, so we accept the voted value either way.
    median-of-K suppresses a single-frame misread; first lock waits for K reads; None this frame
    => hold (Stale, then Unknown by age)."""
    K = 3
    STALE_MAX_MS = 5000

    def __init__(self):
        self.good = None; self.t_good = None; self.buf = []

    @staticmethod
    def _median(xs):
        s = sorted(xs); n = len(s)
        return s[n // 2] if n % 2 else (s[n // 2 - 1] + s[n // 2]) // 2

    def feed(self, cand, t_ms):
        if cand is None:
            return self._hold(t_ms)
        self.buf.append(cand); self.buf = self.buf[-self.K:]
        prop = self._median(self.buf)
        if self.good is None and len(self.buf) < self.K:
            return self._hold(t_ms)            # don't latch on one early misread; wait for K reads
        return self._accept(prop, t_ms)

    def _accept(self, v, t):
        self.good = v; self.t_good = t
        return {'state': 'Known', 'value': v, 'age_ms': 0}

    def _hold(self, t):
        if self.good is None:
            return {'state': 'Unknown', 'value': None, 'age_ms': None}
        age = t - self.t_good
        if age <= self.STALE_MAX_MS:
            return {'state': 'Stale', 'value': self.good, 'age_ms': age}
        return {'state': 'Unknown', 'value': None, 'age_ms': age}


class TeamTowerReader:
    """Reads each team's tower-takedown count off the top bar (TOWER_ROI_1080), with the same
    crop change-detection as TeamGoldReader (the digit changes only when a tower falls) and a
    per-team CountGate. Authoritative HUD source -> used in Replay mode where /eventdata is sparse."""
    CHANGE_MAD = 2.0   # tower digit changes rarely; skip OCR when the crop is unchanged

    def __init__(self, reader, roi=TOWER_ROI_1080):
        self.reader = reader; self.roi = roi
        self.gate = {'blue': CountGate(), 'red': CountGate()}
        self._prev = {}   # team -> (gray_crop, (val, raw))

    def candidate(self, img, team):
        x0, x1, y0, y1 = self.roi[team]
        crop = img[y0:y1, x0:x1]
        gray = cv2.cvtColor(crop, cv2.COLOR_BGR2GRAY)
        prev = self._prev.get(team)
        if prev is not None:
            pgray, presult = prev
            if pgray.shape == gray.shape and \
               float(np.mean(np.abs(gray.astype(np.int16) - pgray.astype(np.int16)))) < self.CHANGE_MAD:
                return presult
        val = read_count(self.reader, crop, team)
        result = (val, '' if val is None else str(val))
        # Cache ONLY a successful read. A failed read (None) must NOT be cached: tower digits stay
        # byte-stable for minutes, so a cached None would be returned every frame (change-detection
        # skips re-OCR) and freeze the gate to Unknown forever. Leaving _prev untouched re-attempts
        # OCR next frame until it succeeds.
        if val is not None:
            self._prev[team] = (gray, result)
        return result

    def update(self, img, t_ms):
        res = {}
        for team in ('blue', 'red'):
            cand, raw = self.candidate(img, team)
            g = self.gate[team].feed(cand, t_ms)
            res[team] = {**g, 'raw': raw}
        return res


class TeamObjectiveReader:
    """Reads both teams' grub/baron/dragon/tower counts off the observer top bar
    (OBJ_ROI_1080). Per-counter crop change-detection (a count changes only when an
    objective falls) + per-counter CountGate (median-of-K; non-monotonic so a replay
    seek's downward move is honored). Each ROI is OCR'd with its OWN Otsu (read_count)
    -- the authoritative HUD source, since /eventdata never carries these kills in
    spectator/replay. Emits {key: {state,value,age_ms,raw}} for the 8 OBJ_ROI keys."""
    CHANGE_MAD = 2.0   # objective digits change rarely; skip OCR when the crop is unchanged

    def __init__(self, reader, roi=OBJ_ROI_1080):
        self.reader = reader
        self.roi = roi
        self.gate = {k: CountGate() for k in roi}
        self._prev = {}   # key -> (gray_crop, (val, raw))

    def candidate(self, img, key):
        x0, x1, y0, y1 = self.roi[key]
        crop = img[y0:y1, x0:x1]
        gray = cv2.cvtColor(crop, cv2.COLOR_BGR2GRAY)
        prev = self._prev.get(key)
        if prev is not None:
            pgray, presult = prev
            if pgray.shape == gray.shape and \
               float(np.mean(np.abs(gray.astype(np.int16) - pgray.astype(np.int16)))) < self.CHANGE_MAD:
                return presult
        color = 'blue' if key.startswith('blue') else 'red'
        obj = key.split('_')[1]
        val = read_count(self.reader, crop, color, OBJ_MAX[obj])
        result = (val, '' if val is None else str(val))
        # Cache ONLY a successful read (same reasoning as TeamTowerReader: a cached None would
        # be frozen by change-detection forever; leaving _prev untouched re-attempts next frame).
        if val is not None:
            self._prev[key] = (gray, result)
        return result

    def update(self, img, t_ms):
        res = {}
        for key in self.roi:
            cand, raw = self.candidate(img, key)
            g = self.gate[key].feed(cand, t_ms)
            res[key] = {**g, 'raw': raw}
        return res


# ----------------------------- tests -----------------------------
if __name__ == '__main__':
    import os, easyocr
    # Folder of native-HUD screenshots to re-run this OCR test against (per-user; set via env).
    BASE = os.environ.get('OFUI_OCR_TEST_FRAMES', '.')
    SEQ = [  # (file, blue_gt_sum, red_gt_sum)  -- real frames, used as per-frame OCR test
        ('League of Legends Screenshot 2026.06.11 - 00.41.17.95.png', 31954, 34754),
        ('League of Legends Screenshot 2026.06.11 - 01.07.35.87.png', 54104, 56668),
        ('League of Legends Screenshot 2026.06.11 - 01.18.20.73.png', None, None),
        ('League of Legends Screenshot 2026.06.11 - 01.20.57.91.png', 55819, None),
        ('League of Legends Screenshot 2026.06.11 - 01.25.31.92.png', 34708, 37783),
        ('League of Legends Screenshot 2026.06.11 - 01.34.46.79.png', 53104, 55760),
    ]
    reader = easyocr.Reader(['en'], gpu=False, verbose=False)
    rdr = TeamGoldReader(reader)

    print('=== TEST A: per-frame OCR + range check (no gate state shared) ===')
    okc = tot = 0
    for fn, gb, gr in SEQ:
        img = cv2.imread(os.path.join(BASE, fn))
        for team, gt in (('blue', gb), ('red', gr)):
            cand, raw, parsed = rdr.candidate(img, team)
            verdict = 'reject' if cand is None else 'accept'
            mark = ''
            if gt is not None:
                exp = round(gt / 100) * 100
                tot += 1; ok = (cand == exp); okc += ok
                mark = 'OK' if ok else ('HELD(reject->hold last)' if cand is None else f'WRONG(exp {exp})')
            print(f'  {fn[-9:-4]} {team:4} raw={raw!r:8} parsed={str(parsed):>8} -> {verdict:6} cand={str(cand):>7} {mark}')
    print(f'  accepted-candidate accuracy on range-passing reads: {okc}/{tot}')

    print('\n=== TEST B: gate on a synthetic 4Hz sequence (valid + injected errors) ===')
    g = TeamGate()
    true = 30000
    inj = {12: None, 13: None,          # two dropped frames (OCR fail)
           20: 999999,                  # 10x '.'-misread -> rejected by range check -> hold
           28: 31650}                   # a single in-range blip (+~500) -> median should suppress
    t = 0
    for i in range(40):
        true += 80 // 4 + (300 if i in (10, 25) else 0)   # ~80g/s + two kills
        cand = true
        if i in inj:
            v = inj[i]
            cand = None if (v is None or not (MIN_GOLD <= v <= MAX_GOLD)) else v
        out = g.feed(cand, t)
        flag = '' if out['value'] in (true, None) or abs((out['value'] or true) - true) <= 60 else '  <-- shows wrong!'
        if i in inj or i % 8 == 0:
            print(f"  t={t:5}ms true={true:6} fed={str(cand):>7} -> {out['state']:7} val={str(out['value']):>7} age={out['age_ms']}{flag}")
        t += 250
