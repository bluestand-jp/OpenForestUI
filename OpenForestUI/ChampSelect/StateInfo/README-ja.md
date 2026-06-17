> 🌐 [English](README.md) ・ **日本語**

# Champ-select の状態と変換

チャンピオンセレクトパイプラインの中核。生の LCU セッションデータを正規化されたオーバーレイモデルへ変換し、現在のドラフト状態をシングルトンとして保持し、どの pick/ban スロットが「アクティブ」かを判定する。`Common/Controllers/PickBanController` があらゆる LCU イベント時と各ティック時にここを呼び出す。

## 内容

| File | 役割 |
| --- | --- |
| `CurrentState.cs` | 生の LCU `Session` に `isChampSelectActive` フラグを加えた薄いラッパー — `Converter.ConvertState` へ渡される入力。 |
| `Converter.cs` | 純粋な変換ロジック: グループ化された LCU アクションを平坦化し、正規化された `Team` を構築し (picks/bans、`DataDragon` 経由のスペル、`AppStateController` 経由の名前)、カウントダウンタイマー (`ConvertTimer`) とフェーズラベル (`ConvertStateName`、例 "BAN PHASE 1"/"PICK PHASE 2"/"FINAL PHASE") を計算する。`StateConversionOutput` を出力する。 |
| `State.cs` | 静的な状態ハブ: シングルトン `StateData` を保持し、差分検出済みの更新を適用し (`NewState`)、`EventHandler` 群 (`StateUpdate`、`NewAction`、`ChampSelectStarted`、`ChampSelectEnded`) と Data Dragon の version/CDN/config アクセサを公開する。`TriggerUpdate` がオーバーレイへ配信する。 |
| `StateData.cs` | 現在のドラフト状態 (`blueTeam`/`redTeam`、`timer`、`state`、`champSelectActive`、`leagueConnected`、`config`)。ネストした `CurrentAction` 型と、現在アクティブな pick/ban スロットを導出・再解決する `GetCurrentAction`/`RefreshAction` も定義する。 |
