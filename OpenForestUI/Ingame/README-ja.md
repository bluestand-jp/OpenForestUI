> 🌐 [English](README.md) ・ **日本語**

# Ingame オーバーレイ バックエンド

**ingame** ブラウザソース オーバーレイを駆動する C# サブシステム。各 tick でローカルの Riot エンドポイント（port 2999 の観戦用 Live Client Data API、Replay API、そして有効時は Farsight メモリリーダーと OCR サイドカー）からデータを取得し、集約されたゲーム状態へ畳み込み、オブジェクトのスポーンタイマーを導出し、フロントエンドのペイロードへシリアライズして WebSocket 経由で Phaser オーバーレイへ送出する。

## サブディレクトリ

| Directory | 役割 |
| --- | --- |
| [`Data/`](Data/README-ja.md) | ingame の全データモデル — provider、Riot/Replay DTO、集約 hub モデル、フロントエンド ペイロード、config。 |
| [`Events/`](Events/README-ja.md) | `RiotEvent` と型付き `/eventdata` イベント DTO（kill、オブジェクト撃破、item-completed、level-up など）、および `IngameOverlay` の config-overlay 登録。 |
| [`State/`](State/README-ja.md) | tick ごとのステートマシン。`State`（provider を取り込み → hub モデルを更新 → イベントを emit）、`StateData`（シリアライズされたフロントエンドのスナップショット）、`ObjectiveSpawnClock`（kill イベント + パッチタイミングから Dragon/Baron/Herald のカウントダウンを導出）。 |
