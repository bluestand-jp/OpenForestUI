> 🌐 [English](README.md) ・ **日本語**

# Ingame データモデル（Riot Live Client Data API）

観戦用 **Live Client Data API**（`https://127.0.0.1:2999/liveclientdata/*`）が返す JSON をミラーするプレーンな C# DTO。`IngameDataProvider` はエンドポイントのレスポンスをこれらの型へ直接デシリアライズし、ingame ステートマシン（`Ingame/State/State.cs`）がそれをブロードキャストモデルへ畳み込む。フィールド名は Riot の JSON と完全一致するため、Newtonsoft は属性なしでバインドできる。

## 内容

| File | エンドポイント / 役割 |
| --- | --- |
| [`GameMetaData.cs`](GameMetaData.cs) | `/gamestats` — `gameMode`、`gameTime`、map 名/番号/terrain。デバッグ値のデフォルトコンストラクタ。 |
| [`Player.cs`](Player.cs) | `/playerlist` の 1 エントリ。champion/items/runes/spells/scores に加え、ブロードキャスト側の追加項目を保持する。オプションの Farsight `GameObject`、マイルストーン/item-completion 検出用の level/inventory 差分トラッカー（`LastLevel`、`LastItemBySlot`、`LastCountBySlot`）、CS-per-minute ヘルパー。注意: `summonerName` は完全な `Name#Tag` 形式の Riot ID。`/eventdata` の名前は `riotIdGameName` と突き合わせること。 |
| [`Score.cs`](Score.cs) | プレイヤーの `scores` ブロック（kills/deaths/assists/CS/ward score/plates）。`Update` はオプションで CS をスキップできる。 |
| [`Item.cs`](Item.cs) | 1 つのインベントリアイテム（`itemID`、`price`、`slot`、`count`、…）。item-completion および gold 推定ロジックへ供給する。 |
| [`Rune.cs`](Rune.cs) / [`RuneList.cs`](RuneList.cs) | 1 つの rune と、プレイヤーの `runes` の keystone + primary/secondary ツリー。 |
| [`Summoner.cs`](Summoner.cs) / [`SummonerList.cs`](SummonerList.cs) | summoner spell（表示名/raw 名）と、プレイヤーの 2 つの spell スロット。 |
| [`Turret.cs`](Turret.cs) | turret kill のクレジット付与に使う、メモリリーダーの turret モデル（name、position、health、last-damaged-by マップ）。 |
