> 🌐 [English](README.md) ・ **日本語**

# 集約ゲーム状態 hub モデル

ブロードキャスト側の集約モデル — tick ごとの生 Riot DTO（`RIOT/`）とフロントエンド ペイロード（`Frontend/`）の間に位置する層。これらの型は teams、players、inhibitors、objectives、そしてオーバーレイが表示する gold 値を蓄積する。Vanguard 互換性の制約が宿るのもここである。観戦 API がプレイヤー/チーム単位の gold を公開しないため、それらは推定または OCR 由来となる。

## 内容

- [`Team.cs`](Team.cs) — チーム単位の集約。players、kills/towers/plates、void grubs、破壊した inhibitors、baron/elder タイマー、`dragonsTaken`。**gold ロジック** を担う。`GetGold` は OCR 由来の `ExternalGold`（`GoldConfidence` フラグ付き）を優先し、それ以外では CS/kill/assist + 自然回収のヒューリスティックである `EstimatePlayerGold` にフォールバックする。また OCR 由来のオブジェクト数（`OcrGrubs/Baron/Dragons/Towers`）も保持し、存在する場合はイベント集計値を上書きする。
- [`PlayerTab.cs`](PlayerTab.cs) — info サイドページ行のビルダー。`GetGoldTabs`（推定プレイヤー単位 gold）と `GetCSPerMinTabs`（CS/min）で、それぞれ `ValueBar`（min/current/max）とチャンピオンアイコンパスを備えた `PlayerTab` 行を生成する。（XP タブは削除済み — Vanguard セーフなプレイヤー単位 XP ソースが存在しないため。）
- [`InfoSidePage.cs`](InfoSidePage.cs) — `PlayerOrder` enum（`MaxToMin` / `MinToMax`）でソートされた `PlayerTab` のサイドページ。
- [`Inhibitor.cs`](Inhibitor.cs) — 単一の inhibitor（`id`、`key`、`timeLeft`）と、標準 6 本の inhibitor をシードする `InhibitorInfo`。

## サブディレクトリ

| Directory | 役割 |
| --- | --- |
| [`Objectives/`](Objectives/README-ja.md) | Baron/Dragon の power-play および次回スポーンモデル。 |
