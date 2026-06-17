> 🌐 [English](README.md) ・ **日本語**

# Ingame データ provider（ローカル Riot HTTP クライアント）

OpenForestUI と、ゲームクライアントの port 2999 上のループバックエンドポイントとの間の薄い HTTP 層。各 provider は `HttpClient`（自己署名証明書を受け入れ、タイムアウト 250ms）を保持し、レスポンスを `RIOT/` と `Replay/` 配下の DTO へデシリアライズする。ingame ステートマシン（`Ingame/State/State.cs`）が tick ごとにこれらを呼び出す。

## 内容

- [`IngameDataProvider.cs`](IngameDataProvider.cs) — Live Client Data クライアント。`GetGameData`（`/gamestats`）、`GetPlayerData`（`/playerlist`）、`GetEventData`（`/eventdata`）。`PlaybackMode` enum（`Live` / `Spectator` / `Replay`）と `DetectPlaybackMode` も定義する。後者はエンドポイントを探査してセッションを分類する（replay は spectator に優先する。live ゲームは `/playerlist` がローカルプレイヤーしか公開しないため使えるパスが存在しない）。`IsSpectatorGame` は空の `activeplayername` を spectator のシグナルとして検出する。
- [`ReplayDataProvider.cs`](ReplayDataProvider.cs) — Replay API クライアント（全 static。static コンストラクタが `HttpClient` を初期化する）。`IsReplayActive`/`GETGameAsync`（`/replay/game`）、`GET/POSTPlaybackAsync`（`/replay/playback`、クロック + seek）、`GET/POSTRenderAsync`（`/replay/render`、カメラ + HUD トグル）。
- [`ObjectiveTakenArgs.cs`](ObjectiveTakenArgs.cs) — `/eventdata` パイプラインから発火し `IngameController` が消費する、Dragon/Baron/Herald 撃破用のイベント引数（`Type`、`Team`、`GameTime`）。
