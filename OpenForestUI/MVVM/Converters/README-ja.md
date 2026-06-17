> 🌐 [English](README.md) ・ **日本語**

# 値コンバーター (XAML バインディング)

view-model の値を表示用に適合させるため、XAML から参照される `IValueConverter` 実装。
これらの多くは一方向 (Convert のみ) であり、特記がない限り `ConvertBack` は例外を投げるか何もしない。

## Contents

| File | Role |
| --- | --- |
| `EnumToBooleanConverter.cs` | `ConverterParameter` と比較して enum を bool にマップする、双方向。サイドバーの `RadioButton.IsChecked` を `INavigationService.CurrentRoute` にバインドする。`ConvertBack` はボタンが checked になったときにのみ route を返す (それ以外は `Binding.DoNothing` を返す)。 |
| `InvertableBooleanToVisibilityConverter.cs` | Bool → `Visibility`。マッピングを反転させる `Normal`/`Inverted` の `ConverterParameter` を持つ。 |
| `BooleanToColorConverter.cs` | Bool → `SolidColorBrush`。任意の `ConverterParameter` として CSV `ColorIfTrue;ColorIfFalse;Opacity` (既定は `LimeGreen;Transparent;1.0`)。 |
| `StringToImageSourceConverter.cs` | URI 文字列 → `BitmapFrame.Create` 経由の `ImageSource` (オンデマンド読み込み、キャッシュ無視)。失敗時は `null` を返す。 |
| `ToggleSwitchOffsetConverter.cs` | `ToggleSwitch` テンプレート用に、スイッチ幅からサムの移動オフセットを計算する。`IsReversed` で符号反転。[`../Theme/GenericToggle.xaml`](../Theme/GenericToggle.xaml) で使用。 |
