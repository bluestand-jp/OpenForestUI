using System;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace OpenForestUI.Common.Controllers
{
    public enum OcrEnvStatus { NotProvisioned, Provisioning, Ready, Failed }

    /// <summary>
    /// Provisions the Python environment the OCR sidecar (ocr-poc/goldcap.py) runs in — mirroring
    /// the DataDragon first-run download. A tiny embeddable Python ships in ./python (next to the
    /// exe); the first time OCR is needed (or when the user clicks "Set up OCR now"), this
    /// pip-installs the heavy deps (easyocr -> PyTorch, opencv, dxcam, numpy, Pillow) ONCE, in the
    /// background, surfacing progress. While provisioning (or on failure) the app keeps using the
    /// estimated gold/CS fallback; once Ready, <see cref="OcrGoldController"/> launches the sidecar
    /// with the bundled interpreter (<see cref="InterpreterPath"/>).
    ///
    /// In a source checkout (no ./python bundle) there is nothing to provision: we mark Ready and
    /// fall back to the system "python" on PATH (or the OPENFORESTUI_PYTHON override), preserving
    /// the original developer behavior.
    /// </summary>
    public static class OcrEnvController
    {
        public static OcrEnvStatus Status { get; private set; } = OcrEnvStatus.NotProvisioned;
        public static string StatusText { get; private set; } = "Not set up";
        /// <summary>Raised (possibly off the UI thread) whenever Status/StatusText changes.</summary>
        public static event EventHandler StatusChanged;

        // ./python/python.exe next to the exe (resolved like OcrGoldController.ScriptPath: walk up so
        // both a published build and a source checkout work). null => no bundled interpreter.
        public static readonly string BundledPython = Resolve(Path.Combine("python", "python.exe"));
        private static readonly string RequirementsPath = Resolve("requirements.txt")
            ?? Path.Combine(AppContext.BaseDirectory, "requirements.txt");
        private static readonly string PyDir = BundledPython != null ? Path.GetDirectoryName(BundledPython) : null;
        private static readonly string GetPipPath = PyDir != null ? Path.Combine(PyDir, "get-pip.py") : null;
        // Readiness marker: ./python/.ocr-ready holding the requirements.txt hash it was built for.
        private static readonly string ReadyMarker = PyDir != null ? Path.Combine(PyDir, ".ocr-ready") : null;

        /// <summary>True when a bundled embeddable Python is present (then we manage provisioning).</summary>
        public static bool HasBundledPython => BundledPython != null;

        /// <summary>Interpreter the sidecar should use: bundled if present, else the dev fallback.</summary>
        public static string InterpreterPath =>
            BundledPython ?? (Environment.GetEnvironmentVariable("OPENFORESTUI_PYTHON") ?? "python");

        private static readonly object _gate = new();

        static OcrEnvController()
        {
            // Already provisioned for the current requirements? Then we're Ready with no work.
            if (MarkerMatches()) { Status = OcrEnvStatus.Ready; StatusText = "Ready"; }
        }

        /// <summary>
        /// Kick off provisioning if not already Ready/Provisioning. Idempotent + single-flight;
        /// returns immediately. Safe to call every tick (OcrGoldController.EnsureStarted does).
        /// </summary>
        public static void EnsureProvisioning() => Begin(force: false);

        /// <summary>User-triggered ("Set up OCR now"): (re)provision even after a previous failure.</summary>
        public static void Retry() => Begin(force: true);

        private static void Begin(bool force)
        {
            if (Status == OcrEnvStatus.Provisioning) return;
            if (!force && (Status == OcrEnvStatus.Ready || Status == OcrEnvStatus.Failed)) return;
            lock (_gate)
            {
                if (Status == OcrEnvStatus.Provisioning) return;
                if (!force && (Status == OcrEnvStatus.Ready || Status == OcrEnvStatus.Failed)) return;
                if (!HasBundledPython)
                {
                    // Source checkout / no bundle: nothing to install — use the system interpreter.
                    Set(OcrEnvStatus.Ready, "Using system Python");
                    return;
                }
                Set(OcrEnvStatus.Provisioning, "Preparing…");
            }
            Task.Run(Provision);
        }

        private static void Provision()
        {
            try
            {
                // 1. Bootstrap pip — the embeddable distribution ships without pip/ensurepip.
                if (!PipPresent())
                {
                    Set(OcrEnvStatus.Provisioning, "Bootstrapping pip…");
                    if (GetPipPath == null || !File.Exists(GetPipPath))
                    {
                        Set(OcrEnvStatus.Failed, "get-pip.py missing from bundle");
                        Log.Warn("OCR env: get-pip.py not found next to the bundled python");
                        return;
                    }
                    int rcPip = Run(BundledPython, $"\"{GetPipPath}\" --no-warn-script-location");
                    if (rcPip != 0 || !PipPresent())
                    {
                        Set(OcrEnvStatus.Failed, "pip bootstrap failed");
                        return;
                    }
                }

                // 2. Install the OCR deps (downloads torch etc. — large, one-time, cached in
                //    ./python/Lib/site-packages so a relaunch short-circuits via the marker).
                Set(OcrEnvStatus.Provisioning, "Downloading OCR dependencies… (one-time, large)");
                int rc = Run(BundledPython,
                    $"-m pip install --no-warn-script-location --disable-pip-version-check -r \"{RequirementsPath}\"");
                if (rc != 0)
                {
                    Set(OcrEnvStatus.Failed, "dependency install failed (see Logs)");
                    return;
                }

                WriteMarker();
                Set(OcrEnvStatus.Ready, "Ready");
                Log.Info("OCR Python environment provisioned");
            }
            catch (Exception ex)
            {
                Set(OcrEnvStatus.Failed, "setup error: " + ex.Message);
                Log.Warn($"OCR env provisioning failed: {ex.Message}");
            }
        }

        private static bool PipPresent()
        {
            try { return Run(BundledPython, "-m pip --version", quiet: true) == 0; }
            catch { return false; }
        }

        // Run a child process; stream its output so a long install surfaces progress and never
        // blocks on a full pipe buffer. Returns the exit code.
        private static int Run(string exe, string args, bool quiet = false)
        {
            var psi = new ProcessStartInfo
            {
                FileName = exe,
                Arguments = args,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            };
            if (PyDir != null) psi.WorkingDirectory = PyDir;
            using var p = new Process { StartInfo = psi };
            p.OutputDataReceived += (_, e) => { if (!quiet && e.Data != null) OnPipLine(e.Data); };
            p.ErrorDataReceived += (_, e) => { if (!quiet && e.Data != null) OnPipLine(e.Data); };
            p.Start();
            p.BeginOutputReadLine();
            p.BeginErrorReadLine();
            p.WaitForExit();
            return p.ExitCode;
        }

        // Surface the meaningful pip lines as the live status; log warnings/errors for diagnosis.
        private static void OnPipLine(string line)
        {
            string l = line.Trim();
            if (l.Length == 0) return;
            if (l.StartsWith("Collecting ") || l.StartsWith("Downloading ") || l.StartsWith("Installing ")
                || l.StartsWith("Building ") || l.StartsWith("Successfully "))
                Set(OcrEnvStatus.Provisioning, "OCR setup: " + Trunc(l, 64));
            if (l.IndexOf("error", StringComparison.OrdinalIgnoreCase) >= 0)
                Log.Warn($"[ocr-setup] {l}");
        }

        private static void Set(OcrEnvStatus s, string text)
        {
            Status = s;
            StatusText = text;
            try { StatusChanged?.Invoke(null, EventArgs.Empty); } catch { /* UI gone */ }
        }

        private static bool MarkerMatches()
        {
            try
            {
                return ReadyMarker != null && File.Exists(ReadyMarker)
                    && File.ReadAllText(ReadyMarker).Trim() == RequirementsHash();
            }
            catch { return false; }
        }

        private static void WriteMarker()
        {
            try { if (ReadyMarker != null) File.WriteAllText(ReadyMarker, RequirementsHash()); }
            catch (Exception ex) { Log.Warn($"OCR env: could not write ready marker: {ex.Message}"); }
        }

        private static string RequirementsHash()
        {
            try
            {
                if (RequirementsPath == null || !File.Exists(RequirementsPath)) return "no-req";
                using var sha = SHA256.Create();
                return Convert.ToHexString(sha.ComputeHash(File.ReadAllBytes(RequirementsPath)));
            }
            catch { return "err"; }
        }

        private static string Trunc(string s, int n) => s.Length <= n ? s : s.Substring(0, n) + "…";

        private static string Resolve(string relative)
        {
            var dir = AppContext.BaseDirectory;
            for (int i = 0; i < 6 && dir != null; i++)
            {
                var candidate = Path.Combine(dir, relative);
                if (File.Exists(candidate)) return candidate;
                dir = Directory.GetParent(dir)?.FullName;
            }
            return null;
        }
    }
}
