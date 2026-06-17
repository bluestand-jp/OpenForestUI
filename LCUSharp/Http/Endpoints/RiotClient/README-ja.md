> 🌐 [English](README.md) ・ **日本語**

# Riot client (UX) エンドポイント

クライアントのユーザーインターフェースウィンドウとズームレベルを制御する LCU の `riotclient/*` ルートをラップする。

## 内容

| ファイル | 役割 |
| --- | --- |
| [`IRiotClientEndpoint.cs`](IRiotClientEndpoint.cs) | 公開インターフェース。`MinimizeUxAsync`、`ShowUxAsync`、`FlashUxAsync`、`KillUxAsync`、`KillAndRestartUxAsync`、`UnloadUxAsync`、`LaunchUxAsync`、および `GetZoomScaleAsync` / `SetZoomScaleAsync`。 |
| [`RiotClientEndpoint.cs`](RiotClientEndpoint.cs) | 実装。`riotclient/{ux-minimize,ux-show,ux-flash,kill-ux,kill-and-restart-ux,unload,launch-ux}` に POST し、UX ズームの読み書きに `riotclient/zoom-scale` を GET/POST する（設定は `newZoomScale` クエリパラメータ経由）。 |
