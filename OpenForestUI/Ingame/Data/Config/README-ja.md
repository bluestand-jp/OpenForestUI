> 🌐 [English](README.md) ・ **日本語**

# Ingame & Farsight 設定モデル

ingame オーバーレイとメモリリーダーを駆動する、2 つの永続化 JSON config セクション。いずれも `OpenForestUI.Common` の `JSONConfig`（バージョン管理付き、デフォルト生成・マイグレーションフックを備える）から派生し、`ConfigController` を通じて `Ingame.json` / `Farsight.json` としてロードされる。

## 内容

- [`IngameConfig.cs`](IngameConfig.cs) — すべての ingame オーバーレイ要素に対する大規模なレイアウト/スタイリング config。scoreboard、inhibitors、オブジェクトの kill/spawn ポップアップ、item-completed および level-up アニメーション、info サイドページ、gold グラフ、Baron/Elder power play、Dragon/Baron タイマー、Google フォント、そしてオプトインの `PrmScore` ブロック（ブロードキャスト トップバー + 比較 scoreboard スタイル）を含む。ネストされた `*DisplayConfig` / `FontConfig` / `VisualElementAnimationConfig` 型と、大きな `CreateDefault()` ベースラインを内包する。現在のファイルバージョンは `3.1`。
- [`FarsightConfig.cs`](FarsightConfig.cs) — メモリリーダーの **offset**（`GameOffsets`、`ObjectOffsets`）と `OffsetVersion` を保持する。`CreateDefault`/`UpdateValues` は、設定された offset リポジトリからローカルパッチに合致する offset ファイルを自動ダウンロードする。失敗時には `FarsightController.ShouldRun` を無効化する。現在のファイルバージョンは `3.0`。
