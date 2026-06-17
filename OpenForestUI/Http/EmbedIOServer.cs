using EmbedIO;
using EmbedIO.Actions;
using EmbedIO.Files;
using OpenForestUI.Common;
using OpenForestUI.Common.Controllers;
using OpenForestUI.OperatingSystem;
using Swan.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace OpenForestUI.Http
{
    class EmbedIOServer
    {
        private WebServer webServer;

        public static WSServer SocketServer;

        // JSON/HTML are sent without a BOM (RFC 8259: a JSON text MUST NOT be prefixed with a BOM,
        // and Encoding.UTF8 emits one). new UTF8Encoding(false) = no preamble.
        private static readonly Encoding Utf8NoBom = new UTF8Encoding(false);

        // Self-contained inspector served at /debug-ocr. Polls /debug-ocr/data every 300ms and shows the
        // sidecar's raw OCR read + gate state per metric, the freshness verdict, and the per-player CS
        // accept-vs-API-floor decision. Single-quoted throughout so it lives in a C# verbatim string.
        private const string DebugOcrPage = @"<!doctype html><html><head><meta charset='utf-8'><title>OCR Debug</title>
<style>
body{background:#111;color:#ddd;font:13px/1.45 Consolas,monospace;margin:14px}
h1{font-size:15px;color:#cde}
h2{margin:16px 0 4px;color:#8cf;font-size:12px;text-transform:uppercase;letter-spacing:.5px}
table{border-collapse:collapse;margin-bottom:6px}
td,th{border:1px solid #333;padding:2px 9px;text-align:right}
th{color:#9ab;background:#1a1a1a}
.k{color:#5e5;font-weight:bold}.s{color:#fd5}.u{color:#f66}
.ok{color:#5e5;font-weight:bold}.no{color:#f66}
.dim{color:#777;word-break:break-all}
#hdr span{margin-right:22px}
</style></head><body>
<h1>OCR pipeline &mdash; live inspector</h1>
<div id='hdr'></div><div id='body'></div>
<script>
function E(i){return document.getElementById(i)}
function cls(st){return st=='Known'?'k':st=='Stale'?'s':'u'}
function fmt(v){return v==null?'<span class=dim>--</span>':v}
async function tick(){
 var d;
 try{d=await (await fetch('/debug-ocr/data',{cache:'no-store'})).json()}
 catch(e){E('hdr').innerHTML='<span class=no>fetch failed: '+e+'</span>';return}
 var h='';
 h+='<span>sidecar: '+(d.running?'<b class=ok>running</b>':'<b class=no>STOPPED</b>')+'</span>';
 h+='<span>IPC age: '+(d.ageMs<0?'<b class=no>never received</b>':(d.ageMs+'ms '+(d.fresh?'<b class=ok>fresh</b>':'<b class=no>STALE (&gt;'+d.freshMs+'ms)</b>')))+'</span>';
 h+='<span>CS-apply age: '+(d.csApplyAgeMs>4000?'<b class=no>'+d.csApplyAgeMs+'ms &mdash; not applying</b>':'<b class=ok>'+d.csApplyAgeMs+'ms</b>')+'</span>';
 E('hdr').innerHTML=h;
 var b='';
 b+='<h2>Gold / Towers</h2><table><tr><th>team</th><th>gold OCR</th><th>state</th><th>towers OCR</th><th>state</th><th>held</th></tr>';
 ['blue','red'].forEach(function(t){var g=d.gold[t],w=d.towers[t];
  b+='<tr><td>'+t+'</td><td>'+fmt(g.value)+'</td><td class='+cls(g.state)+'>'+g.state+'</td><td>'+fmt(w.value)+'</td><td class='+cls(w.state)+'>'+w.state+'</td><td>'+fmt(w.held)+'</td></tr>'});
 b+='</table>';
 b+='<h2>Objectives (grub / baron / dragon / tower)</h2><table><tr><th>team</th><th>counter</th><th>OCR</th><th>state</th><th>held</th></tr>';
 ['blue','red'].forEach(function(t){d.obj[t].forEach(function(o){
  b+='<tr><td>'+t+'</td><td>'+o.key+'</td><td>'+fmt(o.value)+'</td><td class='+cls(o.state)+'>'+o.state+'</td><td>'+fmt(o.held)+'</td></tr>'})});
 b+='</table>';
 b+='<h2>CS per player &mdash; B0..B4 blue, R0..R4 red (== /playerlist order)</h2><table><tr><th>cell</th><th>OCR</th><th>state</th><th>API floor</th><th>|diff|</th><th>within 15?</th><th>FINAL shown</th></tr>';
 var nm=['B0','B1','B2','B3','B4','R0','R1','R2','R3','R4'];
 d.cs.forEach(function(c){var dl=(c.ocr!=null&&c.api>=0)?Math.abs(c.ocr-c.api):null;
  b+='<tr><td>'+nm[c.idx]+'</td><td>'+fmt(c.ocr)+'</td><td class='+cls(c.state)+'>'+c.state+'</td><td>'+(c.api<0?'<span class=dim>--</span>':c.api)+'</td><td>'+fmt(dl)+'</td><td class='+(c.accepted?'ok':'no')+'>'+(c.accepted?'YES':'no')+'</td><td><b>'+(c.final<0?'--':c.final)+'</b></td></tr>'});
 b+='</table>';
 b+='<h2>raw sidecar stdout (last JSON line)</h2><div class=dim>'+(d.rawLine||'(none)')+'</div>';
 b+='<h2>sidecar stderr (last line)</h2><div class=dim>'+(d.stderr||'(none)')+'</div>';
 E('body').innerHTML=b;
}
setInterval(tick,300);tick();
</script></body></html>";

        public EmbedIOServer(string location, int port)
        {
            var uri = $"http://{location}:{port}/";

            webServer = CreateWebServer(uri);

            webServer.RunAsync();
            Log.Info($"WebServer running on {uri}");
        }

        public void Restart()
        {
            throw new NotImplementedException();
        }

        public void Stop()
        {
            webServer.Dispose();
            Log.Info($"WebServer stopped");
        }

        private static WebServer CreateWebServer(string url)
        {
            var webRoot = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "cache");
            var ingameDir = $"{Directory.GetCurrentDirectory()}\\Frontend\\ingame";
            Log.Info($"Server file system starting");
            var server = new WebServer(o => o
                    .WithUrlPrefix(url)
                    .WithMode(HttpListenerMode.EmbedIO))
                // Add modules
                .WithLocalSessionManager()
                .WithCors()
                .WithModule(SocketServer = new("/api"))
                .WithModule(new FileModule("/cache",
                    new FileSystemProvider(webRoot, false))
                {
                    DirectoryLister = DirectoryLister.Html
                })
                // /debug-ocr : live OCR pipeline inspector. /data = JSON snapshot, page = auto-polling HTML.
                // Registered before the "/" catch-all static folder so it isn't shadowed (longer /data
                // route first so it matches before the page route).
                .WithModule(new EmbedIO.Actions.ActionModule("/debug-ocr/data", HttpVerbs.Get, async ctx =>
                    await ctx.SendStringAsync(OcrGoldController.GetDebugJson(), "application/json", Utf8NoBom)))
                .WithModule(new EmbedIO.Actions.ActionModule("/debug-ocr", HttpVerbs.Get, async ctx =>
                    await ctx.SendStringAsync(DebugOcrPage, "text/html", Utf8NoBom)))
                // Static files last to avoid conflicts. A no-store module sits in front of the ingame
                // overlay and serves its HTML entry itself, so OBS always refetches index.html (and thus
                // the latest main.<hash>.js) after a redeploy. Hashed JS/CSS/images fall through to the
                // static FileModule and keep normal caching.
                .WithModule(new IngameIndexNoStoreModule("/frontend", ingameDir))
                .WithStaticFolder("/frontend", ingameDir, true, m => m
                    .WithContentCaching(true))
                .WithStaticFolder("/", $"{Directory.GetCurrentDirectory()}\\Frontend\\pickban", true, m => m
                    .WithContentCaching(true))
                ;

            // Listen for state changes.
            server.StateChanged += (s, e) => Log.Info($"WebServer New State - {e.NewState}");

            return server;
        }

        /// <summary>
        /// Serves the ingame overlay's HTML entry (index.html / directory index) itself, with a
        /// no-store policy, so OBS's embedded browser always refetches it — and thus the latest
        /// main.&lt;hash&gt;.js — after a redeploy, with no manual cache purge. EmbedIO's FileModule would
        /// otherwise force "max-age=0, must-revalidate", which CEF doesn't reliably honor on the main frame.
        ///
        /// Non-HTML requests (hashed JS/CSS/images) pass through (IsFinalHandler = false, no SetHandled)
        /// to the static FileModule, which keeps normal caching for those immutable, content-hashed files.
        /// </summary>
        private sealed class IngameIndexNoStoreModule : WebModuleBase
        {
            private readonly string _dir;

            public IngameIndexNoStoreModule(string baseRoute, string dir) : base(baseRoute) { _dir = dir; }

            public override bool IsFinalHandler => false;

            protected override async Task OnRequestAsync(IHttpContext context)
            {
                var p = context.RequestedPath;   // relative to baseRoute: "/", "/index.html", "/main.x.js", ...
                var isIndex = string.IsNullOrEmpty(p) || p == "/" || p.EndsWith(".html", StringComparison.OrdinalIgnoreCase);
                if (!isIndex)
                    return;                       // pass through -> static FileModule serves it

                var rel = (string.IsNullOrEmpty(p) || p == "/") ? "index.html" : p.TrimStart('/');
                var file = Path.Combine(_dir, rel.Replace('/', Path.DirectorySeparatorChar));
                if (!File.Exists(file))
                    return;                       // let the FileModule produce a normal 404

                context.Response.Headers.Set("Cache-Control", "no-store, no-cache, must-revalidate");
                context.Response.Headers.Set("Pragma", "no-cache");
                await context.SendStringAsync(File.ReadAllText(file, Encoding.UTF8), "text/html", Utf8NoBom);
                context.SetHandled();
            }
        }
    }
}
