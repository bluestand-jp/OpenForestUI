> 🌐 [English](README.md) ・ **日本語**

# "Europe" レイアウトのスタイル

ドラフトオーバーレイ用の LESS スタイルシート。[`../`](../README-ja.md) のコンポーネントから CSS モジュール（`import css from './style/index.less'`）としてインポートされる。`index.less` がアグリゲータで、残りはそれが `@import` するパーシャルである。ここでエクスポートされるクラス名（`css.Overlay`、`css.Timer`、`css.Pick`、アニメーション状態クラスなど）は JSX から直接参照される。

## Contents

| File | Role |
| --- | --- |
| `index.less` | エントリスタイルシート: 基本の `body`／ボックスモデル、中央ボックス／ロゴ／パッチのレイアウト、下記パーシャルの `@import`。 |
| `variables.less` | `:root` の CSS カスタムプロパティ — ボックス寸法、クロップ、チーム／タイマー／フォントの色（チームカラーは実行時に config から上書きされる）。 |
| `fonts.less` | [`../../assets/fonts/`](../../assets/fonts/README-ja.md) のフォント向けの `@font-face` 宣言。 |
| `animation.less` | 段階的なドラフトリビールのキーフレーム／状態クラス（`TheAbsoluteVoid`、`AnimationHidden`、`AnimationTimer`、`AnimationBansPick` など）。`Overlay.jsx` がシーケンスを制御する。 |
| `timer.less` | 中央ドラフトタイマーのスタイル（ブルー／レッドのアクティブサイド背景、数字）。 |
| `team.less` | チーム別ブロックのレイアウト（ピックカラム、名前、スコア、コーチ）。 |
| `picks.less` | ピックスロットのスタイル（スプラッシュクロップ、サモナースペル、アクティブ状態、プレイヤー名）。 |
| `bans.less` | バン行のスタイルと中央のバンスペーサー。 |
