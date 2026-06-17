> 🌐 [English](README.md) ・ **日本語**

# Ingame データモデル

ingame オーバーレイ向けの全データ型を、パイプライン上でデータが置かれる位置に従って整理したもの。生の Riot DTO はローカルの HTTP provider から流入し、ブロードキャストモデルへ集約され、オーバーレイへ送出する JSON ペイロードへと再形成される。オーバーレイおよびメモリリーダー向けの設定モデルもここに置く。

## サブディレクトリ

| Directory | 役割 |
| --- | --- |
| [`Provider/`](Provider/README-ja.md) | ローカル Riot HTTP クライアント（Live Client Data + Replay API）とそのイベント引数。 |
| [`RIOT/`](RIOT/README-ja.md) | 観戦用 Live Client Data API JSON をミラーする DTO。 |
| [`Replay/`](Replay/README-ja.md) | クライアント内蔵 Replay API（`/replay/*`）向け DTO — 再生クロック、カメラ、HUD トグル。 |
| [`Hub/`](Hub/README-ja.md) | 集約されたゲーム状態モデル（teams、players、gold、inhibitors、objectives）。 |
| [`Frontend/`](Frontend/README-ja.md) | WebSocket 経由でオーバーレイへシリアライズされるブロードキャスト ペイロード形状。 |
| [`Config/`](Config/README-ja.md) | ingame オーバーレイ（`IngameConfig`）とメモリリーダー offset（`FarsightConfig`）の永続化 JSON config。 |
