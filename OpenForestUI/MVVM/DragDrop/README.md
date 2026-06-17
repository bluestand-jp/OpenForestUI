> 🌐 **English** ・ [日本語](README-ja.md)

# ListView drag-and-drop helpers (vendored)

Vendored, third-party WPF drag-and-drop utilities for reordering `ListView` items. Used by the Ingame
roster UI ([`../ViewModel/IngameTeamsViewModel.cs`](../ViewModel/IngameTeamsViewModel.cs) and
[`../View/IngameTeamsView.xaml.cs`](../View/IngameTeamsView.xaml.cs)) to let the operator drag players
between/within team lists.

> **Provenance:** Copyright (C) Josh Smith, January 2007 (namespace `WPF.JoshSmith.*`).
> `MouseUtilities` is by Dan Crevier (Microsoft). This is third-party code, not original to OpenForestUI.

## Contents

- **`ListViewDragDropManager.cs`** — Generic `ListViewDragDropManager<ItemType>`; manages dragging and
  reordering `ListViewItem`s. Requires the `ListView.ItemsSource` to be an `ObservableCollection<ItemType>`.
- **`DragAdorner.cs`** — `Adorner` that renders the visual following the cursor during a drag.
- **`MouseUtilities.cs`** — P/Invoke (`GetCursorPos` / `ScreenToClient`) for reliable cursor coordinates
  during a drag operation, where WPF's own mechanisms are unreliable.
