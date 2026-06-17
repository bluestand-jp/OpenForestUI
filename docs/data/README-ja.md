> 🌐 [English](README.md) ・ **日本語**

# アーカイブ済みゲームデータテーブル

アプリが参照する静的・バージョン管理されたデータテーブル — 現在はオプションの
Farsight メモリリーダーが使用するパッチごとのメモリオフセットアーカイブ。これは
ランタイム設定ではなくリファレンスデータである: アプリはパッチごとに単一のアクティブな
オフセットセットをロードし（`Config/Farsight.json`、`OpenForestUI/Offsets/Offsets-<patch>.json`
に由来）、ここにあるコピーはその方式のための歴史的カタログである。

## サブディレクトリ

| Directory | 目的 |
|---|---|
| [`offsets/`](offsets/README-ja.md) | パッチごとの Farsight メモリ読みオフセット（`Offsets-<patch>.json`）、パッチ 11.9 → 14.6 |
