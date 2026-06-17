> 🌐 [English](README.md) ・ **日本語**

# HUD OCR サイドカー（正確な CS / gold / objective 数）

観戦版 Live Client Data API が丸めるか省略する値 — per-player の正確な **CS**、
per-team の **gold**、per-team の **objective 数**（grubs/baron/dragon/towers） — を
ネイティブ HUD から直接読み取る Python OCR の概念実証およびサイドカー。観戦 API は
CS を 10 の倍数に切り捨て、gold や objective モンスターのキルを一切公開しない
（[`../docs/api/`](../docs/api/README-ja.md) を参照）ため、OCR が唯一の厳格に正確な
ソースである。`goldcap.py` は C# アプリが起動して JSON 行を読み取るライブサイドカーで、
残りはリファレンスパイプライン / キャリブレーションツール。

## ライブキャプチャの動作要件

`goldcap.py --live` は **自分の画面そのもの**（DXGI Desktop Duplication）をキャプチャし、
固定のピクセル領域を読み取ります。そのため、HUD が見えていて、ROI を較正したときと同じ
レイアウトになっているときだけ読み取れます:

- **解像度 1920×1080。** すべての ROI（`ROI_1080`, `TOWER_ROI_1080`, `CS_ROI_1080`,
  `OBJ_ROI_1080`）は 1080p のボーダーレスフルスクリーンで較正しています。他の解像度では
  自動でスケールします（`scaled_roi`: `W/1920, H/1080`）が、LoL の HUD はきれいに比例
  スケールしないのでズレることがあります。ズレたら再較正してください。
- **観戦の UI／HUD スケールは較正時の設定に。** 基準フレームは UI スケール **100（最大）**
  で撮っています。スケールが違うとパネルが ROI からズレます。
- **オブザーバーのトップバーを表示したままに。** チームゴールド・タワー数・オブジェクト行
  （grub / baron / dragon / tower）はすべてここから読みます。
- **詳細スコアボード（下部中央パネル）を開いたままに。** デフォルトのズーム／位置で、各
  プレイヤーの CS をその 10 セルから読みます（B0–B4 = 左チーム / ORDER 上→下、
  R0–R4 = 右チーム / CHAOS）。
- **その領域を何かで覆わないこと。** 他のウィンドウ・ポップアップ・マウスカーソル・配信
  オーバーレイがバーに重なると読み取りが壊れます。設計上、失敗・不審な読み取りは
  *保持または非表示*（tri-state の Known / Stale / Unknown）にし、誤った値は出しません。
- **プライマリモニタで。** `dxcam` は既定でプライマリ出力を取得します。観戦クライアントは
  そこで動かしてください（別の出力を使うなら `make_cam()` を編集）。
- 標準の **青/赤のオブザーバー配色**を前提にしています（数字はチームカラーで、
  `_emphasis()` がそれを利用します）。

## サイドカーの実行

```bash
py -m pip install -r ../requirements.txt   # 初回のみ。依存はリポジトリ直下の requirements.txt

py goldcap.py --grab frame.png   # デスクトップを1枚保存（ROI の較正・確認用）
py goldcap.py --probe            # 1フレームだけキャプチャしてリーダーを1回実行、結果を表示
py goldcap.py --live --fps 4     # 連続ループ: tri-state の JSON 行を stdout に出力（C# が読む）
```

C# アプリが `--live` を自分で起動して JSON 行を読むので、ふだん手動で動かす必要はありません。
別の環境に合わせて再較正するときは、フレームを `--grab` で取得して桁の領域を調べ、
`--roi "x0,x1,y0,y1;x0,x1,y0,y1"`（blue;red）で渡すか、`topbar_reader.py` の `*_1080`
定数を調整してください。

## 内容

- [`goldcap.py`](goldcap.py) — **ライブサイドカー**。`dxcam`（DXGI Desktop
  Duplication）でオブザーバートップバーをキャプチャし、リーダーを実行し、tri-state の
  JSON 行を stdout に emit する（per-team gold + towers、10セル CS、per-team
  objective 数）。モード: `--grab`（ROI キャリブレーション用にフレームを保存）、
  `--probe`（1回読み）、`--live`（連続ループ）。C# 側がリプレイシークを報告したとき、
  stdin の `reset` を読んで全ゲートを再ロックする。
- [`topbar_reader.py`](topbar_reader.py) — **確定版リーダーロジック**（C# に移植
  すべきリファレンス）。固定 1080p ROI（`ROI_1080`, `TOWER_ROI_1080`, `CS_ROI_1080`,
  `OBJ_ROI_1080`）、emphasis→Otsu→upscale→EasyOCR の桁パイプライン、範囲チェック +
  hold-last-good 付きの単調/有界な **`TeamGate`** → tri-state（Known / Stale /
  Unknown — 嘘をつくより非表示にする）。`read_cs` は glyph-segmentation + NCC
  テンプレート分類器を使用（テンプレートは `_harvest.py` 由来）。
- [`digit_templates.npz`](digit_templates.npz) — NCC 分類器用の正準 0–9 桁
  テンプレート（HUD フォント、`top` + `cs` コンテキスト）。`_harvest.py` でビルド。
  手編集しないこと。
- [`_harvest.py`](_harvest.py) — **ネイティブ HUD** キャプチャのみからラベル付き桁
  グリフを収穫して `digit_templates.npz` をビルドする（異なるフォントを使う
  オーバーレイレンダーは除外）。
- [`csdiag.py`](csdiag.py) — one-shot の CS セル診断: PIL/GDI でキャプチャし、10 個の
  CS セルを切り出し、特定セルが失敗する理由をデバッグするために `read_cs` +
  per-glyph の segmentation/NCC 内訳を出力する。
- [`replay.py`](replay.py) — 記録済みの raw-OCR JSONL ログを（確定版の）parse + ゲートで
  再実行し、新しいライブキャプチャ無しでロジック変更を検証する。

## サブディレクトリ

| Directory | 目的 |
|---|---|
| [`overlay-harness/`](overlay-harness/README-ja.md) | ヘッドレスレンダーハーネス — モック WS state を ingame オーバーレイに供給してスクリーンショットを撮り、レイアウトを検証する |

> キャリブレーション/デバッグキャプチャ（`cs_*.png`）と `__pycache__` は gitignore 対象。
> 新しい per-player CS パイプラインは `topbar_reader.py` 内にインラインで記述。
