> 🌐 [English](README.md) ・ **日本語**

# OpenForestUI.Common — 共有低レベルライブラリ

OpenForestUI デスクトップアプリ、Farsight メモリリーダー、オーバーレイバックエンドで横断的に共有されるコード群。ファイルログ、HTTP/REST ヘルパー、JSON コンバーター、小さなユーティリティ拡張、そしてチャンピオン・サモナースペル・アイテム・ゲームの XP→レベル換算表を表現するデータモデルを提供する。プロジェクト依存を持たない `net6.0` クラスライブラリ（`OpenForestUI.Common.csproj`）であり、ソリューション内の他のすべてがこれを参照する。

## 構成内容

- `Log.cs` — プロセス全体のシングルトンロガー。メッセージを `StringBuilder` にバッファし、5 秒ごとおよび `ProcessExit` 時に `Logs/Log-<timestamp>.log` へフラッシュする。`LogLevel` enum でゲートされた静的な `Info`/`Warn`/`Verbose`/`Write` を公開し、未処理例外時には `CrashLogs/` レポート（システム詳細とランダム化されたフレーバー行付き）を書き出す。
- `OpenForestUI.Common.csproj` — ライブラリプロジェクト。`Newtonsoft.Json`、`System.Text.Json`、`Microsoft.Win32.Registry` を参照する。

## サブディレクトリ

| ディレクトリ | 目的 |
| --- | --- |
| [`Data/`](Data/README-ja.md) | データモデル: アプリ向け DTO と Riot/Community Dragon の静的データ型 |
| [`Http/`](Http/README-ja.md) | HTTP/REST ヘルパー — REST クライアント、ファイルダウンローダー、テキスト取得 |
| [`Utils/`](Utils/README-ja.md) | 汎用拡張メソッド、JSON コンバーター、循環バッファ、バージョン解析 |
