> 🌐 [English](README.md) ・ **日本語**

# オーバーレイの画像・フォントアセット

オーバーレイに同梱される静的アート（React コンポーネントから直接インポートされるため、webpack がハッシュ化／インライン化する）。ライブのチャンピオンアートは実行時にバックエンドから `/cache` URL として届く。ここにあるファイルは、チャンピオンデータが存在しない前／無い間に表示されるフォールバックとブランディングである。

## Contents

| File | Role |
| --- | --- |
| `BlueEssence.png` | ドラフト中央ボックスに表示される中央ロゴ。[`../europe/Overlay.jsx`](../europe/Overlay.jsx) からインポートされる。 |
| `ban_placeholder.svg` | 空のバンスロットの正方形。`convertState.js` がバンを 5 件まで埋めるために使用する。 |
| `top_/jung_/mid_/bot_/sup_splash_placeholder.svg` | ロール別のピックスロットスプラッシュプレースホルダ（インデックス 0-4）。`convertState.js` がピックを 5 件まで埋めるために使用する。 |

## Subdirectories

| Directory | Purpose |
| --- | --- |
| [`fonts/`](fonts/README-ja.md) | オーバーレイの `@font-face` ルールから参照される TrueType フォント |
