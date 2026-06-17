> 🌐 **English** ・ [日本語](README-ja.md)

# Process-control endpoint

Wraps the LCU `process-control/v1/process/*` routes, which manage the lifecycle of the League Client processes (the whole client, not just its UI).

## Contents

| File | Role |
| --- | --- |
| [`IProcessControlEndpoint.cs`](IProcessControlEndpoint.cs) | Public interface: `QuitAsync`, `RestartAsync` (with optional `restartVersion`), `RestartToRepair`, `RestartToUpdate(delaySeconds, selfUpdateUrl)`. |
| [`ProcessControlEndpoint.cs`](ProcessControlEndpoint.cs) | Implementation. POSTs to `process-control/v1/process/{quit,restart,restart-to-repair,restart-to-update}`, passing `delaySeconds` / `restartVersion` / `selfUpdateUrl` as query parameters. |
