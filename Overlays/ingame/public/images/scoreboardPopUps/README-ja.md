> 🌐 [English](README.md) ・ **日本語**

# オブジェクティブポップアップアート

主要な中立オブジェクティブの spawn / kill / soul ポップアップアート。`ObjectivePopUpVisual`（[`../../../src/visual/ObjectivePopUpVisual.ts`](../../../src/visual/ObjectivePopUpVisual.ts)）がスコアボード上のバナーとして表示する。各イベントは静止画 `.png` とループ `.mp4` の両方で提供され、ビジュアルは config が `UseVideo` を設定していれば動画を、そうでなければ画像を読み込む。消費側はパスを `frontend/images/scoreboardPopUps/<Baron|Herald|Dragon/<Type>>/<type><Event>.{png,mp4}` として組み立てる。

命名規則: `<type><Spawn|Kill|Soul>`（Baron/Herald に Soul バリアントはない）。

## サブディレクトリ

| Dir | 用途 |
| --- | --- |
| [`Baron/`](Baron/README-ja.md) | バロンの spawn/kill ポップアップ |
| [`Herald/`](Herald/README-ja.md) | リフトヘラルドの spawn/kill ポップアップ |
| [`Dragon/`](Dragon/README-ja.md) | 属性ドラゴンごとの spawn/kill/soul ポップアップ |
