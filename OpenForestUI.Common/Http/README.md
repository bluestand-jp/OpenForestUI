> 🌐 **English** ・ [日本語](README-ja.md)

# HTTP / REST helpers

Small HTTP utilities used across the app to fetch JSON (Riot/spectator APIs, Community Dragon) and download asset files. Several files are adapted from [GoldDiff](https://github.com/Johannes-Schneider/GoldDiff) (MIT) — see the source-header comments for provenance.

## Contents

- `RestRequester.cs` — Singleton REST client (`application/json`, 2 s default timeout, `OpenForestUI/2.0` user-agent). `GetAsync<T>(url)` deserializes a JSON response into `T` (Newtonsoft.Json); `GetRaw(url)` returns the raw body. Logs warnings on non-success status or timeout. *(Adapted from GoldDiff.)*
- `HttpUtils.cs` — `GetTextAsync(uri)` — lightweight `HttpWebRequest`-based GET with gzip/deflate decompression and a 2 s timeout; returns `""` on 404 or error instead of throwing.
- `FileDownloader.cs` — `DownloadAsync(remoteUrl, filePath, progress, …)` — downloads a file to disk (creating directories, overwriting existing), with optional progress reporting and a 5-minute timeout. *(Adapted from GoldDiff.)*
- `DownloadProcessEventArgument.cs` — `DownloadProgressEventArguments` value object (progress %, average MB/s, estimated remaining time) used for download progress events. *(From GoldDiff.)*
