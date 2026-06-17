> 🌐 [English](README.md) ・ **日本語**

# プロセス制御エンドポイント

LCU の `process-control/v1/process/*` ルートをラップする。これらは League Client プロセス（UI だけでなくクライアント全体）のライフサイクルを管理する。

## 内容

| ファイル | 役割 |
| --- | --- |
| [`IProcessControlEndpoint.cs`](IProcessControlEndpoint.cs) | 公開インターフェース。`QuitAsync`、`RestartAsync`（任意の `restartVersion` 付き）、`RestartToRepair`、`RestartToUpdate(delaySeconds, selfUpdateUrl)`。 |
| [`ProcessControlEndpoint.cs`](ProcessControlEndpoint.cs) | 実装。`process-control/v1/process/{quit,restart,restart-to-repair,restart-to-update}` に POST し、`delaySeconds` / `restartVersion` / `selfUpdateUrl` をクエリパラメータとして渡す。 |
