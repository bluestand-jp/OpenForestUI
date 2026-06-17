> 🌐 **English** ・ [日本語](README-ja.md)

# Riot client (UX) endpoint

Wraps the LCU `riotclient/*` routes that control the client's user-interface window and zoom level.

## Contents

| File | Role |
| --- | --- |
| [`IRiotClientEndpoint.cs`](IRiotClientEndpoint.cs) | Public interface: `MinimizeUxAsync`, `ShowUxAsync`, `FlashUxAsync`, `KillUxAsync`, `KillAndRestartUxAsync`, `UnloadUxAsync`, `LaunchUxAsync`, and `GetZoomScaleAsync` / `SetZoomScaleAsync`. |
| [`RiotClientEndpoint.cs`](RiotClientEndpoint.cs) | Implementation. POSTs to `riotclient/{ux-minimize,ux-show,ux-flash,kill-ux,kill-and-restart-ux,unload,launch-ux}`; GET/POST `riotclient/zoom-scale` for reading/writing the UX zoom (set via the `newZoomScale` query parameter). |
