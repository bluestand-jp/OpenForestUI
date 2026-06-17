> 🌐 [English](README.md) ・ **日本語**

# チャンピオンセレクトのデータモデル

チャンピオンセレクトパイプラインのデータ層。3 つの関心事を分離する。League Client (LCU) API からデシリアライズされる**生の形状**、オーバーレイへ送られる**正規化された形状**、そして pickban オーバーレイ向けの**永続化された設定**である。

`StateInfo/Converter` が `LCU/` 型を読み取り `DTO/` 型を生成する。結果として得られる `DTO/Team` と `Config/PickBanConfig` が、オーバーレイが実際に WebSocket 経由で受け取るものである。

## サブディレクトリ

| Directory | 目的 |
| --- | --- |
| [`LCU/`](LCU/README-ja.md) | 生の League Client champ-select DTO (LCU JSON からデシリアライズ) |
| [`DTO/`](DTO/README-ja.md) | オーバーレイへ送る正規化された pick/ban/team/version モデル |
| [`Config/`](Config/README-ja.md) | 永続化された pickban オーバーレイ config (teams、scores、toggles、ブロードキャストメタデータ) |
