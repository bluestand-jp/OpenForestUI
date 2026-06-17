"""Live gold-capture sidecar (Stage 1).
Captures the observer top bar via dxcam (DXGI Desktop Duplication), runs the validated
top-bar reader, and emits per-team tri-state gold.

Modes:
  --grab PATH      capture one full desktop frame, save to PATH (for ROI calibration)
  --probe          capture one frame, run the reader once, print result (no game = Unknown)
  --live           continuous loop: capture -> read -> print tri-state JSON lines (~FPS)

ROI: defaults to the 1080p calibration auto-scaled to the capture resolution. If the LoL UI
on this monitor does NOT scale proportionally with resolution, pass an explicit --roi.
"""
import argparse, time, json, sys, threading
import cv2, numpy as np, dxcam
from topbar_reader import (TeamGoldReader, TeamTowerReader, TeamCsReader, TeamObjectiveReader, TeamGate,
                           ROI_1080, TOWER_ROI_1080, CS_ROI_1080, OBJ_ROI_1080, MIN_GOLD, MAX_GOLD)

def scaled_roi(W, H, base=ROI_1080):
    sx, sy = W / 1920.0, H / 1080.0
    return {t: (int(x0*sx), int(x1*sx), int(y0*sy), int(y1*sy))
            for t, (x0, x1, y0, y1) in base.items()}

def make_cam():
    return dxcam.create(output_color='BGR')

def grab_full(cam, tries=40):
    for _ in range(tries):
        f = cam.grab()
        if f is not None:
            return f
        time.sleep(0.03)
    return None

def cmd_grab(path):
    cam = make_cam(); f = grab_full(cam)
    if f is None: print('no frame captured'); return
    cv2.imwrite(path, f); print(f'saved {path}  shape={f.shape}')

def cmd_probe(roi_override):
    import easyocr
    cam = make_cam(); f = grab_full(cam)
    if f is None: print('no frame'); return
    H, W = f.shape[:2]; roi = roi_override or scaled_roi(W, H)
    print(f'capture {W}x{H}  ROI={roi}')
    rdr = TeamGoldReader(easyocr.Reader(['en'], gpu=False, verbose=False), roi=roi)
    # save the two ROI crops so we can verify calibration visually
    for t, (x0, x1, y0, y1) in roi.items():
        cv2.imwrite(f'ocr-poc/out/live_{t}TB.png', cv2.resize(f[y0:y1, x0:x1], None, fx=6, fy=6, interpolation=cv2.INTER_NEAREST))
    res = rdr.update(f, 0)
    print(json.dumps(res, ensure_ascii=False))
    print('saved ROI crops -> ocr-poc/out/live_blueTB.png, live_redTB.png')

def _fmt(team):
    v = team['value']; s = team['state']
    vs = f'{v/1000:.1f}k' if v is not None else '  --  '
    return f"{s:7} {vs:>7}"

def cmd_live(fps, roi_override, log_path=None, debugcrops=None):
    import easyocr, os
    cam = make_cam(); f = grab_full(cam)
    if f is None: print('no frame'); return
    H, W = f.shape[:2]; roi = roi_override or scaled_roi(W, H)
    print(f'[goldcap] capture {W}x{H}  fps={fps}  ROI={roi}  (Ctrl+C to stop)', file=sys.stderr)
    reader = easyocr.Reader(['en'], gpu=False, verbose=False)
    rdr = TeamGoldReader(reader, roi=roi)
    trdr = TeamTowerReader(reader, roi=scaled_roi(W, H, TOWER_ROI_1080))   # reuse the same OCR model
    csrdr = TeamCsReader(reader, roi=scaled_roi(W, H, CS_ROI_1080))        # per-player CS (detail scoreboard)
    ordr = TeamObjectiveReader(reader, roi=scaled_roi(W, H, OBJ_ROI_1080)) # grub/baron/dragon/tower per team
    # Seek-reset: the C# (which holds the authoritative gameTime) writes "reset" on our stdin when
    # the replay seeks (backward or a large jump). We then re-create the readers so every gate
    # re-locks at the NEW timeline instead of holding stale pre-seek values -- gold's gate is
    # monotonic and would otherwise stay stuck at the pre-seek max; the count gates would keep
    # stale counts that the C# tolerance then rejects (CS falls back to the coarse API floor).
    _reset_evt = threading.Event()
    def _stdin_watch():
        try:
            while True:
                line = sys.stdin.readline()    # readline (not the buffered iterator) so "reset\n" arrives promptly under -u
                if not line:
                    break
                if 'reset' in line.lower():
                    _reset_evt.set()
        except Exception:
            pass
    threading.Thread(target=_stdin_watch, daemon=True).start()
    interval = 1.0 / fps; t0 = time.perf_counter(); last = f
    logf = open(log_path, 'w', encoding='utf-8') if log_path else None
    if debugcrops: os.makedirs(debugcrops, exist_ok=True)
    n = 0; known_b = known_r = 0; saved = 0
    try:
        while True:
            if _reset_evt.is_set():                    # replay seek -> re-lock every gate at the new time
                _reset_evt.clear()
                rdr = TeamGoldReader(reader, roi=roi)
                trdr = TeamTowerReader(reader, roi=scaled_roi(W, H, TOWER_ROI_1080))
                csrdr = TeamCsReader(reader, roi=scaled_roi(W, H, CS_ROI_1080))
                ordr = TeamObjectiveReader(reader, roi=scaled_roi(W, H, OBJ_ROI_1080))
                print('[goldcap] seek reset: readers re-locked', file=sys.stderr)
            g = cam.grab()
            if g is not None: last = g
            t_ms = int((time.perf_counter() - t0) * 1000)
            res = rdr.update(last, t_ms)
            tres = trdr.update(last, t_ms)
            csres = csrdr.update(last, t_ms)
            ores = ordr.update(last, t_ms)
            if debugcrops and saved < 60:   # save raw ROI crops to diagnose recognition
                for tm in ('blue', 'red'):
                    x0, x1, y0, y1 = roi[tm]
                    raw = res[tm]['raw'].replace('.', 'p') or 'EMPTY'
                    cv2.imwrite(f"{debugcrops}/{n:03d}_{tm}_{raw}.png",
                                cv2.resize(last[y0:y1, x0:x1], None, fx=6, fy=6, interpolation=cv2.INTER_NEAREST))
                saved += 1
            out = {'t': t_ms,
                   'blue': {k: res['blue'][k] for k in ('state', 'value', 'age_ms')},
                   'red':  {k: res['red'][k]  for k in ('state', 'value', 'age_ms')},
                   'raw_blue': res['blue']['raw'], 'raw_red': res['red']['raw'],
                   'towers': {'blue': {k: tres['blue'][k] for k in ('state', 'value', 'age_ms')},
                              'red':  {k: tres['red'][k]  for k in ('state', 'value', 'age_ms')}},
                   # per-player CS in B0..B4,R0..R4 order (== /playerlist 0..9)
                   'cs': [{k: csres[cell][k] for k in ('state', 'value')}
                          for cell in ('B0','B1','B2','B3','B4','R0','R1','R2','R3','R4')],
                   # per-team objective counts (grub/baron/dragon/tower) from the top-bar OCR
                   'obj': {tm: {o: {k: ores[f'{tm}_{o}'][k] for k in ('state', 'value')}
                                for o in ('grub', 'baron', 'dragon', 'tower')}
                           for tm in ('blue', 'red')}}
            line = json.dumps(out)
            print(line, flush=True)                                  # stdout: JSON (pipe target)
            if logf: logf.write(line + '\n'); logf.flush()
            n += 1
            known_b += res['blue']['state'] == 'Known'; known_r += res['red']['state'] == 'Known'
            # stderr: human-readable live view
            print(f"\rBLUE {_fmt(res['blue'])}  raw={res['blue']['raw']!r:8} | "
                  f"RED {_fmt(res['red'])}  raw={res['red']['raw']!r:8}  "
                  f"TOW b={str(tres['blue']['value']):>2} r={str(tres['red']['value']):>2}  "
                  f"[Known b={known_b}/{n} r={known_r}/{n}]", end='', file=sys.stderr, flush=True)
            time.sleep(interval)
    except KeyboardInterrupt:
        print('', file=sys.stderr)
    finally:
        if logf: logf.close()
        del cam

if __name__ == '__main__':
    ap = argparse.ArgumentParser()
    ap.add_argument('--grab', metavar='PATH')
    ap.add_argument('--probe', action='store_true')
    ap.add_argument('--live', action='store_true')
    ap.add_argument('--fps', type=float, default=4)
    ap.add_argument('--roi', help='blue/red as x0,x1,y0,y1;x0,x1,y0,y1')
    ap.add_argument('--log', metavar='PATH', help='write JSONL log of every frame')
    ap.add_argument('--debugcrops', metavar='DIR', help='save first ~60 ROI crops for diagnosis')
    a = ap.parse_args()
    roi = None
    if a.roi:
        b, r = a.roi.split(';'); pb = tuple(map(int, b.split(','))); pr = tuple(map(int, r.split(',')))
        roi = {'blue': pb, 'red': pr}
    if a.grab: cmd_grab(a.grab)
    elif a.probe: cmd_probe(roi)
    elif a.live: cmd_live(a.fps, roi, a.log, a.debugcrops)
    else: ap.print_help()
