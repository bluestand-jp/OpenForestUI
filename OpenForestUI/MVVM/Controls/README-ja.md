> 🌐 [English](README.md) ・ **日本語**

# カスタムコントロール

コントロールセンターの view で使うカスタムのテンプレート化 WPF コントロール。

## Contents

- **`ToggleSwitch.cs`** — `ToggleButton` から派生した WinUI スタイルの on/off スイッチ。ヘッダーの配置/整列/パディング、
  checked/unchecked 状態のテキスト、スイッチ図形の幅とパディング、状態ごとのブラシ (checked/unchecked の背景・前景・ボーダー)、
  そして列整列用の `SharedSizeGroupName` を依存プロパティとして公開する。レイアウトは `VisualStateManager`
  (ヘッダー/スイッチ配置の状態) と `OnApplyTemplate` で設定するテンプレートパートのバインディングによって駆動される。
  既定のテンプレートは [`../Theme/GenericToggle.xaml`](../Theme/GenericToggle.xaml) にあり、これは
  [`ToggleSwitchOffsetConverter`](../Converters/ToggleSwitchOffsetConverter.cs) を介してサムのオフセットも結線する。
