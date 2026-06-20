using OpenForestUI.Ingame.Data.Hub;
using System;
using System.Diagnostics;
using System.Text.Json;
using System.Threading;

namespace OpenForestUI.Common.Controllers
{
    /// <summary>
    /// Stage 3: launches the Python OCR sidecar (ocr-poc/goldcap.py --live) as a child process,
    /// reads its newline-delimited tri-state JSON from stdout on a background thread, and injects
    /// the gated team-gold values into the Team objects each tick via <see cref="ApplyTo"/>.
    ///
    /// The sidecar already does all OCR + gating (median / strict-monotonic / cross-team /
    /// hold-last) and emits {state: Known|Stale|Unknown, value}. C# only adds a wall-clock
    /// freshness guard (if the sidecar hangs, gold goes Unknown) and maps blue/red -> Team.
    ///
    /// It also emits a "towers" object (tower-takedown count per team, OCR'd from the same top
    /// bar). <see cref="ApplyTowers"/> injects those into Team.towers — but ONLY in Replay mode,
    /// where /eventdata is sparse after a seek and the HUD is the authoritative source. In
    /// Live/Spectator the event-counted towers are kept (see IngameController.DoTick).
    /// </summary>
    public static class OcrGoldController
    {
        // The interpreter the sidecar runs in. Resolved by OcrEnvController: the bundled embeddable
        // ./python when shipped (provisioned on first use), else the system "python" on PATH /
        // the OPENFORESTUI_PYTHON override in a source checkout.
        public static string PythonExe => OcrEnvController.InterpreterPath;
        // Resolved relative to the executable (works from a source checkout — bin/<cfg>/<tfm> — or a
        // published build where the publish step copies ocr-poc next to the exe). Override with the
        // OPENFORESTUI_OCR_SCRIPT environment variable.
        public static string ScriptPath = ResolveScriptPath();
        // 2 fps matches the C# tick rate; combined with the sidecar's crop change-detection
        // (it only runs OCR when the gold digits actually change), CPU stays low.
        public static int Fps = 2;

        private static string ResolveScriptPath()
        {
            var env = Environment.GetEnvironmentVariable("OPENFORESTUI_OCR_SCRIPT");
            if (!string.IsNullOrEmpty(env)) return env;
            var dir = AppContext.BaseDirectory;
            for (int i = 0; i < 6 && dir != null; i++)
            {
                var candidate = System.IO.Path.Combine(dir, "ocr-poc", "goldcap.py");
                if (System.IO.File.Exists(candidate)) return candidate;
                dir = System.IO.Directory.GetParent(dir)?.FullName;
            }
            return System.IO.Path.Combine("ocr-poc", "goldcap.py");
        }

        // Dead-sidecar guard: ignore IPC older than this (wall clock). A full OCR pass on CPU
        // (gold + tower + CS + objectives) takes ~6-7s/frame, so lines arrive only every few seconds.
        // The OLD value (2000ms) was SHORTER than that cadence, so most ticks saw "stale" IPC and fell
        // back to the API floor / estimate even though the sidecar's reads were good -- this was the
        // root cause of "everything shows the fallback value". The sidecar's own per-metric gates
        // (median + STALE_MAX_MS) already downgrade a genuinely old read to Stale/Unknown, so this only
        // needs to catch a TOTALLY dead sidecar -> keep it comfortably above the emit cadence.
        private const long FreshMs = 10000;

        private static readonly object _gate = new();
        private static Process _proc;
        private static volatile bool _running;
        private static volatile Snapshot _latest = Snapshot.Empty;

        // --- /debug-ocr live diagnostics (read-only; no effect on the pipeline) ---
        private static volatile string _lastRawLine = "";   // last stdout JSON line from the sidecar
        private static volatile string _lastStderr = "";    // last stderr line (banner / easyocr warning / crash)
        private static volatile CsDebugCell[] _csDbg = System.Array.Empty<CsDebugCell>();
        private static long _csApplyMs = long.MinValue / 2;  // wall clock of the last ApplyCs (64-bit: via Interlocked)

        /// <summary>Per-player CS decision trace surfaced by the debug endpoint.</summary>
        public sealed class CsDebugCell
        {
            public int idx { get; set; }
            public int? ocr { get; set; }       // sidecar OCR value (null = unread)
            public string state { get; set; }   // Known / Stale / Unknown
            public int api { get; set; }         // /playerlist floored value (-1 = no player)
            public bool accepted { get; set; }   // OCR taken over the API floor this tick?
            public int final { get; set; }       // value the scoreboard will show (-1 = none)
        }

        private sealed class Snapshot
        {
            public float? BlueGold; public GoldConfidence BlueConf;
            public float? RedGold; public GoldConfidence RedConf;
            // Tower-takedown counts (sidecar "towers" object). Used only in Replay mode.
            public int? BlueTowers; public GoldConfidence BlueTowerConf;
            public int? RedTowers; public GoldConfidence RedTowerConf;
            // Per-player CS (sidecar "cs" array), index 0..9 == /playerlist order
            // (0..4 = blue/ORDER by position, 5..9 = red/CHAOS). null when unread.
            public int?[] Cs; public GoldConfidence[] CsConf;
            // Objective counts (sidecar "obj" object), per team, index 0=grub 1=baron 2=dragon 3=tower.
            public int?[] BlueObj, RedObj; public GoldConfidence[] BlueObjConf, RedObjConf;
            public long RecvMs;
            public static readonly Snapshot Empty = new()
            {
                BlueConf = GoldConfidence.Unknown,
                RedConf = GoldConfidence.Unknown,
                BlueTowerConf = GoldConfidence.Unknown,
                RedTowerConf = GoldConfidence.Unknown,
                Cs = new int?[10],
                CsConf = NewUnknownConf(10),
                BlueObj = new int?[4], RedObj = new int?[4],
                BlueObjConf = NewUnknownConf(4), RedObjConf = NewUnknownConf(4),
                RecvMs = long.MinValue / 2
            };
            private static GoldConfidence[] NewUnknownConf(int n)
            {
                var a = new GoldConfidence[n];
                for (int i = 0; i < n; i++) a[i] = GoldConfidence.Unknown;
                return a;
            }
        }

        /// <summary>Start the sidecar once (idempotent). Safe to call every tick.</summary>
        public static void EnsureStarted()
        {
            if (_running) return;
            // Make sure the OCR Python environment is provisioned before launching the sidecar. If it
            // isn't Ready yet, kick off provisioning in the background and bail — the app keeps using
            // the gold/CS estimate fallback, and a later tick re-checks and starts the sidecar once
            // the deps finish downloading (DataDragon-style first-run provisioning).
            if (OcrEnvController.Status != OcrEnvStatus.Ready)
            {
                OcrEnvController.EnsureProvisioning();
                return;
            }
            lock (_gate)
            {
                if (_running) return;
                try
                {
                    _proc = new Process
                    {
                        StartInfo = new ProcessStartInfo
                        {
                            FileName = PythonExe,
                            Arguments = $"-u \"{ScriptPath}\" --live --fps {Fps}",   // -u: unbuffered stdio so the seek "reset" line reaches the sidecar promptly
                            RedirectStandardOutput = true,
                            RedirectStandardError = true,
                            RedirectStandardInput = true,    // C# -> sidecar "reset" on a replay seek
                            UseShellExecute = false,
                            CreateNoWindow = true,
                        }
                    };
                    _proc.Start();
                    _running = true;
                    new Thread(ReadStdout) { IsBackground = true, Name = "OcrGold-stdout" }.Start();
                    new Thread(DrainStderr) { IsBackground = true, Name = "OcrGold-stderr" }.Start();
                    Log.Info($"OCR gold sidecar started ({PythonExe} {ScriptPath})");
                }
                catch (Exception ex)
                {
                    _running = false;
                    Log.Warn($"Could not start OCR gold sidecar: {ex.Message}");
                }
            }
        }

        public static void Stop()
        {
            if (!_running && _proc == null) return;
            _running = false;
            try { if (_proc != null && !_proc.HasExited) _proc.Kill(true); }
            catch (Exception ex) { Log.Warn($"OCR gold sidecar stop: {ex.Message}"); }
            _proc = null;
            _latest = Snapshot.Empty;
            _lastTowerBlue = _lastTowerRed = null;
            System.Array.Clear(_lastObjBlue, 0, 4);
            System.Array.Clear(_lastObjRed, 0, 4);
        }

        /// <summary>
        /// On a replay seek the OCR gates' continuity breaks: the sidecar's gold gate is monotonic
        /// (stays stuck at the pre-seek max) and the count gates hold stale values that the CS
        /// tolerance then rejects (CS falls back to the coarse API floor). Tell the sidecar to
        /// re-create its readers (re-lock at the new time) and drop our own held last-good values.
        /// Called by IngameController.DoTick when gameTime jumps.
        /// </summary>
        public static void SignalSeekReset()
        {
            try
            {
                if (_proc != null && !_proc.HasExited && _proc.StartInfo.RedirectStandardInput)
                    _proc.StandardInput.WriteLine("reset");
            }
            catch (Exception ex) { Log.Warn($"OCR seek-reset signal: {ex.Message}"); }
            _lastTowerBlue = _lastTowerRed = null;
            System.Array.Clear(_lastObjBlue, 0, 4);
            System.Array.Clear(_lastObjRed, 0, 4);
        }

        public static bool IsRunning => _running;

        private static void ReadStdout()
        {
            try
            {
                string line;
                while (_running && (line = _proc.StandardOutput.ReadLine()) != null)
                {
                    _lastRawLine = line;          // for /debug-ocr
                    var s = Parse(line);
                    if (s != null) _latest = s;   // atomic reference swap
                }
            }
            catch (Exception ex) { Log.Warn($"OCR gold stdout reader ended: {ex.Message}"); }
            _running = false;
        }

        private static void DrainStderr()
        {
            // Keep the stderr pipe drained so the sidecar never blocks on a full buffer.
            try
            {
                string line;
                while (_proc != null && (line = _proc.StandardError.ReadLine()) != null)
                {
                    _lastStderr = line;          // for /debug-ocr (surfaces import errors / easyocr warnings)
                    // The sidecar prints a one-line "[goldcap] capture ..." banner + warnings here.
                    if (line.Contains("[goldcap]")) Log.Info($"sidecar: {line}");
                }
            }
            catch { /* process ended */ }
        }

        private static Snapshot Parse(string line)
        {
            // stdout is pure JSON lines; ignore anything that isn't valid JSON.
            try
            {
                using var doc = JsonDocument.Parse(line);
                var root = doc.RootElement;
                var (bg, bc) = ParseTeam(root, "blue");
                var (rg, rc) = ParseTeam(root, "red");
                var (bt, btc) = ParseTowerTeam(root, "blue");
                var (rt, rtc) = ParseTowerTeam(root, "red");
                var (cs, csc) = ParseCs(root);
                var (bo, boc, ro, roc) = ParseObjectives(root);
                return new Snapshot
                {
                    BlueGold = bg, BlueConf = bc, RedGold = rg, RedConf = rc,
                    BlueTowers = bt, BlueTowerConf = btc, RedTowers = rt, RedTowerConf = rtc,
                    Cs = cs, CsConf = csc,
                    BlueObj = bo, BlueObjConf = boc, RedObj = ro, RedObjConf = roc,
                    RecvMs = NowMs()
                };
            }
            catch { return null; }
        }

        private static (float?, GoldConfidence) ParseTeam(JsonElement root, string team)
        {
            if (!root.TryGetProperty(team, out var t))
                return (null, GoldConfidence.Unknown);
            string state = t.TryGetProperty("state", out var st) ? st.GetString() : "Unknown";
            float? val = null;
            if (t.TryGetProperty("value", out var v) && v.ValueKind == JsonValueKind.Number)
                val = (float)v.GetDouble();
            GoldConfidence conf = state switch
            {
                "Known" => GoldConfidence.Exact,
                "Stale" => GoldConfidence.Stale,
                _ => GoldConfidence.Unknown,
            };
            return (val, conf);
        }

        private static (int?, GoldConfidence) ParseTowerTeam(JsonElement root, string team)
        {
            // {"towers": {"blue": {state, value}, "red": {...}}}
            if (!root.TryGetProperty("towers", out var tw) || tw.ValueKind != JsonValueKind.Object
                || !tw.TryGetProperty(team, out var t))
                return (null, GoldConfidence.Unknown);
            string state = t.TryGetProperty("state", out var st) ? st.GetString() : "Unknown";
            int? val = null;
            // TryGetInt32 (not GetInt32) so a malformed/float/out-of-range number can't throw and
            // discard the line's valid gold along with it.
            if (t.TryGetProperty("value", out var v) && v.ValueKind == JsonValueKind.Number && v.TryGetInt32(out var iv))
                val = iv;
            GoldConfidence conf = state switch
            {
                "Known" => GoldConfidence.Exact,
                "Stale" => GoldConfidence.Stale,
                _ => GoldConfidence.Unknown,
            };
            return (val, conf);
        }

        private static (int?[], GoldConfidence[]) ParseCs(JsonElement root)
        {
            var vals = new int?[10];
            var confs = new GoldConfidence[10];
            for (int i = 0; i < 10; i++) confs[i] = GoldConfidence.Unknown;
            if (root.TryGetProperty("cs", out var arr) && arr.ValueKind == JsonValueKind.Array)
            {
                int i = 0;
                foreach (var cell in arr.EnumerateArray())
                {
                    if (i >= 10) break;
                    string state = cell.TryGetProperty("state", out var st) ? st.GetString() : "Unknown";
                    if (cell.TryGetProperty("value", out var v) && v.ValueKind == JsonValueKind.Number && v.TryGetInt32(out var iv))
                        vals[i] = iv;
                    confs[i] = state switch
                    {
                        "Known" => GoldConfidence.Exact,
                        "Stale" => GoldConfidence.Stale,
                        _ => GoldConfidence.Unknown,
                    };
                    i++;
                }
            }
            return (vals, confs);
        }

        // Objective order in the sidecar "obj" object and our int?[4] arrays.
        private static readonly string[] _objKeys = { "grub", "baron", "dragon", "tower" };

        private static (int?[], GoldConfidence[], int?[], GoldConfidence[]) ParseObjectives(JsonElement root)
        {
            var bv = new int?[4]; var bc = new GoldConfidence[4];
            var rv = new int?[4]; var rc = new GoldConfidence[4];
            for (int i = 0; i < 4; i++) { bc[i] = GoldConfidence.Unknown; rc[i] = GoldConfidence.Unknown; }
            // {"obj": {"blue": {grub:{state,value}, baron:{...}, dragon, tower}, "red": {...}}}
            if (root.TryGetProperty("obj", out var obj) && obj.ValueKind == JsonValueKind.Object)
            {
                ParseObjTeam(obj, "blue", bv, bc);
                ParseObjTeam(obj, "red", rv, rc);
            }
            return (bv, bc, rv, rc);
        }

        private static void ParseObjTeam(JsonElement obj, string team, int?[] v, GoldConfidence[] c)
        {
            if (!obj.TryGetProperty(team, out var t) || t.ValueKind != JsonValueKind.Object) return;
            for (int i = 0; i < _objKeys.Length; i++)
            {
                if (!t.TryGetProperty(_objKeys[i], out var cell)) continue;
                string state = cell.TryGetProperty("state", out var st) ? st.GetString() : "Unknown";
                if (cell.TryGetProperty("value", out var val) && val.ValueKind == JsonValueKind.Number && val.TryGetInt32(out var iv))
                    v[i] = iv;
                c[i] = state switch { "Known" => GoldConfidence.Exact, "Stale" => GoldConfidence.Stale, _ => GoldConfidence.Unknown };
            }
        }

        /// <summary>Inject the latest gated gold into the teams (call once per tick after UpdateTeams).</summary>
        public static void ApplyTo(Team blue, Team red)
        {
            var s = _latest;
            bool fresh = (NowMs() - s.RecvMs) <= FreshMs;
            Apply(blue, fresh ? s.BlueGold : null, fresh ? s.BlueConf : GoldConfidence.Unknown);
            Apply(red, fresh ? s.RedGold : null, fresh ? s.RedConf : GoldConfidence.Unknown);
        }

        private static void Apply(Team t, float? gold, GoldConfidence conf)
        {
            if (t == null) return;
            if (gold.HasValue && (conf == GoldConfidence.Exact || conf == GoldConfidence.Stale))
            {
                t.ExternalGold = gold;
                t.Confidence = conf;
            }
            else
            {
                // Strict-hide: no trustworthy OCR value -> don't inject (GetGold falls back to the
                // estimate as a default return), and mark Unknown so the frontend hides the value.
                t.ExternalGold = null;
                t.Confidence = GoldConfidence.Unknown;
            }
        }

        /// <summary>
        /// Overwrite each team's tower-takedown count with the OCR'd HUD value. Call once per tick
        /// (after UpdateTeams, before UpdateScoreboard) ONLY in Replay mode — there /eventdata is
        /// sparse after a seek, so the event-counted Team.towers is unreliable and the HUD digit is
        /// authoritative. If a tower read is not trustworthy (Unknown/stale-by-age) we leave the
        /// event-counted value untouched as a fallback rather than zeroing a real count.
        /// </summary>
        // Last trustworthy OCR'd tower count per team, held across Unknown gaps. The event-counted
        // Team.towers is monotonic and is NOT rolled back on a backward seek (the rewind branch
        // trims dragons but not towers), so reverting to it after a rewind would over-count. Once
        // OCR has locked a count we keep showing it through brief OCR drops instead.
        private static int? _lastTowerBlue, _lastTowerRed;

        public static void ApplyTowers(Team blue, Team red)
        {
            var s = _latest;
            bool fresh = (NowMs() - s.RecvMs) <= FreshMs;
            _lastTowerBlue = ApplyTower(blue, fresh ? s.BlueTowers : null, fresh ? s.BlueTowerConf : GoldConfidence.Unknown, _lastTowerBlue);
            _lastTowerRed = ApplyTower(red, fresh ? s.RedTowers : null, fresh ? s.RedTowerConf : GoldConfidence.Unknown, _lastTowerRed);
        }

        /// <summary>Apply one team's OCR tower count; returns the value to remember as last-good.</summary>
        private static int? ApplyTower(Team t, int? towers, GoldConfidence conf, int? lastOcr)
        {
            if (t == null) return lastOcr;
            if (towers.HasValue && (conf == GoldConfidence.Exact || conf == GoldConfidence.Stale))
            {
                t.towers = towers.Value;
                return towers;                 // new last-good OCR value
            }
            // OCR not trustworthy this tick: hold the last OCR'd count if we have one, rather than
            // leaving the monotonic event count (wrong after a backward seek). Before the first lock
            // (lastOcr == null) the event count stands — unavoidable, and only briefly at start.
            if (lastOcr.HasValue)
                t.towers = lastOcr.Value;
            return lastOcr;
        }

        // Accept an OCR'd CS only when it rounds to the API's value (which is authoritative but
        // floored to 10 in spectator/replay). This single guard rejects misreads, a wrong row->
        // player mapping, and panel-closed garbage — all of them fail the bound — and guarantees
        // the displayed CS never contradicts the API. The window also tolerates the small timing
        // skew between the OCR frame and the /playerlist poll.
        private const int CS_TOLERANCE = 15;

        /// <summary>
        /// Overwrite each player's creepScore with the OCR'd detail-scoreboard CS (exact), where
        /// the API only gives multiples of 10. Call once per tick AFTER UpdateTeams (which sets
        /// creepScore from /playerlist) and BEFORE UpdateScoreboard / the gold sample, so the finer
        /// CS flows into the scoreboard, the CS/min tab, and the gold estimate. Per-cell gated:
        /// an OCR value is taken only if trustworthy AND within CS_TOLERANCE of the API value;
        /// otherwise the API value stands (never worse than the API).
        /// </summary>
        public static void ApplyCs(Team blue, Team red)
        {
            var s = _latest;
            var dbg = new CsDebugCell[10];
            bool fresh = (NowMs() - s.RecvMs) <= FreshMs;
            if (s.Cs != null && fresh)
            {
                ApplyCsTeam(blue, s, 0, dbg);   // cs[0..4] -> blue players by position
                ApplyCsTeam(red, s, 5, dbg);    // cs[5..9] -> red players by position
            }
            else
            {
                // Not applying this tick (stale IPC or no CS array). Still surface the sidecar's
                // per-cell read + state so /debug-ocr shows WHY nothing was taken.
                for (int i = 0; i < 10; i++)
                    dbg[i] = new CsDebugCell
                    {
                        idx = i,
                        ocr = s.Cs?[i],
                        state = ConfName(s.CsConf != null ? s.CsConf[i] : GoldConfidence.Unknown),
                        api = -1, accepted = false, final = -1
                    };
            }
            _csDbg = dbg;
            Interlocked.Exchange(ref _csApplyMs, NowMs());
        }

        private static void ApplyCsTeam(Team t, Snapshot s, int baseIdx, CsDebugCell[] dbg)
        {
            for (int i = 0; i < 5; i++)
            {
                int idx = baseIdx + i;
                int? ocr = s.Cs[idx];
                GoldConfidence conf = s.CsConf[idx];
                var cell = new CsDebugCell { idx = idx, ocr = ocr, state = ConfName(conf), api = -1, accepted = false, final = -1 };
                var p = (t?.players != null && i < t.players.Count) ? t.players[i] : null;
                if (p?.scores != null)
                {
                    int api = p.scores.creepScore;
                    cell.api = api;
                    cell.final = api;
                    if (ocr.HasValue && (conf == GoldConfidence.Exact || conf == GoldConfidence.Stale)
                        && System.Math.Abs(ocr.Value - api) <= CS_TOLERANCE)
                    {
                        p.scores.creepScore = ocr.Value;
                        cell.accepted = true;
                        cell.final = ocr.Value;
                    }
                }
                dbg[idx] = cell;
            }
        }

        // Last trustworthy OCR'd objective count per team, held across Unknown gaps (same reasoning
        // as towers: the event count is 0/wrong in spectator, so hold the OCR value rather than
        // revert). Index 0=grub,1=baron,2=dragon,3=tower.
        private static readonly int?[] _lastObjBlue = new int?[4];
        private static readonly int?[] _lastObjRed = new int?[4];

        /// <summary>
        /// Inject the OCR'd top-bar objective counts (grub/baron/dragon/tower) into the teams. Call
        /// once per tick after UpdateTeams, before UpdateScoreboard. Objective-monster kills are NOT
        /// in the spectator/replay /eventdata, so this HUD OCR is the only count source. Per-counter:
        /// take the gated value when trustworthy, else hold the last OCR'd count. Sets Team.OcrGrubs/
        /// OcrBaron/OcrDragons/OcrTowers; UpdateScoreboard prefers them (OcrX ?? eventValue).
        /// </summary>
        public static void ApplyObjectives(Team blue, Team red)
        {
            var s = _latest;
            bool fresh = (NowMs() - s.RecvMs) <= FreshMs;
            ApplyObjTeam(blue, fresh ? s.BlueObj : null, fresh ? s.BlueObjConf : null, _lastObjBlue);
            ApplyObjTeam(red, fresh ? s.RedObj : null, fresh ? s.RedObjConf : null, _lastObjRed);
        }

        private static void ApplyObjTeam(Team t, int?[] vals, GoldConfidence[] confs, int?[] last)
        {
            if (t == null) return;
            for (int i = 0; i < 4; i++)
            {
                int? v = (vals != null && confs != null && vals[i].HasValue
                          && (confs[i] == GoldConfidence.Exact || confs[i] == GoldConfidence.Stale))
                         ? vals[i] : null;
                if (v.HasValue) last[i] = v;     // new last-good
                int? use = v ?? last[i];          // hold last-good across Unknown gaps
                if (!use.HasValue) continue;      // no OCR ever -> leave null (UpdateScoreboard falls back)
                switch (i)
                {
                    case 0: t.OcrGrubs = use; break;
                    case 1: t.OcrBaron = use; break;
                    case 2: t.OcrDragons = use; break;
                    case 3: t.OcrTowers = use; break;
                }
            }
        }

        /// <summary>Reset teams to the legacy estimate path (used when the feature is toggled off).</summary>
        public static void ClearInjection(Team blue, Team red)
        {
            foreach (var t in new[] { blue, red })
            {
                if (t == null) continue;
                t.ExternalGold = null;
                t.Confidence = GoldConfidence.Estimated;
                t.OcrGrubs = t.OcrBaron = t.OcrDragons = t.OcrTowers = null;
            }
            _lastTowerBlue = _lastTowerRed = null;
            System.Array.Clear(_lastObjBlue, 0, 4);
            System.Array.Clear(_lastObjRed, 0, 4);
        }

        private static string ConfName(GoldConfidence c) =>
            c == GoldConfidence.Exact ? "Known" : c == GoldConfidence.Stale ? "Stale" : "Unknown";

        /// <summary>
        /// Live JSON snapshot for the /debug-ocr inspector: the sidecar's raw read + gate state for
        /// every metric, the C# freshness verdict, and the per-CS accept/fallback decision (incl. the
        /// API floor it was compared against). Read-only; safe to call from the HTTP thread.
        /// </summary>
        public static string GetDebugJson()
        {
            var s = _latest;
            long now = NowMs();
            bool ever = s.RecvMs > 0;
            long age = ever ? now - s.RecvMs : -1;
            var payload = new
            {
                running = _running,
                ageMs = age,
                fresh = ever && age <= FreshMs,
                freshMs = FreshMs,
                rawLine = _lastRawLine,
                stderr = _lastStderr,
                gold = new
                {
                    blue = new { value = s.BlueGold, state = ConfName(s.BlueConf) },
                    red = new { value = s.RedGold, state = ConfName(s.RedConf) },
                },
                towers = new
                {
                    blue = new { value = s.BlueTowers, state = ConfName(s.BlueTowerConf), held = _lastTowerBlue },
                    red = new { value = s.RedTowers, state = ConfName(s.RedTowerConf), held = _lastTowerRed },
                },
                obj = new
                {
                    blue = ObjDbg(s.BlueObj, s.BlueObjConf, _lastObjBlue),
                    red = ObjDbg(s.RedObj, s.RedObjConf, _lastObjRed),
                },
                cs = _csDbg,
                csApplyAgeMs = now - Interlocked.Read(ref _csApplyMs),
            };
            return JsonSerializer.Serialize(payload);
        }

        private static object[] ObjDbg(int?[] v, GoldConfidence[] c, int?[] held)
        {
            var arr = new object[4];
            for (int i = 0; i < 4; i++)
                arr[i] = new
                {
                    key = _objKeys[i],
                    value = v != null ? v[i] : null,
                    state = ConfName(c != null ? c[i] : GoldConfidence.Unknown),
                    held = held[i],
                };
            return arr;
        }

        private static long NowMs() => Environment.TickCount64;
    }
}
