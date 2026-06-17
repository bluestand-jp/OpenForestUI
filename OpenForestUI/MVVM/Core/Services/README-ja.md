> 🌐 [English](README.md) ・ **日本語**

# DI サービス (MVVM の継ぎ目)

view-model がコントローラや具象ウィンドウに直接触れる代わりに利用する、依存性注入されるサービス。
それぞれはアプリの `Microsoft.Extensions.DependencyInjection` コンテナに登録され、コンストラクタ経由で注入される。
これらのインターフェースはテスト可能な継ぎ目であり、将来のアウトオブプロセス (web) メニューを、
view-model を変更せずに到達可能なまま保つ。

## Contents

| Interface | Implementation | Role |
| --- | --- | --- |
| `INavigationService.cs` | `NavigationService.cs` | 「どのページを表示しているか」の単一の真実源。`CurrentView` (`ContentControl` がバインド)、`CurrentRoute` (`EnumToBooleanConverter` 経由でサイドバーのラジオボタンがバインド)、`NavigateTo(AppRoute)` を公開する。`AppRoute` enum (`Home`、`PickBan`、`Ingame`、`PostGame`、`Settings`) を定義する。このサービスはページの view-model を注入し、各ページの `IsOpen` を切り替えてその完全な view をレンダリングさせる。 |
| `IConfigService.cs` | `ConfigService.cs` | 静的な `ConfigController` 上の薄くテスト可能な継ぎ目。Home ページが駆動するコンポーネントの「active」トグル (`PickBanActive`、`IngameActive`、`PostGameActive`) と、`./Config/Component.json` に永続化する `Save()` を公開する。 |
| `IStateService.cs` | `StateService.cs` | UI が監視する、ライブで非永続のアプリ状態を保持する。現状はタイトルバーのピルがバインドする LCU/ゲームの `ConnectionStatus`。セッターはディスパッチャスレッドへマーシャリングするため、バックグラウンドスレッドの LCU/ゲームコールバックは安全である (潜在的なクロススレッド書き込みを塞ぐ)。`DISCONNECTED` で開始する。 |
| `IWindowService.cs` | `WindowService.cs` | ウィンドウ管理アクションをバインド可能なコマンドとして: `Register(window)` (シェル表示時に一度呼ぶ)、`Minimize()`、`Close()`。ドラッグは [`WindowDragBehavior`](../../Behaviors/WindowDragBehavior.cs) が別途処理する。 |

これらは、view-model が具象の `MainWindow` 参照を保持し `ConfigController.Component.*` を直接読み取っていた
旧来のパターンを置き換える。
