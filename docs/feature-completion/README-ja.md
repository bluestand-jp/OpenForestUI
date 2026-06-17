> 🌐 [English](README.md) ・ **日本語**

# Vanguard 互換の feature-completion 設計

すべての Ingame オーバーレイ機能を **メモリ読み無し** で動作させるための設計ノート。
フォークのポリシーに従う: 厳格に正確（近似より非表示を優先）、Vanguard 互換
（Live Client Data API + HUD OCR のみ）。これは、かつて機能していなかった Ingame タイル
（Baron Timer、spawn ポップアップ、Gold Graph/Tab、EXP Tab）を、動作する — あるいは
正直に利用不可な — 機能に変えた設計である。

## 内容

- [`DESIGN.md`](DESIGN.md) — 完全な設計。ハイライト:
  - **Ground truth** — 各タイルが壊れていた根本原因（file:line）（例:
    符号反転の rewind 再計算で seed-and-decrement されていたタイマー、デッドの
    `goldHistory`/`csHistory`、パッチ 14.1 で Riot に削除された LiveEvents API）。
  - **Design 1 — `ObjectiveSpawnClock`**: seed+decrement を、kill イベント +
    `ObjectiveTimingsConfig` から Dragon/Herald/Baron のカウントダウンを純粋に
    tick ごとに導出する方式に置き換える。Baron タイマー、spawn ポップアップ、
    rewind 符号バグを修正。*実装済み & テーブルテスト済み。*
  - **Design 2 — OCR チームゴールドからの Gold Graph**; **Design 3 — Gold Tab**
    （推定による per-player gold）; **Design 4 — EXP Tab**（XP はメモリ読み無しでは
    構造的に取得不可 → 機能を取り下げ）。
  - **Design 5 — `FeatureAvailability`** capability map。各タイルが、ある機能が
    今は正しく動作できないことを自己ドキュメントするためのもの。
  - パッチ定数、再帰的な検証プラン、ビルド順、日付付きの「Implemented」/
    review-hardening ノート。

検証プランは [`../../ocr-poc/overlay-harness/`](../../ocr-poc/overlay-harness/README-ja.md)
のモックフィクスチャを拡張する（例: `mock-state-empty-infopage.json`、回帰フィクスチャ
としてここに追加）。
