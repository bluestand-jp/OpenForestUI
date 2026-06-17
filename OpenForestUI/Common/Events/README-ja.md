> 🌐 [English](README.md) ・ **日本語**

# 共有オーバーレイイベント基底型

アプリが WebSocket サーバー (`ws://localhost:9001/api`) 経由でブラウザソースのオーバーレイへ配信する型付きメッセージのための共通基底クラス。具体的なイベント (例: `GameHeartbeat`、`GameStart`、`ObjectiveSpawn`) は `Ingame.Events` / `ChampSelect.Events` 名前空間に存在し、ここで定義される `eventType` 判別子を使ってシリアライズされる。

## 内容

| ファイル | 役割 |
|------|------|
| [`LeagueEvent.cs`](LeagueEvent.cs) | すべてのオーバーレイイベントの抽象基底。オーバーレイが分岐に使う `eventType` 文字列を保持する。サブクラスは自身の型名をコンストラクタへ渡す。 |
| [`OverlayConfig.cs`](OverlayConfig.cs) | 抽象 `LeagueEvent` (`eventType = "OverlayConfig"`)。config プッシュが正しいオーバーレイ (ingame か pickban か) へルーティングされるよう `FrontEndType` を保持する。 |

## 補足

- `FrontEndType` は [`../../Http/IngameWSClient.cs`](../../Http/IngameWSClient.cs) で定義される。イベントは `EmbedIOServer.SocketServer?.SendEventToAllAsync(...)` 経由で送られる。
