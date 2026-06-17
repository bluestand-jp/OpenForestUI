> 🌐 [English](README.md) ・ **日本語**

# Replay API データモデル（Riot `/replay/*`）

クライアント内蔵の **Replay API**（`https://127.0.0.1:2999/replay/*`）向けのプレーンな C# DTO。`.rofl` 再生中のみ到達可能。`ReplayDataProvider` はこれらの型を GET/POST して再生クロックを読み取り、ブロードキャストのレンダリング中にゲーム内カメラ、HUD 可視性、ポストプロセスを駆動する。JSON バインドは `System.Text.Json` の `[JsonPropertyName]` 属性を用いて Riot の小文字プロパティ名に一致させる。

## 内容

| File | `/replay/*` リソース / 役割 |
| --- | --- |
| [`Game.cs`](Game.cs) | `/replay/game` — `processID` のみ。GET の成功は replay がアクティブであるシグナルとなる（`ReplayDataProvider.IsReplayActive` 参照）。 |
| [`Playback.cs`](Playback.cs) | `/replay/playback` — `time`、`length`、`speed`、`paused`、`seeking`。`gameTime` を上書きする replay クロックのソースであり、seek/pause POST のターゲット。 |
| [`Render.cs`](Render.cs) | `/replay/render` — フルなレンダー状態。カメラ、fog/depth-of-field、skybox、すべての `interface*` HUD トグル。ネイティブ HUD 要素を隠してオーバーレイで置き換えるために使う。 |
| [`Vector3.cs`](Vector3.cs) / [`Color.cs`](Color.cs) | `Render` および sequence 型が用いる `{x,y,z}` と `{r,g,b,a}` プリミティブ。 |
| [`Sequence.cs`](Sequence.cs) | `/replay/sequence` のキーフレームトラック（時間にわたるカメラ + fog/skybox パラメータ）。プロパティ単位のキーフレームラッパー（`DepthFogColor`、`HeightFogEnabled`、…）も宣言する。 |
| [`SequenceVector3.cs`](SequenceVector3.cs) / [`SequenceVectorEntry.cs`](SequenceVectorEntry.cs) | スカラー値および `Vector3` 値それぞれに対する単一キーフレーム（`blend`、`time`、`value`）。 |
| [`Recording.cs`](Recording.cs) | `/replay/recording` — クライアント側ビデオ録画の codec/path/resolution/fps と start/current/end time。 |
| [`InterfaceState.cs`](InterfaceState.cs) | 小さな内部フラグ（`TeamfightOpen`）。Riot DTO ではない。 |
