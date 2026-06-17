> 🌐 [English](README.md) ・ **日本語**

# Ingame オーバーレイイベント (ingame オーバーレイへの WebSocket メッセージ)

このディレクトリは、デスクトップアプリが **ingame** ブラウザソースオーバーレイへ WebSocket (`ws://localhost:9001/api`) 経由でプッシュするアウトバウンドイベント型を保持する。各型は `OpenForestUI.Common.Events.LeagueEvent` を継承し、オーバーレイが分岐する `eventType` 文字列の判別子を持つ。`IngameController` / `State` パイプラインがライブゲームデータ (Live Client API、Farsight、OCR サイドカー) からこれらを構築してブロードキャストし、オーバーレイがバナー・タイマー・スコアボードをアニメーションさせる。

中核となるメッセージは周期的な `HeartbeatEvent` で、シリアライズ済みの `StateData` スナップショット全体を運ぶ ([`../State/`](../State/README-ja.md) を参照)。残りの型は、アニメーション付きポップアップ (オブジェクト取得/出現、アイテム完成、レベルアップ、ゲームライフサイクル) 向けの一過性のワンショット通知である。

## Contents

| File | Type(s) | Role |
| --- | --- | --- |
| `RiotEvent.cs` | `RiotEvent`, `EventTypes` | Riot の `/liveclientdata/eventdata` イベントをミラーする DTO (EventID、名前、時刻、killer/victim/assisters、turret/inhib 破壊、キルストリーク)。`EventTypes` は合成的な Baron/Dragon の取得/終了マーカー (`EventID == -1`) 向けのファクトリヘルパーを持つ。 |
| `RiotEventList.cs` | `RiotEventList` | `List<RiotEvent> Events` を保持する薄いラッパー — `/eventdata` バッチのデシリアライズ形状。 |
| `Heartbeat.cs` | `HeartbeatEvent` | `eventType = "GameHeartbeat"`。`StateData` スナップショットをラップする、オーバーレイへの主要な周期的状態プッシュ。 |
| `ObjectiveKilled.cs` | `ObjectiveKilled`, `ObjectiveKilledSimple` | `eventType = "ObjectiveKilled"`。オブジェクト名 + それを取得したチーム。`RiotEvent` 派生のバリアントはゲーム時刻も運ぶ。オブジェクト取得バナーを駆動する。 |
| `ObjectiveSpawn.cs` | `ObjectiveSpawn`, `ObjectiveSpawnSimple` | `eventType = "ObjectiveSpawn"`。オブジェクト (例: Baron) がカウントダウンゼロで出現したときに発火し、出現ポップアップを駆動する。 |
| `ItemCompleted.cs` | `ItemCompleted` | `eventType = "ItemCompleted"`。プレイヤー id + 完成した `ItemData`。アイテム購入通知を駆動する。 |
| `PlayerLevelUp.cs` | `PlayerLevelUp` | `eventType = "PlayerLevelUp"`。プレイヤー id + 新レベル (6/11/16 のパワースパイク)。 |
| `BuffDespawn.cs` | `BuffDespawn` | `eventType = "BuffDespawn"`。Baron/Elder バフが終了したときのオブジェクト名 + チーム id。 |
| `GameStart.cs` | `GameStart` | `eventType = "GameStart"`。ゲーム開始のライフサイクルシグナル。 |
| `GameEnd.cs` | `GameEnd` | `eventType = "GameEnd"`。ゲーム終了のライフサイクルシグナル。 |
| `GamePause.cs` | `GamePause` | `eventType = "GamePause"`。プレイが一時停止したときの現在のゲーム時刻を運ぶ。 |
| `GameUnpause.cs` | `GameUnpause` | `eventType = "GameUnpause"`。プレイが再開したときの現在のゲーム時刻を運ぶ。 |
| `IngameOverlay.cs` | `IngameOverlay` | アクティブな `IngameConfig` を公開するシングルトン `OverlayConfig` (`FrontEndType.Ingame`)。一過性のイベントではなく、オーバーレイ設定ペイロード。 |

## Notes

- `RiotEvent.EventID == -1` は内部で生成された**合成的**マーカー (オブジェクトの取得/終了) を表し、ID が `>= 0` の実際の Riot イベントとは区別される。`State.cs` はこれを利用して、ティックごとの `/eventdata` 上書きを跨いでオブジェクトマーカーを引き継ぐ ([`../State/`](../State/README-ja.md) を参照)。
- `*Simple` バリアントは、完全な `RiotEvent` フィールドを伴わず名前/チームのみを送るケースのために存在する。
