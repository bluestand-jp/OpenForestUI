> 🌐 [English](README.md) ・ **日本語**

# 共有リソースディクショナリ (デザイントークン)

メニューの共有デザイン値を保持する XAML リソースディクショナリ。`Tokens.xaml` は
[`../../App.xaml`](../../App.xaml) で **最初に** マージされるため、すべてのテーマと view がこれらのキーを参照できる。

## Contents

- **`Tokens.xaml`** — コントロールセンターの色と主要寸法の単一の真実源。`Color.*` 値と対応する
  `Brush.*` の `SolidColorBrush` (タイトルバー/サイドバー/コンテンツの背景、アクセント、ボーダー、テキスト) に加え、
  `Dim.*` の double (シェル幅/高さ、タイトルバー高さ、サイドバー幅、コンテンツ幅) を定義する。値はかつて
  `MainWindow.xaml` とテーマディクショナリ全体にインラインで散在していたリテラルから引き上げたもので、
  移行ではハードコードされたリテラルをこれらのキーへ段階的に再ポイントする。
- **`ColorStyles.xaml`** — `ToggleSwitch` コントロールのビジュアル状態
  (static / mouse-over / pressed / checked / disabled、および on 状態のバリアント) 用の `SolidColorBrush` パレット。
  [`../Theme/GenericToggle.xaml`](../Theme/GenericToggle.xaml) が消費する。
