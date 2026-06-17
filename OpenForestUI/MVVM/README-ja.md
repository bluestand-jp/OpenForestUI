> 🌐 [English](README.md) ・ **日本語**

# MVVM レイヤー (WPF コントロールセンター UI)

OpenForestUI デスクトップアプリ (アセンブリ `OpenForestUI.exe`) の WPF プレゼンテーションレイヤー全体である。
コントロールセンターは CommunityToolkit.Mvvm + Microsoft.Extensions.DependencyInjection を基盤に MVVM ファーストで構築され、
`INavigationService`、デザイントークン、Fluent/WPF-UI のダッシュボードシェルを備える。View は view-model にバインドし、
view-model はコントローラや具象ウィンドウに手を伸ばすのではなく薄い DI サービスとやり取りする。

このアプリはオーバーレイそのものではなく *コントロールセンター* である。これらの画面は、組み込みサーバが WebSocket / HTTP 上で配信する
ブラウザソースのオーバーレイ (`ingame`、`pickban`) を切り替え・設定する。

## サブディレクトリ

| Directory | Purpose |
| --- | --- |
| [`Core/`](Core/README-ja.md) | MVVM プリミティブ: `ObservableObject`、relay/delegate コマンド、dependency-object ヘルパー |
| [`Core/Services/`](Core/Services/README-ja.md) | view-model に注入される DI サービス (navigation、config、app state、window) |
| [`View/`](View/README-ja.md) | XAML ウィンドウ/ページとその code-behind (シェル、Home、Settings、PickBan、Ingame、PostGame) |
| [`ViewModel/`](ViewModel/README-ja.md) | 各 view を支える view-model |
| [`Models/`](Models/README-ja.md) | view-model がバインドする UI 側のモデル/ヘルパー型 |
| [`Converters/`](Converters/README-ja.md) | XAML バインディングで使う `IValueConverter` |
| [`Behaviors/`](Behaviors/README-ja.md) | code-behind のイベントハンドラを置き換える attached behavior (例: ウィンドウドラッグ) |
| [`Controls/`](Controls/README-ja.md) | カスタム WPF コントロール (テンプレート化された `ToggleSwitch`) |
| [`Theme/`](Theme/README-ja.md) | コントロールごとの `Style` / `ControlTemplate` リソースディクショナリ |
| [`Resources/`](Resources/README-ja.md) | デザイントークン (color、brush、寸法)。`App.xaml` で最初にマージされる |
| [`DragDrop/`](DragDrop/README-ja.md) | ベンダー取り込みの Josh Smith 製 ListView ドラッグドロップヘルパー (Ingame ロスターで使用) |

すべてのテーマとリソースディクショナリは [`App.xaml`](../App.xaml) でマージされる。`Resources/Tokens.xaml`
が最初にマージされるため、テーマと view は共有トークンを参照できる。
