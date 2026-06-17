"""One-shot CS-cell diagnostic. Captures the primary screen via PIL (GDI -- independent of the
sidecar's dxcam, so no conflict / no need to stop the app), crops the 10 CS cells with the live
CS_ROI, and for each prints read_cs + the segmentation breakdown (n glyphs, each glyph's NCC class
+ confidence + width). Saves each cell crop (6x) and its segmented glyphs to ocr-poc/csdiag/.

Goal: see WHY cells whose CS ends in 0 (220/160/180/60/50) return None while non-0 reads succeed."""
import os, sys
import numpy as np, cv2
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from topbar_reader import (CS_ROI_1080, read_cs, _segment_glyphs, _classify_glyph,
                           _DIGIT_TMPL, _CS_MIN_CONF, MAX_CS)

print('digit_templates loaded:', _DIGIT_TMPL is not None, ' _CS_MIN_CONF=', _CS_MIN_CONF)
try:
    from PIL import ImageGrab
    rgb = np.array(ImageGrab.grab())          # primary monitor, RGB
    img = cv2.cvtColor(rgb, cv2.COLOR_RGB2BGR)
except Exception as e:
    print('PIL ImageGrab failed:', e); sys.exit(1)

H, W = img.shape[:2]
print(f'capture {W}x{H}  mean_brightness={img.mean():.1f}')
if img.mean() < 8:
    print('SCREEN BLACK (exclusive fullscreen?) -- PIL cannot see it; need dxcam path'); sys.exit(2)

sx, sy = W / 1920.0, H / 1080.0
outdir = os.path.join(os.path.dirname(os.path.abspath(__file__)), 'csdiag')
os.makedirs(outdir, exist_ok=True)
names = ['B0','B1','B2','B3','B4','R0','R1','R2','R3','R4']
print(f'scale sx={sx:.3f} sy={sy:.3f}')
print(f'{"cell":4} {"read_cs":>7}  {"nGly":>4}  glyphs(digit:conf:w)            roiWxH')
for nm in names:
    x0,x1,y0,y1 = CS_ROI_1080[nm]
    X0,X1,Y0,Y1 = int(round(x0*sx)), int(round(x1*sx)), int(round(y0*sy)), int(round(y1*sy))
    crop = img[Y0:Y1, X0:X1]
    if crop.size == 0:
        print(f'{nm:4} EMPTY CROP ({X0}:{X1},{Y0}:{Y1})'); continue
    val = read_cs(None, crop)
    glyphs = _segment_glyphs(crop)
    parts = []
    for g in glyphs:
        d, s = _classify_glyph(g)
        parts.append(f'{d}:{s:.2f}:{g.shape[1]}')
    print(f'{nm:4} {str(val):>7}  {len(glyphs):>4}  {("  ".join(parts)):30}  {crop.shape[1]}x{crop.shape[0]}')
    cv2.imwrite(os.path.join(outdir, f'{nm}.png'),
                cv2.resize(crop, None, fx=6, fy=6, interpolation=cv2.INTER_NEAREST))
    # also save the binarized strip + each segmented glyph (so over-split / dropped 0 is visible)
    for gi, g in enumerate(glyphs):
        cv2.imwrite(os.path.join(outdir, f'{nm}_g{gi}.png'),
                    cv2.resize(g, None, fx=6, fy=6, interpolation=cv2.INTER_NEAREST))
print('saved crops -> ocr-poc/csdiag/')
