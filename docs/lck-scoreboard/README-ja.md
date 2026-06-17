> 🌐 [English](README.md) ・ **日本語**

# 比較スコアボードオーバーレイ — 仕様 & リファレンス

放送向けトップバーと共存するよう設計された、ingame オーバーレイ内の
**プロ放送スタイルの下部比較スコアボードレイアウト** を再現するための
仕様とリファレンスアセットです。実装は
`Overlays/ingame/src/visual/LckScoreboardVisual.ts` で、加算的な
`BottomStyle: 'prm' | 'lck'` の config ディスクリミネータで選択されます。既存の
per-player ロスター（`state.scoreboard.Players`）を再利用するため、新しい
バックエンドデータは不要です。

## 内容

- [`SPEC.md`](SPEC.md) — `/goal` ビルド仕様: スコープ/non-goals、バンドレイアウトの
  リファレンスデコード（チャンピオン+レベル · アイテム · KDA · CS のミラー配置 5
  ロール行と、中央レーンのゴールド差）、メトリクス → データソースのマップ、
  ファイルごとのフロントエンド変更、キャリブレーション済み 1920×1080 ジオメトリ、
  保持すべき不変条件、検証ハーネス手順、受け入れ基準。日付付きの「implemented &
  harness-verified」ステータスと、組み込みのオペレータ判断（端の dmg/vision/KP の
  数値と per-player ゴールド列は **スコープ外**; ゴールドは中央レーンのゴールド差
  としてのみ表示）を含む。
- `lck_reference.png` — §7 のジオメトリをレンダーハーネスでキャリブレーションする
  際の対象となる放送リファレンスフレーム（プロの試合, ~1920×1080, イベント/シリーズ名,
  パッチ 26.11）。

[`../../ocr-poc/overlay-harness/`](../../ocr-poc/overlay-harness/README-ja.md) で検証済みです。
放送向けトップバーの下部バー（[`../prm-overlay/`](../prm-overlay/README-ja.md)）と同じ
パターンに従います。
