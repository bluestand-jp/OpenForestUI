> 🌐 [English](README.md) ・ **日本語**

# コントロールセンターの View（WPF XAML ページ・ウィンドウ群）

OpenForestUI デスクトップ・コントロールセンターの視覚レイヤー。Fluent/WPF-UI のダッシュボードシェルに加え、機能ごとのページ（Pre Game / Ingame / Post Game / Settings）と少数のポップアップウィンドウで構成される。各 `*.xaml` には薄い `*.xaml.cs` code-behind が対になる。ページは [`../ViewModel/`](../ViewModel/README-ja.md) の view-model にバインドし、`INavigationService`（[`../Core/Services/`](../Core/Services/README-ja.md) 参照）によってシェルへ差し替えられる。ルーティングは `AppRoute` enum をキーとする。MVVM モダナイズの一環で、chrome/クリックロジックの大半は code-behind から command・behavior・service へ移された。残る code-behind は XAML バインドでは表現できないもの（ファイルダイアログ、ドラッグ&ドロップ、リストの初期投入）のみを担う。

## 内容

| File | 役割 |
| --- | --- |
| `MainWindow.xaml(.cs)` | トップレベルのボーダーレスウィンドウ。サイドバーナビゲーション（`MainViewModel.NavigateCommand` にバインドした RadioButton 群）、タイトルバーのステータスピル、`Nav.CurrentView` を描画する `ContentControl` ホストを持つ。code-behind は所有ダイアログのセンタリングのみを保持し、ドラッグ/最小化/クローズは behavior + `IWindowService` 経由で動く。 |
| `StartupWindow.xaml(.cs)` | メインウィンドウ前に表示されるロード用スプラッシュ。glow 背景、ステータステキスト、`StartupViewModel.LoadProgress` で駆動するプログレスバーを持つ。任意の「アップデートあり」プロンプト（Update / Skip ボタンが view-model の `Update`/`SkipUpdate` イベントを発火）をホストする。 |
| `HomeView.xaml(.cs)` | ダッシュボードのランディングページ。機能カード（Pre Game / Ingame / Post Game）と InfoEdit オーバーレイの chevron をホストする。純粋な code-behind（`InitializeComponent` のみ）。 |
| `PickBanView.xaml(.cs)` | チャンピオンセレクト（「Pre Game」）設定ページ。チーム名/タグ/スコア/コーチ、パッチ、チーム別カラーとロゴ、保存チームセレクタ、サイドスワップを扱う。code-behind はテキスト検証、`JSONConfigProvider` 経由のチーム設定永続化、ロゴのファイルダイアログ、`ColorPickerWindow` の起動を担う。 |
| `IngameView.xaml(.cs)` | Ingame オーバーレイ設定ページ。Objectives / Players / Teams 機能グループ向けの Fluent トグルリスト。code-behind は 3 つのパネルの `DataContext` を `IngameViewModel` から割り当て（これらはバインド可能でない素のフィールド）、シェル破棄時に向けて null ガードされている。 |
| `IngameTeamsView.xaml(.cs)` | ingame オーバーレイ用のライブロスターパネル。ドラッグ&ドロップで並べ替え可能な青/赤プレイヤーリストと、best-of シリーズ数セレクタ。code-behind は `ListViewDragDropManager`（vendored の `WPF.JoshSmith` ドラッグ&ドロップ）を結線し、放送コントローラ向けに static な `InitPlayers`/`ClearPlayers` を公開する。 |
| `PostGameView.xaml(.cs)` | ポストゲームオーバーレイ設定ページ。最小限の code-behind（ロード時に詳細ペインを畳む）。 |
| `SettingsView.xaml(.cs)` | アプリ設定ページ。ログレベルと Data Dragon ロケールのセレクタ、offset 更新 / メモリリーダー（Farsight） / mock フィードのトグル、加えてクレジット + バージョンのフッター。code-behind はコンボ選択を config へマッピングする。 |
| `InfoEditView.xaml(.cs)` | Home 上に重ねて表示されるスライドインの情報/ヘルプオーバーレイ。`HomeVM` の InfoEdit command へ転送する。純粋な code-behind。 |
| `ColorPickerWindow.xaml(.cs)` | チーム / デフォルト pick-ban カラー用の RGB カラーピッカーダイアログ。code-behind は 0-255 入力を検証し、選択色を `TeamConfigViewModel` または `PickBanConfigViewModel` へ書き戻し、チームカラーのリフレッシュをトリガする。 |

Notes:
- 「Pre Game」はチャンピオンセレクトページのリネーム後の名称。型名は全体を通じて依然 `PickBan` を使う。
- 一部のページは折りたたみ可能な詳細ペイン（`OpenContent`）を公開する。code-behind で非表示に初期化され、behavior / navigation service によってアニメーションで開く。
