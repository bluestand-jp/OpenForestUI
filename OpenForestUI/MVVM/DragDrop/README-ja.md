> 🌐 [English](README.md) ・ **日本語**

# ListView ドラッグアンドドロップヘルパー (ベンダー取り込み)

`ListView` の項目を並べ替えるための、ベンダー取り込みのサードパーティ製 WPF ドラッグアンドドロップユーティリティ。
Ingame ロスター UI ([`../ViewModel/IngameTeamsViewModel.cs`](../ViewModel/IngameTeamsViewModel.cs) と
[`../View/IngameTeamsView.xaml.cs`](../View/IngameTeamsView.xaml.cs)) が使用し、オペレータがプレイヤーを
チームリスト間/内でドラッグできるようにする。

> **Provenance:** Copyright (C) Josh Smith, January 2007 (namespace `WPF.JoshSmith.*`)。
> `MouseUtilities` は Dan Crevier (Microsoft) によるもの。これはサードパーティのコードであり、OpenForestUI のオリジナルではない。

## Contents

- **`ListViewDragDropManager.cs`** — ジェネリックな `ListViewDragDropManager<ItemType>`。`ListViewItem` の
  ドラッグと並べ替えを管理する。`ListView.ItemsSource` が `ObservableCollection<ItemType>` であることを要求する。
- **`DragAdorner.cs`** — ドラッグ中にカーソルに追従するビジュアルを描画する `Adorner`。
- **`MouseUtilities.cs`** — ドラッグ操作中に確実なカーソル座標を得るための P/Invoke (`GetCursorPos` / `ScreenToClient`)。
  WPF 自身の仕組みが信頼できない場面で使う。
