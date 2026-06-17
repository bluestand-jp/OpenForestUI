> 🌐 [English](README.md) ・ **日本語**

# オーバーレイ背景プレートと動画バッキング

ingame overlay パネル用の背景アート。各パネルは通常、静的 `.png` とループ `.mp4` の両方で提供される。ビジュアル要素は config が `UseVideo` を設定していれば動画を、そうでなければ画像を選ぶ（[`../../src/visual`](../../src/visual) の `ScoreboardVisual`、`GraphVisual`、`InhibitorVisual`、`InfoPageVisual` を参照）。`frontend/backgrounds/...` として読み込まれる。

## 内容

- `Score.png` / `Score.mp4` — スコアボードパネルのバッキング（`ScoreboardVisual`）。
- `GoldGraph.png` / `GoldGraph.mp4` — gold/グラフパネルのバッキング（`GraphVisual`）。
- `Inhibitor.png` / `Inhibitor.mp4` — インヒビタータイマーパネルのバッキング（`InhibitorVisual`）。
- `InfoPage.png` / `InfoPage.mp4` — インフォページのバッキング（`InfoPageVisual`）。
- `BaronIcon.png`、`DragonIcon.png` — トップのオブジェクティブバー上のオブジェクティブアイコン。
- `BaronTimer.png`、`DragonTimer.png` — オブジェクティブのリスポーンタイマープレート。
- `ObjectiveBG.png`、`ObjectiveBGLeft.png` — オブジェクティブバーのバッキング。
- `ScoreTeamIconBGLeft.png`、`ScoreTeamIconBGRight.png` — チームごとのスコアボードロゴバッキング。
- `ItemText.png` — アイテムテキストパネルのバッキング。
