> 🌐 [English](README.md) ・ **日本語**

# ブラウザソース配信オーバーレイ（フロントエンド）

OpenForestUI デスクトップアプリが駆動し、放送クライアント（OBS のブラウザソース / vMix）へ配信する 2 つの Web オーバーレイ。いずれもプレーンな TypeScript Web アプリであり、アプリは組み込みサーバー越しにこれらをホストする — HTTP は `/frontend`、WebSocket 制御チャンネルは `ws://localhost:9001/api` — そのソケット越しにゲーム状態と `OverlayConfig` をプッシュする。各オーバーレイは放送の対応するステージを描画する。

## サブディレクトリ

| ディレクトリ | 役割 |
| --- | --- |
| [`ingame/`](ingame/README-ja.md) | インゲームオーバーレイ（Phaser 3 + rexUI、parcel バンドル）: スコアボード / トップバー、オブジェクトタイマー、ゴールドグラフ、レベルアップ・アイテムポップアップ、インヒビタータイマー、情報サイドページ。 |
| [`pickban/`](pickban/README-ja.md) | チャンピオンセレクト（ピック & バン）オーバーレイ — RCVolus `lol-pick-ban-ui` の C# 統合。 |

いずれも `dist/` フォルダへバンドルされ、デスクトップアプリが配信する。ビルド / 実行の詳細は各オーバーレイの readme を参照。
