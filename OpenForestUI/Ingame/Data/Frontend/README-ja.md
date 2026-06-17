> 🌐 [English](README.md) ・ **日本語**

# フロントエンド ペイロードモデル（オーバーレイへ送出）

WebSocket（`ws://localhost:9001/api`）経由でシリアライズして ingame ブラウザソース オーバーレイへ送出する DTO 群。これらはブロードキャスト向けの形状で、`RIOT/` 内の生の Riot DTO とは別物であり、`ShouldSerialize*` ゲートに強く依存することで、各フィールドは対応するオーバーレイ機能が有効なとき（`IngameController.CurrentSettings` と `ConfigController.Component.Ingame` 経由）にのみ emit される。

## 内容

- [`FrontEndTeam.cs`](FrontEndTeam.cs) — 1 チームのブロードキャスト統計。name/icon/score、kills/towers/gold/plates/void-grubs/inhibitors、トップバー用の追加項目 `Baron`/`DragonCount`（オブジェクトモンスターの kill は `/eventdata` に含まれないため OCR 由来）、および region/seed/flag メタデータ。`Dragons` はチームの `dragonsTaken` リストへプロキシしてレガシーな pip を表示する。
- [`PlayerScoreboardEntry.cs`](PlayerScoreboardEntry.cs) — ボトム比較 scoreboard（10 人ロスター）における 1 プレイヤーの行。`From(...)` を介して `Player` から構築する。team/position/name/champion/level/KDA/CS、推定 `Gold`（プレイヤー単位の総 gold は Vanguard 下では公開されない — `Hub/Team.EstimatePlayerGold` 参照）、アイテムスロット ID、DataDragon の summoner-spell キー。
- [`ScoreboardConfig.cs`](ScoreboardConfig.cs) — トップレベルの scoreboard ペイロード。blue/red の `FrontEndTeam`、game time、シリーズの game カウント、トーナメント名、そしてオプションの `Players` ロスター。プレイヤーロスターと game time はカスタムのブロードキャスト scoreboard が有効なときにのみシリアライズされる。
- [`LocalFont.cs`](LocalFont.cs) — オーバーレイ用にローカル配信するフォントファイルを記述する小さな `{ Name, Location }` ペア。
