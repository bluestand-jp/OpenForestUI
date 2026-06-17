> 🌐 [English](README.md) ・ **日本語**

# オーバーレイアイコンとテーマアート

汎用オーバーレイアイコンに加え、ドラゴン、レーン、放送用スキン、スコアボードポップアップ動画のテーマ別サブツリー。`frontend/images/...` として読み込まれ、大半は [`../../src/scenes/IngameScene.ts`](../../src/scenes/IngameScene.ts) でプリロードされる。

## 内容

- `ObjectiveGold.png`、`ObjectiveCdr.png` — gold/CDR のオブジェクティブバーアイコン。
- `ScoreboardGold.png`、`ScoreboardCenterIcon.png` — スコアボードの gold アイコンと中央アイコン。
- `tower.png`、`baronTimer.png` — タワーおよびバロンタイマーのグリフ。
- `InfoTabSeparator.png` — インフォページのタブセパレーター。

## サブディレクトリ

| Dir | 用途 |
| --- | --- |
| [`dragons/`](dragons/README-ja.md) | ドラゴンタイプ別アイコン（スコアボード + タイマーのバリアント） |
| [`lanes/`](lanes/README-ja.md) | レーングリフ SVG（top/mid/bot） |
| [`prm/`](prm/README-ja.md) | 放送用スキンのオブジェクティブアイコン |
| [`scoreboardPopUps/`](scoreboardPopUps/README-ja.md) | オブジェクティブの spawn/kill/soul ポップアップ動画と静止画 |
