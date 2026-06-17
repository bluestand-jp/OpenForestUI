# -*- coding: utf-8 -*-
# Harvest labeled digit glyphs from NATIVE HUD frames only (exclude overlay renders -- different
# font), build canonical templates (per-digit median over size-normalized samples), save + montage.
import cv2, numpy as np, easyocr, re, os
from topbar_reader import _emphasis, ocr_crop, read_cs, read_count, parse_gold, ROI_1080, CS_ROI_1080, OBJ_ROI_1080, OBJ_MAX
RD = easyocr.Reader(['en'], gpu=False, verbose=False)
samples = {'top': {d: [] for d in '0123456789'}, 'cs': {d: [] for d in '0123456789'}}

# NATIVE captures only (NOT screen_overlay/mock/live0 -- those are our PRM overlay's own font)
NATIVE = ['cap1.png', 'cs_live.png', 'final.png', 'screen_cur.png', 'screen_measure.png', 'screen_verify.png', 'screen_final.png']

def bw_blobs(crop, mode):
    if mode == 'cs':
        g = cv2.GaussianBlur(cv2.cvtColor(crop, cv2.COLOR_BGR2GRAY), (3, 3), 0)
        _, bw = cv2.threshold(g, 0, 255, cv2.THRESH_BINARY + cv2.THRESH_OTSU)
        if bw.mean() > 127: bw = 255 - bw
        base = cv2.cvtColor(crop, cv2.COLOR_BGR2GRAY)
    else:
        g = cv2.GaussianBlur(_emphasis(crop, mode), (3, 3), 0)
        _, bw = cv2.threshold(g, 0, 255, cv2.THRESH_BINARY + cv2.THRESH_OTSU)
        base = _emphasis(crop, mode)
    n, lab, st, _ = cv2.connectedComponentsWithStats(bw, 8)
    bl = [(st[c, cv2.CC_STAT_LEFT], st[c, cv2.CC_STAT_TOP], st[c, cv2.CC_STAT_WIDTH], st[c, cv2.CC_STAT_HEIGHT])
          for c in range(1, n) if st[c, cv2.CC_STAT_AREA] >= 12 and st[c, cv2.CC_STAT_HEIGHT] >= 8]
    return sorted(bl), base

def harvest(crop, digits, mode, ctx, drop_last=False):
    bl, base = bw_blobs(crop, mode)
    if drop_last and bl: bl = bl[:-1]
    if not digits or len(bl) != len(digits): return
    ws = [b[2] for b in bl]
    if len(bl) > 1 and max(ws) > 1.4 * sorted(ws)[len(ws)//2]: return   # merged blob -> skip
    for (x, y, w, h), d in zip(bl, digits):
        samples[ctx][d].append(base[y:y+h, x:x+w])

for fn in NATIVE:
    img = cv2.imread('overlay-harness/' + fn)
    if img is None: continue
    for tm, (x0, x1, y0, y1) in ROI_1080.items():
        crop = img[y0:y1, x0:x1]; g = parse_gold(ocr_crop(RD, crop, tm))
        if g: harvest(crop, re.sub(r'\D', '', str(g // 100)), tm, 'top', drop_last=True)
    for k, (x0, x1, y0, y1) in OBJ_ROI_1080.items():
        col = 'blue' if 'blue' in k else 'red'; crop = img[y0:y1, x0:x1]
        v = read_count(RD, crop, col, OBJ_MAX[k.split('_')[1]])
        if v is not None: harvest(crop, str(v), col, 'top')
    for cell, (x0, x1, y0, y1) in CS_ROI_1080.items():
        crop = img[y0:y1, x0:x1]; v = read_cs(RD, crop)
        if v is not None: harvest(crop, str(v), 'cs', 'cs')

# canonical = per-digit median over samples resized to the digit's median (w,h)
templates = {}
for ctx in ('top', 'cs'):
    for d in '0123456789':
        S = samples[ctx][d]
        if not S: print('  !! %s %s no sample' % (ctx, d)); continue
        mw = int(np.median([s.shape[1] for s in S])); mh = int(np.median([s.shape[0] for s in S]))
        stack = np.stack([cv2.resize(s, (mw, mh), interpolation=cv2.INTER_AREA) for s in S]).astype(np.float32)
        templates['%s_%s' % (ctx, d)] = np.median(stack, axis=0).astype(np.uint8)

np.savez('digit_templates.npz', **templates)
print('saved digit_templates.npz  (%d templates)' % len(templates))
for ctx in ('top', 'cs'):
    print('  %-4s n_samples %s' % (ctx, {d: len(samples[ctx][d]) for d in '0123456789'}))

# montage for visual sanity (top row 0-9, cs row 0-9), scaled
cell_w, cell_h = 24, 28
mont = np.zeros((cell_h * 2 + 10, cell_w * 10 + 10), np.uint8)
for r, ctx in enumerate(('top', 'cs')):
    for i, d in enumerate('0123456789'):
        t = templates.get('%s_%s' % (ctx, d))
        if t is None: continue
        t = cv2.resize(t, (min(cell_w-4, t.shape[1]*2), min(cell_h-4, t.shape[0]*2)), interpolation=cv2.INTER_NEAREST)
        y0 = r * (cell_h + 4) + 2; x0 = i * cell_w + 2
        mont[y0:y0+t.shape[0], x0:x0+t.shape[1]] = t
cv2.imwrite('overlay-harness/templates_montage.png', cv2.resize(mont, None, fx=4, fy=4, interpolation=cv2.INTER_NEAREST))
print('wrote templates_montage.png (row0=top 0-9, row1=cs 0-9)')
