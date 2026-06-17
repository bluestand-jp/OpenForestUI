> 🌐 [English](README.md) ・ **日本語**

# 生の LCU champ-select DTO

League Client (LCU) `lol-champ-select` セッションの JSON 形状に一致するプレーンな C# モデル。これらは生の LCU WebSocket イベントペイロード (`PickBanController.ApplyNewState`) からデシリアライズされ、その後 `StateInfo/Converter` へ渡されてオーバーレイ向けの `DTO` 型へ正規化される。クライアントのフィールド名をそのまま反映しており、オーバーレイへ直接送られることはない。

## 内容

| File | 役割 |
| --- | --- |
| `Session.cs` | トップレベルの champ-select セッション: `myTeam`/`theirTeam` (`Cell` リスト)、グループ化された `actions` (`List<List<Action>>`)、`timer`。 |
| `Cell.cs` | 1 つのドラフトスロット: `cellId`、`championId`、`summonerId`、`spell1Id`/`spell2Id`。 |
| `Action.cs` | pick または ban アクション (`type`、`championId`、`actorCellId`、`completed`)。加えて `"pick"`/`"ban"` 定数を公開する `ActionType` ヘルパー。 |
| `Timer.cs` | カウントダウンの計算に使うフェーズタイミングフィールド (`phase`、`adjustedTimeLeftInPhase`、`internalNowInEpochMs`、…)。 |
| `Summoner.cs` | プレイヤー名のキャッシュ時に使う最小限のサモナーレコード (`displayName`、`summonerId`)。 |
