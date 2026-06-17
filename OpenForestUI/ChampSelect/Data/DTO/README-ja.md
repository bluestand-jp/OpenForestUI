> 🌐 [English](README.md) ・ **日本語**

# 正規化された champ-select DTO (オーバーレイ向け)

`StateInfo/Converter` が生の `LCU/` 型から生成する、クリーンでオーバーレイ向けのドラフトモデル。これらが pickban オーバーレイの消費対象である。各チームの picks と bans、現在アクティブなスロット、サモナースペル/名前、Data Dragon の version/CDN メタデータ。Champion とサモナースペルのペイロードは `OpenForestUI.Common` (`Champion`、`FrontEndSummonerSpell`) 由来で、`DataDragon` プロバイダ経由で解決される。

## 内容

| File | 役割 |
| --- | --- |
| `PickBan.cs` | `Champion` を保持する基底型。`Pick` と `Ban` の親。 |
| `Pick.cs` | ドラフトの pick: スロット `id`、2 つのサモナースペル (`FrontEndSummonerSpell`)、`isActive`、`displayName`。 |
| `Ban.cs` | ドラフトの ban: チャンピオンに加えて `isActive`。 |
| `Team.cs` | ドラフトの片側: `bans`、`picks`、`isActive`。値ベースの `Equals`/`GetHashCode` を実装しており、`State` が更新間でチームを差分検出できる。 |
| `Meta.cs` | Data Dragon メタデータのラッパー: `cdn` URL に加えて `Version`。 |
| `Version.cs` | 現在の Data Dragon の `champion` および `item` バージョン (`DataDragon` プロバイダから読み取る)。 |
