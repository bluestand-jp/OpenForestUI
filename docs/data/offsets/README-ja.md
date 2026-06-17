> 🌐 [English](README.md) ・ **日本語**

# Farsight メモリ読みオフセット（パッチごとアーカイブ）

`Offsets-<patch>.json` ファイルの歴史的カタログ — **OpenForestUI.Farsight** メモリ
リーダーが、観戦 API が省略するプロセス内ゲームデータ（現在/合計ゴールド、EXP、
HP/MP、座標、アイテム）を引き出すために使用するバイトオフセット。これらは
オプションのメモリリーダーが有効な場合にのみ意味を持つ。デフォルトの Vanguard 互換
構成では、アプリはオフセットのロードを完全にスキップする
（リーダーがオフのとき `ConfigController.LoadOffsetConfig` がショートサーキットする）。

## 内容

- `Offsets-<version>.json` — League パッチごとに1ファイル、`11.9.1` → `14.6.1`
  （加えて `13.11.2`, `13.21.2` のようなホットフィックス派生）。ファイル名接頭辞
  `Offsets-` は `ComponentConfig.OffsetPrefix` と一致する。アプリは稼働中の
  クライアントのパッチに対応するファイルを選択する
  （`AppStateController.LoadOffsets` → `ConfigController`）。
- アーカイブには **2世代のスキーマ** が現れる:
  - **レガシー（floh22/LeagueBroadcast 期、例 `11.9.1`）** — `GameOffsets` が
    オブジェクトごとのフィールドオフセットを保持し、`ObjectOffsets` が
    object-manager/map ポインタを保持する。キー: `Manager`, `Map*`, `ID`,
    `NetworkID`, `Team`, `Pos`, `Mana/MaxMana`, `Health/MaxHealth`,
    `CurrentGold/GoldTotal`, `EXP`, `Name`, `ItemList`, `SpellBook`。`FileVersion: "1.0"`。
  - **新しい世代（例 `14.x`）** — 2つのセクションが入れ替わり/拡張される:
    `GameOffsets` が manager/map ポインタ（`Manager`, `MapCount`, `MapRoot`,
    `MapNodeNetId`, `MapNodeObject`）を保持し、`ObjectOffsets` がオブジェクトごとの
    フィールド（`DisplayName`, `Level` など）を保持する。`OffsetVersion` が
    パッチ名を示す。`FileVersion: "3.0"`。

> 出自: このオフセット方式と初期のファイルは上流の LeagueBroadcast プロジェクト
> （floh22, MIT）に由来する。オフセットはパッチごとにリバースエンジニアリングされ、
> ゲームバイナリのメモリレイアウトが変わるたびに壊れる。これがパッチごとに1ファイル
> 存在する理由である。

## 消費のされ方

`ConfigController.Farsight` がアクティブなオフセット JSON から populate され、続いて
`FarsightController.GameOffsets` / `.ObjectOffsets` がそこから設定される
（`AppStateController.cs`）。リーダープロジェクトは
[`../../../OpenForestUI.Farsight/`](../../../OpenForestUI.Farsight/README-ja.md)。
