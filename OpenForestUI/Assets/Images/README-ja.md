> 🌐 [English](README.md) ・ **日本語**

# アプリ内画像

WPF ダッシュボード UI が使う小さな PNG ビットマップ。ブランドロゴといくつかのコントロール用グリフ。実行時に相対パスで読み込まれるため、いくつかは `OpenForestUI.csproj` によって出力ディレクトリへコピーされる。

## 内容

- **`OpenForestUI_icon.png`** — メインの アプリ内ロゴ。メインウィンドウのブランドヘッダー（`MainWindow.xaml`）と起動／スプラッシュウィンドウ（`StartupWindow.xaml`）に表示される。
- **`ArrowsDownWhite.png`** — `IngameView.xaml` と `InfoEditView.xaml` の折りたたみ可能なセクションで使われる白いシェブロン／エキスパンダーグリフ。
- **`LeagueOfLegendsIcon.png`** — デフォルトのチームロゴのプレースホルダー。カスタムロゴが設定されていないとき、`PickBanView.xaml.cs` がこれをチームアイコンフォルダに `Default.png` としてコピーする。
- **`BE_icon.png`** / **`BE_icon_white.png`** — 上流系譜由来のレガシーな「Blue Essence」マーク。`BE_icon.png` は今も csproj の NuGet `<PackageIcon>` として配線されているが、両方ともそれ以外は痕跡的である。
