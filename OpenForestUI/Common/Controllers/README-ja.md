> 🌐 [English](README.md) ・ **日本語**

# アプリ controllers (ゲームライフサイクル、データ取り込み、配信)

OpenForestUI デスクトップアプリのオーケストレーション層。これらのシングルトンは League Client とゲームプロセスを監視し、あらゆるソース (LCU、観戦用 Live Client Data API、Replay API、Farsight メモリリーダー、Python OCR サイドカー) からデータを取得し、オーバーレイ状態を構築し、組み込みの WebSocket サーバー経由でブラウザソースのオーバーレイへ送出する。大半は起動時に `BroadcastController` が生成・結線する。

## ライフサイクルモデル

- **`BroadcastController`** がルート。コンストラクタは `EarlyInit` -> (DataDragon ロード) -> `Init` (全 controller、HTTP/WS サーバー、tick タイマーの構築) -> `PostInit` (DI 経由でメインウィンドウを開き、ticking を開始) の順に実行する。`ToTick` リストと `~2 tps` タイマー (`TickRate`) を保持し、登録された各 `ITickable` に対して `DoTick()` を呼び出す。`[Flags] LeagueState` (Connected / ChampSelect / InProgress / PostGame) と、他の全 controller への `Instance` シングルトンアクセサを保持する。
- **`ITickable`** は、ticked される各 controller が実装する単一メソッドのインターフェース (`DoTick()`)。
- チャンピオンセレクトとインゲームは `AppStateController.Enable/Disable*` を通じてオン/オフされ、該当 controller を LCU/プロセスイベントへ購読登録する。

## 内容

| ファイル | 役割 |
|------|------|
| [`BroadcastController.cs`](BroadcastController.cs) | アプリルート: 起動シーケンス、controller 結線、tick タイマー + `ToTick` リスト、`LeagueState` フラグ。 |
| [`ITickable.cs`](ITickable.cs) | 中央 tick ループで駆動されるものすべてのためのインターフェース (`DoTick()`)。 |
| [`AppStateController.cs`](AppStateController.cs) | LCU (`LeagueClientApi`) に接続し、gameflow / champ-select イベントを購読し、サモナーをキャッシュし、ローカルパッチ用の Farsight オフセットをロードし、毎 tick タイトルバーの接続ステータスチップ (Mocking / Disconnected / Connected / LCU) を整合させる。 |
| [`ConfigController.cs`](ConfigController.cs) | すべての JSON 設定 (`Component`、`PickBan`、`Ingame`、`Farsight`) をロード/保存し、静的な config シングルトンを保持し、ホットリロード用にファイルごとの `ConfigWatcher` (FileSystemWatcher) を実行する。 |
| [`PickBanController.cs`](PickBanController.cs) | チャンピオンセレクト (Pre Game) ドライバー: LCU の champ-select タイマーを tick し、LCU セッション状態を変換し、pick/ban 状態 + ハートビートを `pickban` オーバーレイへ配信する。 |
| [`IngameController.cs`](IngameController.cs) | 最重要。毎 tick のインゲームパイプライン: ゲーム/プレイヤー/イベントデータを取得し、再生モード (Spectator / Replay / Live) を判定し、オブジェクトのスポーン時計を導出し、replay-seek ロールバックを処理し、OCR の gold/CS/objectives を注入し、gold グラフをサンプリングし、スコアボードを構築し、`GameHeartbeat` を配信する。`CurrentSettings` (ランタイムのオーバーレイ機能ゲート) も定義する。 |
| [`OcrGoldController.cs`](OcrGoldController.cs) | Python OCR サイドカー (`ocr-poc/goldcap.py --live`) を起動・読み取り、その tri-state JSON (Known/Stale/Unknown) をパースし、ゲートされた正確な gold / CS / objective 数 / replay tower 数を `Team` オブジェクトへ注入する。`/debug-ocr` 診断のバックエンド。 |
| [`ReplayAPIController.cs`](ReplayAPIController.cs) | ローカルの Replay API (`https://127.0.0.1:2999/replay/render`) と通信し、クリーンなレンダリングのためにインゲーム HUD スコアボードをトグルする。任意で `GameInputController.InitUI` をトリガーする。 |
| [`GameInputController.cs`](GameInputController.cs) | ゲーム開始時にオブザーバー HUD を初期化するため、League ウィンドウへ合成キー入力 (`InputUtils` 経由) を送る (`Replay.UseAutoInitUI` の背後でゲート)。 |
| [`MockController.cs`](MockController.cs) | 開発/プレビュー用ヘルパー。`overlay-harness` フィクスチャから組んだ既定の `GameHeartbeat` を配信し、ライブゲームなしでオーバーレイをプレビュー可能にする。ライブチーム Details をオーバーレイし、Ingame > Teams の `ShouldSerialize*` 機能ゲートをミラーする。有効時はライブフィードより優先される。 |

## 補足

- このフォークは Vanguard 互換性のためメモリリーダーを既定で **オフ** (`UseMemoryReader = false`) にする。`FarsightController.ShouldRun` が全メモリリーダー経路をゲートする。観戦 API + OCR サイドカーが主要なデータソース。
- `IngameController` 内の `Broadcast(...)` は全ライブオーバーレイイベントの唯一の出口で、Mock 有効時は沈黙を保つため、ライブとモックのフィードが混ざることはない。
