> 🌐 [English](README.md) ・ **日本語**

# 放送トップバーオーバーレイ — 仕様 & リファレンス

ingame オーバーレイ内で **プロ/esports の放送トップバー** を再現するための仕様、
リファレンスフレーム、抽出済みアイコン。Live Client Data API（port 2999）、LCU
クライアント、オペレータ config からの厳格に正確なデータを用い、近似を表示するより
値を非表示にすることを優先する。`Overlays/ingame/src/visual/PrmScoreboardVisual.ts`
（トップバー）+ `PrmBottomBarVisual.ts`（下部比較バー）として実装され、
`OverlayConfig.PrmScore` で opt-in する。

## 内容

- [`SPEC.md`](SPEC.md) — `/goal` ビルド仕様: リファレンスデコード（トップバー領域 +
  中央 objective カウンタ）、メトリクス → データソースのマップ、ヴォイドグラブ
  （`HordeKill`）イベントの検証、色/レイアウト仕様、config 駆動のカウンタスロット
  アーキテクチャ、バックエンド + フロントエンドの変更リスト、検証ハーネス、
  フェージング（Phase 1 トップバー + Phase 2 下部バー、両方とも実装済み &
  harness-verified）。ビジュアル差分時に再確認すべき open questions を含む。
- `prm_reference.png` — レイアウト/色がキャリブレーションされる対象の、デコード済み
  1920×1080 放送リファレンス（プロの試合, 19:52）。
- 抽出済みトップバーカウンタアイコン（white-on-transparent）。`Overlays/ingame/public/images/prm/`
  以下のオーバーレイ内アセットのソース:
  - `ic_tower.png` — タレットシルエット（towers カウント）
  - `ic_grub.png` — ヴォイドグラブの頭（void grubs カウント）
  - `ic_shield.png` — シールド/クレスト（dragons カウント）

[`../../ocr-poc/overlay-harness/`](../../ocr-poc/overlay-harness/README-ja.md) で検証済み。
下部の比較スコアボード（[`../lck-scoreboard/`](../lck-scoreboard/README-ja.md)）はこの
同じパターンに従う。
