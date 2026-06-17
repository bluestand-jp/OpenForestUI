> 🌐 [English](README.md) ・ **日本語**

# Champ-select WebSocket イベント (送信)

OpenForestUI が pickban ブラウザソースオーバーレイへ `ws://localhost:9001/api` 経由でプッシュするイベントペイロード。各型は `OpenForestUI.Common.Events.LeagueEvent` を継承し、オーバーレイの JS がディスパッチに使う文字列 `eventType` を設定する。これらは `StateInfo/State` のハンドラと `Common/Controllers/PickBanController` から発行され、その後シリアライズされて組み込みソケットサーバ経由で送られる。

## 内容

| File | `eventType` | 役割 |
| --- | --- | --- |
| `NewState.cs` | `newState` | ドラフトの完全なスナップショット — 現在の `StateData` (teams、timer、phase) を運ぶ。あらゆる state/timer 更新時に送られる。 |
| `NewAction.cs` | `newAction` | 直前にアクティブになったスロット — `CurrentAction` (state `pick`/`ban`/`none`、team、slot index) を運ぶ。アクティブアクションが変わったときに送られる。 |
| `ChampSelectStart.cs` | `champSelectStart` | ドラフト開始を知らせる。 |
| `ChampSelectEnd.cs` | `champSelectEnd` | ドラフト終了を知らせる。 |
| `Heartbeat.cs` | `heartbeat` | 定期的 (10 秒ごと) な keepalive。現在の `PickBanConfig` もオーバーレイへ届ける。 |
