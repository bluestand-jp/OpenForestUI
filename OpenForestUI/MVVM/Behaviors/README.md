> 🌐 **English** ・ [日本語](README-ja.md)

# Attached behaviors (code-behind replacements)

Reusable WPF attached behaviors that let XAML wire up interaction logic declaratively instead of
through code-behind event handlers — part of the MVVM-first refactor of the control-center shell.

## Contents

- **`WindowDragBehavior.cs`** — Attached `EnableDrag` bool property. When set on the custom title-bar
  element it hooks `MouseLeftButtonDown` and calls `Window.DragMove()`, so the borderless shell window
  can be dragged. Replaces the old `Grid.MouseDown -> DragMove()` code-behind handler; child buttons
  (minimize / close / status) mark the event handled so the drag only fires on the empty title-bar area.
  Usage: `behaviors:WindowDragBehavior.EnableDrag="True"`.
