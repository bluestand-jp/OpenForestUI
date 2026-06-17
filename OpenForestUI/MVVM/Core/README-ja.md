> 🌐 [English](README.md) ・ **日本語**

# MVVM プリミティブ

view-model と view が構築される土台となる低レベルの構成要素: observable 基底クラス、コマンド実装、
小さな WPF ヘルパー。view-model が依存する DI サービスは
[`Services/`](Services/README-ja.md) サブディレクトリにある。

## Contents

| File | Role |
| --- | --- |
| `ObservableObject.cs` | プロジェクトの `ObservableObject` 基底を `CommunityToolkit.Mvvm.ComponentModel.ObservableObject` に再ポイントする shim。これにより既存の `: ObservableObject` な view-model は変更なくコンパイルでき、同時に `[ObservableProperty]`/`[RelayCommand]` のソースジェネレータを利用できる。すべての view-model がツールキット基底を直接参照したら削除する。 |
| `RelayCommand.cs` | `execute`/任意の `canExecute` デリゲートを持つ `ICommand`。`CanExecuteChanged` を `CommandManager.RequerySuggested` 経由でルーティングする。 |
| `DelegateCommand.cs` | よりシンプルな `ICommand` (常に実行可能)。`Key`/`ModifierKeys`/`MouseAction` のジェスチャメタデータも保持する。 |
| `DependencyObjectExtension.cs` | ヘルパー: `TryCast<T>` と `FindAncestorOfType` (visual-tree の走査)。 |
| `InstantBinding.cs` | 双方向 + 検証をプリセットした `Binding` サブクラス: `InstantBinding` (`UpdateSourceTrigger.PropertyChanged`) と `LostFocusBinding` (`UpdateSourceTrigger.LostFocus`)。テキストボックスのテーマで使用。 |

## サブディレクトリ

| Directory | Purpose |
| --- | --- |
| [`Services/`](Services/README-ja.md) | view-model に注入される DI サービス (navigation、config、app state、window) |
