> 🌐 [English](README.md) ・ **日本語**

# プロジェクトドキュメント & 設計仕様

OpenForestUI — 公開・MIT ライセンスの League of Legends 放送オーバーレイ群のハブ —
のためのリファレンスドキュメント、ビルド仕様、設計ノート。これらのファイルは
**ライブデータソースが実際に何を公開しているか**（機能を厳格に正確かつ
Vanguard 互換に保つため）と、オーバーレイが照合される **レイアウト仕様** を記録する。
`OpenForestUI/` アプリ、`Overlays/` ブラウザソース、`ocr-poc/` サイドカーが
実装上一致すべき source of truth である。

## サブディレクトリ

| Directory | 目的 |
|---|---|
| [`api/`](api/README-ja.md) | League ローカル API（`https://127.0.0.1:2999`、Live Client Data + Replay API）の完全リファレンス — 観戦時に何が取れて何が取れないか |
| [`data/`](data/README-ja.md) | アプリが使用するアーカイブ済みゲームデータテーブル（Farsight メモリオフセット） |
| [`feature-completion/`](feature-completion/README-ja.md) | メモリリーダー無しで全 Ingame 機能を動作させるための設計（objective クロック、ゴールドグラフ、capability map） |
| [`lck-scoreboard/`](lck-scoreboard/README-ja.md) | 下部の比較スコアボードビジュアルの仕様 + リファレンス画像 |
| [`prm-overlay/`](prm-overlay/README-ja.md) | 放送用トップバーオーバーレイの仕様 + リファレンス画像/アイコン |

## 内容

- ほとんどの末端ドキュメントは `/goal` ビルドターゲットとして書かれている: 目次、
  リファレンスデコード、メトリクス → データソースのマップ、検証 / 受け入れ基準の
  明示的なリスト。実装ステータスのノートはインラインで保持し、機能が着地するごとに
  日付を付ける。
- オーバーレイ仕様（[`prm-overlay/`](prm-overlay/README-ja.md), [`lck-scoreboard/`](lck-scoreboard/README-ja.md)）は
  [`../ocr-poc/overlay-harness/`](../ocr-poc/overlay-harness/README-ja.md) の
  ヘッドレスレンダーハーネスに対して検証される。
