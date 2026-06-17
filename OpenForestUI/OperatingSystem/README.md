> 🌐 **English** ・ [日本語](README-ja.md)

# OS interop & helpers

Low-level Windows interop and small utility helpers used by the desktop app: synthesizing keyboard/
mouse input into the League client window, watching for game processes starting/stopping, and a few
enum/color/messagebox extensions. This is how OpenForestUI drives the in-game HUD (e.g. forcing
scoreboard/replay state) and detects when the game launches.

## Contents

- **`Input.cs`** — `InputUtils`: P/Invoke wrappers over `user32.dll` (`SendInput`, `GetCursorPos`,
  `SetForegroundWindow`, `GetForegroundWindow`, …) plus the `INPUT`/`KEYBDINPUT`/`MOUSEINPUT` structs
  and a `KeyCode` enum. Exposes `SendKeyPress`/`SendKeyDown`/`SendKeyUp`/`MultiKeyPress`, cursor
  get/set, and `GetActiveProcess`. Consumed by `Common/Controllers/GameInputController.cs`, which
  briefly focuses the League window, sends keystrokes, then restores the previously active window and
  cursor position.
- **`ProcessEventWatcher.cs`** — `ProcessEventWatcher` (`IDisposable`): uses WMI
  (`System.Management` / `ManagementEventWatcher`) to fire `ProcessStarted` / `ProcessStopped` events
  for any `Win32_Process` creation/deletion. Adapted from the GoldDiff project (attributed in-source).
- **`ProcessEventArguments.cs`** — event-args carrying the affected `ProcessId`. Also from GoldDiff
  (attributed in-source).
- **`Extensions.cs`** — small helpers: `Enum.Next<T>()` (cycle to next enum value), `string.ToColor()`
  / `Color.ToSerializedString()` (parse/format `rgb(r,g,b)` for config), `MessageBoxUtils.ShowErrorBox`
  (single-flight error dialog on its own thread), and `FlagsHelper.Set`/`Unset` for `[Flags]` enums
  (used by `IngameWSClient`'s `FrontEndType`).
