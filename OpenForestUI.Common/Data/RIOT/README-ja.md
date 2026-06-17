> 🌐 [English](README.md) ・ **日本語**

# Riot 静的データモデル

ゲームから来る生の値（特に Farsight メモリリーダーと Community Dragon）を解釈するために使う静的なゲームデータモデル。Riot の固定テーブル — レベルごとの XP カーブとアイテムのゴールドコスト — をエンコードし、アプリの他の部分が生の数値を意味のある状態へ変換できるようにする。

## 構成内容

- `ChampionLevel.cs` — Riot の XP→レベル換算表（レベル 1–18、累積 XP しきい値付き）。`ChampionLevel.EXPToLevel(exp)` はテーブルを二分探索し、生の XP 値（例: メモリから読み取った値）をチャンピオンレベルへ変換する。
- `ItemData.cs`
  - `ItemData` — アプリ側アイテムレコード（`itemID`、ゴールド `ItemCost`、`specialRecipe`、`sprite`、`name`）。
  - `CDragonItem` — Community Dragon からパースされたアイテム（`id`、`name`、ビルド構成要素 `from`、`price`/`priceTotal`、`iconPath`）。静的な `All` と `Full` セットがロード済みアイテムを保持する。
- `ItemCost.cs` — アイテムのゴールドコストのペア: `total`（購入）と `sell`（売却）。
