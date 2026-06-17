> 🌐 [English](README.md) ・ **日本語**

# ブランドフォント

OpenForestUI のワードマークに使われる **Venus Rising** ブランド書体を格納する。

## 内容

- **`Venus Rising Rg.otf`** — 横長・全大文字のブランドフォント。単にコピーされるのではなく `OpenForestUI.csproj` に WPF `Resource` として含まれるため、pack URI が実行時に解決される。XAML では `FontFamily="pack://application:,,,/Assets/Fonts/#Venus Rising"` として参照される — `#` の後のファミリ名はファイル名ではなく**`Venus Rising`**（書体の内部名）である点に注意。

ブランドワードマーク用に `MVVM/View/MainWindow.xaml`、`StartupWindow.xaml`、`HomeView.xaml` から利用される。
