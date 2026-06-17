> 🌐 [English](README.md) ・ **日本語**

# GameObject — League メモリから読み取る単一ユニット

このディレクトリには [Farsight](../README-ja.md) メモリリーダーが使用するインメモリのユニットモデルを
格納する。`GameObject` は League のゲーム内で移動するあらゆる物体や建造物 — チャンピオン、タワー、
dragon、baron、herald — の基底型であり、ゲームの ObjectManager ツリーの 1 ノードを C# でミラーした
ものである。

## 内容

- [`GameObject.cs`](GameObject.cs) — ユニットクラスとそのハイドレーションロジック。
  - メモリから読み取る **Fields**: `ID`、`NetworkID`、`Team`、`Position`（`Vector3`）、`Name`、
    `DisplayName`、`Health`／`MaxHealth`、`Mana`／`MaxMana`、そして（チャンピオンのみ）
    `CurrentGold`、`GoldTotal`、`EXP`、`Level`。
  - **`LoadFromMemory(baseAddr, buffSize)`** — オブジェクトのバイトブロックを読み取り、各フィールドを
    パッチ固有のオフセットでデコードする。`Name`／`DisplayName` の short（インライン ≤16 バイト）と
    long（ポインタ間接参照）双方の文字列レイアウトを扱う。
  - **`LoadChampFromMemory`** — gold/XP/level を取得するためチャンピオンに対してのみ実行される。
  - **`IsChampion()`** — `Name` を `FarsightController.Champions`（`OpenForestUI.Common` 内の
    CommunityDragon チャンピオンデータ由来）と照合するルックアップをキャッシュする。
  - **`Offsets`**（ネストクラス） — ユニットごとのフィールドオフセットテーブル。各フィールドは
    `HexStringJsonConverter` を使用し、`Config/Farsight.json` の hex 文字列から
    `FarsightController.ObjectOffsets` へデシリアライズされる。これらの値は **パッチ固有** であり、
    League の更新時にリフレッシュする必要がある —
    [親 README](../README-ja.md#オフセットのリバースエンジニアリングパッチ後の更新) を参照。
