> 🌐 [English](README.md) ・ **日本語**

# オーバーレイシーン（WebSocket クライアント + visual オーケストレーター）

インゲームオーバーレイを駆動する単一の Phaser シーン。`main.ts` に登録され、バックエンドと visual 要素の間の中央コーディネーターとなる。

## 内容

- [`IngameScene.ts`](IngameScene.ts) — 唯一のシーン。責務:
  - **アセット読み込み**（`preload`）: マスク、オブジェクト / dragon / タイマーのアイコン、スコアボードと放送トップバーのアイコン、レーン SVG、アイテム背景、Chart.js（ゴールドグラフ用）。すべての相対読み込みはサイトルートを基点とし、`/frontend/...` 配下で解決される。
  - **WebSocket ライフサイクル**（`create` → `connect`）: `ws://host:9001/api` を開き、`Ingame` の `OverlayConfig` を要求し、クローズ時に自動再接続し、各メッセージで `eventType`（`GameHeartbeat`、`OverlayConfig`、`PlayerLevelUp`、`ObjectiveKilled`/`Spawn`、`ItemCompleted`、`GameEnd`、`ForceRefresh` …）によりディスパッチする。
  - **設定の結線**（`UpdateConfig` / `UpdateConfigWhenReady`）: Google + ローカルフォントを読み込み、続いて全 visual 要素を生成 / 更新する。レガシーの `ScoreboardVisual` とオプトインの放送トップバー（`PrmScoreboardVisual`）を選択し、`PrmScore.BottomBar` がオンのときは（`BottomStyle` により）下部の比較スコアボードのバリアントを選択する。ゴールドグラフ（`GraphVisual`）はオプトイン / 遅延生成。
  - **状態のファンアウト**（`OnNewState`）: 各ハートビートを `StateData` でラップし、インヒビター、スコアボード / トップバー、下部バー、レガシーのオブジェクトタイマー / Power Play、情報ページ、ゴールドグラフへ供給する。スコアボードのマスタートグルゲート（`GameTime` 不在 ⇒ スコアボードオフ → 該当ボードを非表示）とソウルポイント検出（`CheckSoulPoint`）を実装する。
  - プレイヤーごとのレベルアップ / アイテムポップアップアニメーションをクリップするために使う 11 個の `RegionMask`（プレイヤースロットごと + グローバル地域 1 つ）を保持する。
