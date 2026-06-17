> 🌐 [English](README.md) ・ **日本語**

# アプリコア (controllers, config, data, overlay events)

OpenForestUI デスクトップアプリの中核。パイプライン全体を統括する controllers、それらが参照する設定/データ層、そしてオーバーレイへ配信されるイベントの基底型を収める。ここで LCU、観戦用 Live Client Data API、Replay API、Farsight メモリリーダー、Python OCR サイドカーからのライブゲームデータを取り込み、オーバーレイ状態へ変換し、組み込みの WebSocket/HTTP サーバー経由でブラウザソースへ送出する。

(注: これはアプリ自身の `Common` 名前空間であり、共有 HTTP/DTO/ユーティリティコードを収める独立した `OpenForestUI.Common` ライブラリプロジェクトとは別物。)

## サブディレクトリ

| ディレクトリ | 目的 |
|-----------|---------|
| [`Controllers/`](Controllers/README-ja.md) | アプリのライフサイクルとデータ取り込みを駆動するシングルトン群: `BroadcastController`、`AppStateController`、`IngameController`、`PickBanController`、`OcrGoldController`、`MockController` ほか。 |
| [`Data/`](Data/README-ja.md) | 設定モデル + ローダー (`Config/`) と Data Dragon 静的アセットプロバイダー (`Provider/`)。 |
| [`Events/`](Events/README-ja.md) | オーバーレイへ配信される型付きメッセージの基底型 (`LeagueEvent`、`OverlayConfig`)。 |
