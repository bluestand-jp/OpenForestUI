> 🌐 [English](README.md) ・ **日本語**

# オーバーレイデータモデル（バックエンド状態 DTO）

OpenForestUI バックエンドが WebSocket 越しにプッシュする JSON の TypeScript ミラー。`StateData` は `GameHeartbeat` ごとのペイロードであり、visual 要素は生の JSON ではなくこれらの型付きオブジェクトを読む。各クラスはパース済みの `message` から構築され、フィールドをそのままマッピングする（軽微な型強制 / デフォルトあり）。

## 内容

| ファイル | 役割 |
| --- | --- |
| [`stateData.ts`](stateData.ts) | トップレベルのハートビートペイロード: dragon/baron オブジェクト、次スポーンのオブジェクト、ゲーム時間 / ポーズ、チームゴールド、ゴールドグラフ系列、インヒビター、スコアボード、情報ページ、チームカラー。 |
| [`scoreboardConfig.ts`](scoreboardConfig.ts) | スコアボード状態: blue/red の `FrontEndTeam`、`GameTime`、シリーズのゲーム数、トーナメント名、プレイヤーごとの `PlayerScoreboardEntry[]`。 |
| [`frontEndTeam.ts`](frontEndTeam.ts) | 1 チームの表示スタット: 名前 / アイコン / スコア、kills/towers/gold、型付き dragon リスト、加えて放送トップバー用の追加項目（void grubs、baron/dragon 数、インヒビター、プレート、地域 / シード / フラグ）。 |
| [`playerScoreboardEntry.ts`](playerScoreboardEntry.ts) | 下部の比較スコアボード用の 1 プレイヤー行: チーム / ポジション / 名前、チャンピオン + スペルキー、レベル、KDA、CS、ゴールド、アイテム ID。 |
| [`frontEndObjective.ts`](frontEndObjective.ts) | オブジェクトの表示状態: 基となる `Objective`、残り時間文字列、丸めたゴールド差、スポーンタイマー。 |
| [`objective.ts`](objective.ts) | オブジェクトの中核事実: クールダウン、生存フラグ、取得回数、最後の取得者、種別。 |
| [`upcomingObjective.ts`](upcomingObjective.ts) | 保留中のオブジェクト: 要素名 + スポーンタイマー（スポーンタイマーウィジェットを駆動）。 |
| [`inhibitor.ts`](inhibitor.ts) | `Inhibitor`（id/key/残り時間）と `InhibitorInfo`（リスト + マップ `Location`）。 |
| [`goldEntry.ts`](goldEntry.ts) | ゴールド差グラフ用の単一 `{x: time, y: gold}` 点。 |
| [`infoSidePage.ts`](infoSidePage.ts) | 情報タブのサイドページ: タイトル、`PlayerOrder` enum、`PlayerInfoTab[]`。null セーフ（タブにデータが無いときバックエンドは `null` を送る）。 |
| [`playerInfoTab.ts`](playerInfoTab.ts) | 情報ページタブの 1 エントリ: プレイヤー名、アイコンパス、`ValueBar`、追加情報文字列。 |
| [`ValueBar.ts`](ValueBar.ts) | 情報ページのプログレスバー用の最小 / 現在 / 最大の数値トリプル。 |
| [`RegionMask.ts`](RegionMask.ts) | プレイヤースロットの Phaser ビットマップマスクを地域ごとのアニメーション `Queue` と対にし、同一スロットのポップアップを 1 つずつ再生させる。 |

## サブディレクトリ

| ディレクトリ | 役割 |
| --- | --- |
| [`config/`](config/README-ja.md) | `OverlayConfig` インターフェース群（接続時に一度送られる要素ごとのレイアウト / 表示設定）。 |
