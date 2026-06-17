> 🌐 [English](README.md) ・ **日本語**

# ウィンドウ／実行ファイルアイコン

OpenForestUI デスクトップアプリ用の `.ico` アイコン — 実行ファイルアイコンおよび WPF ウィンドウアイコンとして使われる。

## 内容

- **`OpenForestUI.ico`** — 現行のアプリアイコン（緑の「weeds」マーク）。`OpenForestUI.csproj` の `<ApplicationIcon>`（`.exe` のアイコン）に設定され、`MainWindow.xaml` と `StartupWindow.xaml` の `Icon` として使われる。ビルド時に出力ディレクトリへコピーされる。
- **`BlueEssence.ico`** — 上流系譜から受け継いだレガシーアイコン。csproj によって今も出力にコピーされるが、現行のウィンドウからは参照されていない。
