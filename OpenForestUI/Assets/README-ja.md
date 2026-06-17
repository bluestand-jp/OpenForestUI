> 🌐 [English](README.md) ・ **日本語**

# アプリのブランディングアセット

OpenForestUI デスクトップアプリ（`OpenForestUI.exe`）向けの静的ブランディングアセット。ウィンドウ／実行ファイルのアイコン、アプリ内ロゴと小さな UI 画像、ブランドフォントが含まれる。これらは WPF XAML（pack URI ／相対パス経由）と `OpenForestUI.csproj` から参照され、csproj はそのうちいくつかをビルド時に出力ディレクトリへコピーする。

> 注: これらは **WPF アプリ**のアセットである。オーバーレイ（ブラウザソース）のアートワークは別途 `Overlays/*/public/images/` 配下に置かれる。

## サブディレクトリ

| Directory | Purpose |
| --- | --- |
| [`Fonts/`](Fonts/README-ja.md) | ワードマーク用に埋め込まれた Venus Rising ブランドフォント |
| [`Icons/`](Icons/README-ja.md) | `.ico` のウィンドウ／実行ファイルアイコン |
| [`Images/`](Images/README-ja.md) | アプリ内ロゴと小さな UI ビットマップ（PNG） |
