> 🌐 [English](README.md) ・ **日本語**

# HTTP / REST ヘルパー

JSON の取得（Riot/spectator API、Community Dragon）やアセットファイルのダウンロードのためにアプリ全体で使われる小さな HTTP ユーティリティ。いくつかのファイルは [GoldDiff](https://github.com/Johannes-Schneider/GoldDiff)（MIT）から流用している — 出典についてはソースヘッダーのコメントを参照。

## 構成内容

- `RestRequester.cs` — シングルトンの REST クライアント（`application/json`、デフォルトタイムアウト 2 秒、`OpenForestUI/2.0` user-agent）。`GetAsync<T>(url)` は JSON レスポンスを `T` へデシリアライズする（Newtonsoft.Json）。`GetRaw(url)` は生のボディを返す。非成功ステータスやタイムアウト時に警告をログする。*（GoldDiff から流用。）*
- `HttpUtils.cs` — `GetTextAsync(uri)` — gzip/deflate 展開と 2 秒タイムアウトを備えた軽量な `HttpWebRequest` ベースの GET。404 やエラー時には例外を投げる代わりに `""` を返す。
- `FileDownloader.cs` — `DownloadAsync(remoteUrl, filePath, progress, …)` — ファイルをディスクへダウンロードする（ディレクトリを作成し既存を上書き）。任意の進捗報告と 5 分タイムアウト付き。*（GoldDiff から流用。）*
- `DownloadProcessEventArgument.cs` — ダウンロード進捗イベントで使う `DownloadProgressEventArguments` 値オブジェクト（進捗 %、平均 MB/s、推定残り時間）。*（GoldDiff より。）*
