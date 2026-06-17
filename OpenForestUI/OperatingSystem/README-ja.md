> 🌐 [English](README.md) ・ **日本語**

# OS インターロップ＆ヘルパー

デスクトップアプリが使う低レベルの Windows インターロップと小さなユーティリティヘルパー。League クライアントウィンドウへのキーボード／マウス入力の合成、ゲームプロセスの起動／停止の監視、そしていくつかの enum／color／messagebox 拡張が含まれる。これが OpenForestUI がゲーム内 HUD を制御し（例: スコアボード／リプレイ状態の強制）、ゲーム起動を検知する仕組みである。

## 内容

- **`Input.cs`** — `InputUtils`: `user32.dll` 上の P/Invoke ラッパー（`SendInput`、`GetCursorPos`、`SetForegroundWindow`、`GetForegroundWindow` など）に加え、`INPUT`/`KEYBDINPUT`/`MOUSEINPUT` 構造体と `KeyCode` enum。`SendKeyPress`/`SendKeyDown`/`SendKeyUp`/`MultiKeyPress`、カーソルの get/set、`GetActiveProcess` を公開する。`Common/Controllers/GameInputController.cs` から利用され、同コントローラーは League ウィンドウを一瞬フォーカスしてキーストロークを送り、その後それまでアクティブだったウィンドウとカーソル位置を復元する。
- **`ProcessEventWatcher.cs`** — `ProcessEventWatcher`（`IDisposable`）: WMI（`System.Management` / `ManagementEventWatcher`）を用いて、あらゆる `Win32_Process` の生成／削除に対して `ProcessStarted` / `ProcessStopped` イベントを発火する。GoldDiff プロジェクトから流用（ソース内で出典明記）。
- **`ProcessEventArguments.cs`** — 影響を受けた `ProcessId` を運ぶイベント引数。これも GoldDiff 由来（ソース内で出典明記）。
- **`Extensions.cs`** — 小さなヘルパー: `Enum.Next<T>()`（次の enum 値へ巡回）、`string.ToColor()` / `Color.ToSerializedString()`（config 用に `rgb(r,g,b)` をパース／フォーマット）、`MessageBoxUtils.ShowErrorBox`（専用スレッド上の single-flight なエラーダイアログ）、そして `[Flags]` enum 用の `FlagsHelper.Set`/`Unset`（`IngameWSClient` の `FrontEndType` で使用）。
