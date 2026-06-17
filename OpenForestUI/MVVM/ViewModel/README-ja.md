> 🌐 [English](README.md) ・ **日本語**

# コントロールセンターの ViewModel（MVVM のバインド可能ステート）

デスクトップ・コントロールセンターの View（[`../View/`](../View/README-ja.md)）背後にあるプレゼンテーションロジック層。各 view-model はバインド可能なプロパティと `ICommand` を公開する。大半はアプリの config/state（`ConfigController.Component.*`、`IngameController.CurrentSettings`）に対する薄いファサードであり、UI でコントロールをトグルすると永続化された放送設定へそのまま書き込まれる。モダナイズされたグラフは `Microsoft.Extensions.DependencyInjection` で構築され、`INavigationService`/`AppRoute`（[`../Core/Services/`](../Core/Services/README-ja.md) 参照）でナビゲートされる。view-model は段階的移行の途中で、CommunityToolkit の `ObservableObject`（source-gen の `[ObservableProperty]`）か、レガシーの `OpenForestUI.MVVM.Core.ObservableObject` シムのいずれかを継承する。

## 内容

| File | 役割 |
| --- | --- |
| `MainViewModel.cs` | シェル view-model。`NavigateCommand` + ウィンドウ chrome コマンド、`Nav`（現在の view/route）と `State`（接続ステータス）を公開する。DI シングルトンを解決する `static` な `XVM` アクセサの「strangler facade」を保持し、レガシー呼び出し元と新しい結線が単一のオブジェクトグラフを共有できるようにする。 |
| `HomeViewModel.cs` | ダッシュボードホスト。`PickBan`/`Ingame`/`PostGame`/`InfoEdit` の子 view-model を注入し、InfoEdit オーバーレイの開閉/可視状態を駆動する。CommunityToolkit の source-gen へモダナイズ済み。 |
| `PickBanViewModel.cs` | チャンピオンセレクト（「Pre Game」）ページのステート。`IsActive`/`IsOpen`、開閉コマンド。コンストラクタは config から static な青/赤の `TeamConfigViewModel` を初期投入する。 |
| `PickBanConfigViewModel.cs` | チャンピオンセレクトオーバーレイ設定のファサード（パッチ、spells/coaches/score トグル、デフォルトの青/赤カラー + ブラシ、pick-ban ディレイ）。シングルトンの `ChampSelectSettings`。 |
| `TeamConfigViewModel.cs` | チーム別 config（name/tag/score/coach/color、ロゴアイコンパス、ブロードキャストの region/seed/flag メタデータ）。static な `BlueTeam`/`RedTeam` インスタンスを持つ。`JSONConfigProvider` 経由でロード/セーブし、カラー変更を config へ反映する。 |
| `IngameViewModel.cs` | Ingame オーバーレイページのステートに加え、入れ子の 3 つの機能グループ view-model（`ObjectivesTabViewModel`、`PlayersTabViewModel`、`TeamsTabViewModel`）と再利用可能な `ControlButtonViewModel`（title/description/有効トグルのタイル）。`IsActive` をトグルすると ingame パイプラインの有効/無効が切り替わり、ライブ tick を再開する。 |
| `IngameTeamsViewModel.cs` | ライブロスターのステート。static な `BluePlayers`/`RedPlayers` の observable コレクション + 変更イベント、ドラッグ並べ替え（`OnProcessDrop`）付きの `PlayerViewModel`（name/champion/team/baron フラグ）。ドラッグ並べ替えは下層ゲームステートのプレイヤー順を再同期する。 |
| `PostGameViewModel.cs` | ポストゲームオーバーレイページのステート（`IsActive`/`IsOpen` + 開閉コマンド）。 |
| `SettingsViewModel.cs` | アプリ設定のファサード。offset 更新チェック、Farsight メモリリーダーのトグル、一時的な mock フィードのトグル、クレジット/バージョン文字列。 |
| `StartupViewModel.cs` | ロード用スプラッシュのステート。ステータステキスト、ロード進捗、アップデートプロンプトのフラグ。`LoadStatus` enum の各段階と Data Dragon ダウンロード進捗をプログレスバーへマッピングする。 |
| `ConnectionStatusViewModel.cs` | タイトルバーのステータスチップ用のカラー/ラベルモデル。static なパレットインスタンス（Disconnected / Client Loaded / Connected / Mocking / Issue Found）を持ち、`AppStateController` が毎 tick 書き込む。 |
| `ColorPickerViewModel.cs` | `ColorPickerWindow` 背後の RGB <-> `Color`/`SolidColorBrush` バインドモデル。 |
| `InfoEditViewModel.cs` | Home view-model の InfoEdit ボタンコマンド/可視性を `InfoEditView` へ公開する薄いプロキシ。 |
