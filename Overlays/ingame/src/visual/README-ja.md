> 🌐 [English](README.md) ・ **日本語**

# オーバーレイ visual 要素（Phaser 描画コンポーネント）

インゲームオーバーレイの画面要素ごとに 1 クラス。各クラスは抽象基底 `VisualElement` を継承する。基底はライフサイクル（`Load` / `Start` / `Stop` / `UpdateValues` / `UpdateConfig`）、tween 駆動のアニメーション状態、シーンへの登録を標準化する。`IngameScene` はこれらを `OverlayConfig` から生成し、各 `StateData` ハートビートを供給する。

## 内容

| ファイル | 役割 |
| --- | --- |
| [`VisualElement.ts`](VisualElement.ts) | 全 visual の抽象基底: id 登録、`PlayAnimationState` tween シーケンサー、`AnimationStart`/`Complete` シグナル、テキストスタイルヘルパー。 |
| [`VisualComponent.ts`](VisualComponent.ts) | Phaser ゲームオブジェクトを `Size` と `AnimateScale` フラグと対にする軽量ラッパー。アニメーションシーケンサー用。 |
| [`ScoreboardVisual.ts`](ScoreboardVisual.ts) | レガシー / デフォルトのスコアボード: チームごとの kills/towers/gold、drake アイコン、スコア、名前、中央クロック。マスク付き、config 駆動のレイアウト。 |
| [`PrmScoreboardVisual.ts`](PrmScoreboardVisual.ts) | オプトインの放送トップバー（プロ / e スポーツ放送スタイル）: グラデーションバー、チームごとのオブジェクトカウンター（dragons/grubs/towers）、ゴールド + リードバッジ、kills、チームパネル、中央クロック。`PrmScore.Enabled` のとき `ScoreboardVisual` を置き換える。 |
| [`PrmBottomBarVisual.ts`](PrmBottomBarVisual.ts) | 下部の比較スコアボード: 5 つのレーンマッチアップ行（アイコン / スペル / アイテム / KDA / CS / レベル）、中央を軸にミラー。DataDragon アイコン。`PrmScore.BottomBar` でオプトイン。 |
| [`LckScoreboardVisual.ts`](LckScoreboardVisual.ts) | 別スタイルの下部比較スコアボード（プロ / e スポーツ放送スタイル）: 5 つのロール行、中央のレーンごとゴールド差とリーダー矢印、パッチラベル。`BottomStyle === 'lck'` のとき `PrmBottomBarVisual` の代わりに使われる。 |
| [`InfoPageVisual.ts`](InfoPageVisual.ts) | サイド情報ページ: プレイヤーごとのスタットタブ（gold/XP/CSPM）にアイコン、値バー、並び順。 |
| [`GraphVisual.ts`](GraphVisual.ts) | 中央のゴールド差グラフ（rexUI 経由の Chart.js）、マスクによるワイプイン / アウト。オプトイン / `GoldGraph` データから遅延生成。 |
| [`ObjectiveTimerVisual.ts`](ObjectiveTimerVisual.ts) | dragon / baron のスポーンタイマーウィジェット（アイコン + カウントダウン）。レガシー（トップバー非使用）モード。 |
| [`PowerPlayVisual.ts`](PowerPlayVisual.ts) | Baron / Elder「Power Play」ウィジェット: ゴールド差と / またはタイマーをオブジェクトアイコンとともに表示。レガシー（トップバー非使用）モード。 |
| [`InhibitorVisual.ts`](InhibitorVisual.ts) | チーム / レーンごとのインヒビターリスポーンタイマー。チームカラーと背景のオプションあり。 |
| [`ObjectivePopUpVisual.ts`](ObjectivePopUpVisual.ts) | オブジェクトのキル / スポーン / ソウルポイント用の中央スコアボードポップアップ（画像または動画、アルファマスク）。 |
| [`ItemVisual.ts`](ItemVisual.ts) | プレイヤースロットに固定されるアイテム完成通知（アイコン + 任意のアイテム名テキスト）。 |
| [`LevelUpVisual.ts`](LevelUpVisual.ts) | プレイヤースロットに固定されるプレイヤーのレベルアップ通知（レベル数 + 色付き背景）。 |
