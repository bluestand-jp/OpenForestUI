> 🌐 [English](README.md) ・ **日本語**

# OpenForestUI — WPF コントロールセンターアプリ

`OpenForestUI` プロジェクトは OpenForestUI のデスクトップ・コントロールセンターである。**`OpenForestUI.exe`** にビルドされる WPF（.NET 6、`net6.0-windows`）アプリケーションで、League of Legends のライブゲームデータを取り込み、組み込みのオーバーレイ Web サーバーを実行し、放送中に使うオペレーター用ダッシュボードを提供するホストプロセスである。

[LeagueBroadcast](https://github.com/floh22/LeagueBroadcast)（floh22、MIT）からのハードフォーク派生である。UI は MVVM に刷新されており、[CommunityToolkit.Mvvm]、DI コンテナ用の `Microsoft.Extensions.DependencyInjection`、`INavigationService`、デザイントークン、そして [WPF-UI]（`net6.0-windows` を対象とする最後のリリースである 3.0.5 に固定）上に構築された Fluent ダッシュボードシェルを採用している。

## 全体の組み立て

- **`App.xaml` / `App.xaml.cs`** — アプリケーションのエントリポイントであり DI のコンポジションルート。`OnStartup` は `BroadcastController.Instance` に最初に触れる**前**に `ServiceCollection`（nav/config/state/window サービス＋各ページの view-model、すべてシングルトン）を構築する — これはコードが明示的に指摘している順序の不変条件である。WPF-UI Dark テーマの上に OpenForestUI のグリーンアクセント（`#5CC59E`）を適用する。`App.xaml` は WPF-UI Fluent ディクショナリに加えてプロジェクトのデザイントークンとコントロールごとのテーマをマージし、`DataTemplate` を介して view-model を view にマッピングする。
- **`AssemblyInfo.cs`** — WPF テーマリソース解決のための `ThemeInfo` 属性。
- **`OpenForestUI.csproj`** — `WinExe`、アプリアイコン `Assets/Icons/OpenForestUI.ico`、`Resource` として埋め込まれた Venus Rising ブランドフォント。3 つの兄弟プロジェクト（`LCUSharp`、`OpenForestUI.Common`、`OpenForestUI.Farsight`）とパッケージ（EmbedIO、WPF-UI、CommunityToolkit.Mvvm、System.Management）を参照する。publish 後のターゲットで `ingame`/`pickban` オーバーレイをビルドし、OCR サイドカーをステージングし、リリースを zip 化する。

## サブディレクトリ

| Directory | Purpose |
| --- | --- |
| [`Assets/`](Assets/README-ja.md) | ビルドに同梱されるアプリのブランディングアセット（アイコン、画像、ブランドフォント） |
| [`ChampSelect/`](ChampSelect/README-ja.md) | チャンピオンセレクト（ピック＆バン）のデータモデル、状態トラッキング、LCU 連携 |
| [`Common/`](Common/README-ja.md) | アプリレベルのコントローラー、config/state の配線、共有ヘルパー |
| [`Http/`](Http/README-ja.md) | ブラウザソースのオーバーレイへ給電する組み込み EmbedIO Web/WebSocket サーバー |
| [`Ingame/`](Ingame/README-ja.md) | インゲームオーバーレイのデータ層（Live Client API、Farsight、OCR）とイベント DTO |
| [`MVVM/`](MVVM/README-ja.md) | ダッシュボード UI の view、view-model、コンバーター、テーマ、デザイントークン |
| [`OperatingSystem/`](OperatingSystem/README-ja.md) | Win32 入力合成、プロセス監視、小さな拡張ヘルパー |
