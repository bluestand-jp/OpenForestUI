> 🌐 [English](README.md) ・ **日本語**

# 型付き LCU エンドポイント

個々の League Client REST ルートに対する強く型付けされたラッパー。各エンドポイントは `ILeagueRequestHandler` を受け取り、C# のメソッド呼び出しを対応する HTTP リクエストへと変換し、相対 URL とクエリパラメータを呼び出し側から隠蔽する。

## 内容

| ファイル | 役割 |
| --- | --- |
| [`EndpointBase.cs`](EndpointBase.cs) | `internal abstract` 基底クラス。すべてのエンドポイントが共有する `ILeagueRequestHandler` を保持する。 |

## サブディレクトリ

| ディレクトリ | 目的 |
| --- | --- |
| [`ProcessControl/`](ProcessControl/README-ja.md) | `process-control/v1/process/*` — quit、restart、restart-to-repair / restart-to-update。 |
| [`RiotClient/`](RiotClient/README-ja.md) | `riotclient/*` — クライアント UX の表示／最小化／フラッシュ／kill／起動、および UX ズームスケールの取得／設定。 |
