"""Replay recorded raw OCR reads through the (fixed) parse + gate to validate improvements
without a new live run."""
import json, os
from collections import Counter
from topbar_reader import parse_gold, TeamGate, TeamGoldReader, MIN_GOLD, MAX_GOLD
LOG = os.path.join(os.path.dirname(__file__), 'out', 'livelog.jsonl')
rows = [json.loads(l) for l in open(LOG, encoding='utf-8') if l.strip()]
print(f'frames={len(rows)}')
gates = {'blue': TeamGate(), 'red': TeamGate()}
states = {'blue': Counter(), 'red': Counter()}; vals = {'blue': [], 'red': []}
MTD = TeamGoldReader.MAX_TEAM_DIFF
for r in rows:
    cands = {}
    for team in ('blue', 'red'):
        v = parse_gold(r.get('raw_' + team, ''))
        cands[team] = v if (v is not None and MIN_GOLD <= v <= MAX_GOLD) else None
    cb, cr = cands['blue'], cands['red']; gb, gr = gates['blue'].good, gates['red'].good
    if cb is not None and cr is not None and abs(cb - cr) > MTD and gb is None and gr is None:
        cands['blue'] = cands['red'] = None
    for team, other in (('blue', 'red'), ('red', 'blue')):   # cross-team anchor (mirrors update())
        c = cands[team]; og = gates[other].good
        if c is not None and og is not None and abs(c - og) > MTD:
            cands[team] = None
    for team in ('blue', 'red'):
        out = gates[team].feed(cands[team], r['t'])
        states[team][out['state']] += 1
        if out['value'] is not None:
            vals[team].append((r['t'], out['value']))
for team in ('blue', 'red'):
    v = vals[team]
    back = [(v[i-1], v[i]) for i in range(1, len(v)) if v[i][1] < v[i-1][1]]
    kn = states[team]['Known'] / len(rows) * 100
    print(f'\n{team}: {dict(states[team])}  Known%={kn:.1f}')
    print(f'  monotonic backsteps={len(back)}  ' + ('; '.join(f'{a[1]}->{b[1]}' for a, b in back[:6])))
    if v: print(f'  value range: {v[0][1]} .. {v[-1][1]}')
