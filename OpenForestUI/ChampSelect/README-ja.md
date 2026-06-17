> 🌐 [English](README.md) ・ **日本語**

# チャンピオンセレクトのパイプライン (LCU draft -> overlay)

この名前空間は OpenForestUI のチャンピオンセレクト機能である。**League Client (LCU) API** から WebSocket 経由でライブのドラフトデータを取り込み、安定したブロードキャストモデルへ正規化し、pick/ban/timer/state の更新を `pickban` ブラウザソースオーバーレイ (RCVolus の `lol-pick-ban-ui` を C# 統合したもの) へ送り出す。

このフローは `Common/Controllers/PickBanController.cs` が駆動する。LCU WebSocket イベントが生の `Session` として到着し、`StateInfo/Converter` がそれを平坦化してクリーンな `Team`/timer/state の値へ変換し、`StateInfo/State` がそれらを差分検出して共有の `StateData` に格納し、結果はここで定義された `Events` としてオーバーレイへブロードキャストされる (`ws://localhost:9001/api` 上でシリアライズ)。コントローラはさらに、ティックごとのタイマーリフレッシュ (LCU はフェーズ変更時にしか発火しない) と、現在の `PickBanConfig` を運ぶ 10 秒間隔の `heartbeat` も実行する。

## サブディレクトリ

| Directory | 目的 |
| --- | --- |
| [`Data/`](Data/README-ja.md) | ドラフトデータモデル: 生の LCU 入力 DTO、正規化されたオーバーレイ DTO、config |
| [`Events/`](Events/README-ja.md) | pickban オーバーレイへ送る送信用 WebSocket イベントペイロード |
| [`StateInfo/`](StateInfo/README-ja.md) | 現在のドラフト状態ストア、LCU->overlay コンバータ、現在アクションのロジック |

## 全体の組み合わさり方

1. **入力:** LCU `champSelect` セッション JSON -> `Data/LCU/Session` (および `Cell`、`Action`、`Timer`)。
2. **変換:** `StateInfo/Converter` がアクショングループを平坦化し、正規化された `Data/DTO/Team` (`Pick`/`Ban`/`Champion`)、カウントダウンタイマー、フェーズ名を構築する。
3. **格納/差分:** `StateInfo/State` がシングルトン `StateData` を更新し、`StateUpdate` / `NewAction` / start / end の各ハンドラを発火する。
4. **出力:** `Events/*` (`newState`、`newAction`、`champSelectStart`、`champSelectEnd`、`heartbeat`) としてラップされ、オーバーレイへ送られる。
