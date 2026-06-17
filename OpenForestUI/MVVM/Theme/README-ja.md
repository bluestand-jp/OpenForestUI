> 🌐 [English](README.md) ・ **日本語**

# コントロールテーマ (style & template)

コントロールセンターにカスタムのボーダーレスでダークな Fluent ルックを与える、コントロールごとの
`Style` / `ControlTemplate` リソースディクショナリ。これらはすべて [`../../App.xaml`](../../App.xaml) でマージされ、
[`../Resources/Tokens.xaml`](../Resources/Tokens.xaml) の共有デザイントークンを参照する。Style はキー付き
(例: `x:Key="MenuButtonTheme"`) で、`Style="{StaticResource ...}"` により明示的に適用される。

## Contents

| File | Styles |
| --- | --- |
| `MenuButtonTheme.xaml` | サイドバーのナビゲーション項目用の `RadioButton` style (`MenuButtonTheme`)。 |
| `ConnectionStatusTheme.xaml` | タイトルバーの接続ステータスピル用の `Button` style (`ConnectionStatusTheme`) と、saved-teams の `AddTheme` ボタン。 |
| `CloseButtonTheme.xaml` | タイトルバーの閉じる `Button` style。 |
| `MinimizeButtonTheme.xaml` | タイトルバーの最小化 `Button` style。 |
| `StartupButtonTheme.xaml` | startup/splash ウィンドウで使う `Button` style。 |
| `ColorSelectButtonTheme.xaml` | カラーピッカーのスウォッチ用の `Button` style (`ColorSelectTheme`、`ColorSelectLeftTheme`)。 |
| `ComboBoxTheme.xaml` | `ComboBox` テンプレートとそれを支えるブラシ。 |
| `TextBoxTheme.xaml` | `TextBox` style の `InstantUpdateTextBox` / `LostFocusTextBox` ([`../Core/InstantBinding.cs`](../Core/InstantBinding.cs) の `InstantBinding`/`LostFocusBinding` と対になる)。 |
| `CollapsableStackPanelTheme.xaml` | 折りたたみ/展開セクション用の `StackPanel` style (`Expanding`)。 |
| `GenericToggle.xaml` | カスタム [`ToggleSwitch`](../Controls/ToggleSwitch.cs) の既定の `ControlTemplate` と `Style`。サムのオフセットを [`ToggleSwitchOffsetConverter`](../Converters/ToggleSwitchOffsetConverter.cs) 経由で、状態ブラシを [`../Resources/ColorStyles.xaml`](../Resources/ColorStyles.xaml) から結線する。 |
