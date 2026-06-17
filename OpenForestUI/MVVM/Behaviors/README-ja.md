> 🌐 [English](README.md) ・ **日本語**

# Attached behavior (code-behind の置き換え)

XAML がインタラクションロジックを code-behind のイベントハンドラ経由ではなく宣言的に結線できるようにする、
再利用可能な WPF attached behavior 群。コントロールセンターシェルの MVVM ファースト化リファクタの一部である。

## Contents

- **`WindowDragBehavior.cs`** — attached の `EnableDrag` bool プロパティ。カスタムタイトルバー要素に設定すると
  `MouseLeftButtonDown` をフックして `Window.DragMove()` を呼び出すため、ボーダーレスのシェルウィンドウを
  ドラッグできる。旧来の `Grid.MouseDown -> DragMove()` の code-behind ハンドラを置き換える。子ボタン
  (最小化 / 閉じる / ステータス) はイベントを handled としてマークするため、ドラッグは空のタイトルバー領域でのみ発火する。
  使用例: `behaviors:WindowDragBehavior.EnableDrag="True"`。
