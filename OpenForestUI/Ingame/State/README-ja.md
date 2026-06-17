> 🌐 [English](README.md) ・ **日本語**

# Ingame ゲーム状態エンジン (ライブ状態モデル + シリアライズ済みオーバーレイスナップショット)

このディレクトリは、ライブ/リプレイゲームのインメモリモデルと、ingame オーバーレイへシリアライズされるスナップショットを保持する。`State` はゲームごとのエンジンであり、Live Client Data (`/playerlist`、`/eventdata`)、Farsight のメモリ読み取り、OCR サイドカーを取り込み、チーム/オブジェクト/ゴールド/スコアボードの状態を維持する。`StateData` はその状態の JSON シリアライズ可能なビューで、各 `HeartbeatEvent` の内部でオーバーレイへ送られる ([`../Events/`](../Events/README-ja.md) を参照)。

## Contents

- **`State.cs`** (`State`) — コアエンジン。`IngameController.DoTick` によって毎ティック駆動される。責務:
  - `UpdateTeams` — `/playerlist` から `blueTeam`/`redTeam` を構築/更新し、プレイヤーごとのレベルとインベントリ ((`itemID`, `slot`) 単位) を差分して `LevelUp` / `ItemCompleted` イベントを発火する。Baron/Elder バフの失効も追跡する。
  - `UpdateEvents` — 累積された `/eventdata` バッチを消費し、`EventID` で重複排除し、タワー/インヒビターのキル、ヴォイドグラブ、Dragon/Baron/Herald の取得をクレジットする (ポート 34243 のレガシー LiveEvents API はパッチ 14.1 で削除されたため、`/eventdata` が現在唯一のオブジェクトドライバー)。
  - `ApplyHistoricalBaseline` — すでに進行中のゲームを観戦する際にタワー/ドラゴン数を再構築し、カウンターがゼロから始まらないようにする。
  - ゴールド差グラフ: `RecordGoldDiff` / `TrimGoldDiffHistory` / `GetGoldGraph` が青−赤のゴールド差の時系列を維持し (ロックガード付き、ダウンサンプリング)、サイドのゴールドグラフに使われる。
  - `UpdateScoreboard` — 上記すべてを `stateData.scoreboard` (キル、タワー、ゴールド、ドラゴン、グラブ、インヒブ、プレイヤーごとのロスター) に投影し、サイドカーがロックを持つ場合は OCR の正確値を優先する。
  - `ResetState` — ゲーム間ですべてをクリアする。`CreditTurretKill` / `CreditInhibKill` はレガシーの `T1/T2` と現行の `TOrder/TChaos` の両方の id 表記を受け付ける。
- **`StateData.cs`** (`StateData`) — シリアライズ済みオーバーレイスナップショット: オブジェクトタイマー (`dragon`/`baron`、`nextDragon`/`nextBaron`)、`gameTime`/`gamePaused`、青/赤のゴールド、ゴールドグラフ、インヒビター情報、`ScoreboardConfig`、オプションの info サイドページ、チームカラー。`ShouldSerialize*` メソッドは各フィールドをユーザーの ingame 機能トグル (`IngameController.CurrentSettings`) でゲートし、無効なコンポーネントをワイヤーペイロードから省く (また `infoPage` は空のときにドロップし、フロントエンドが「存在するが空」をデータとして扱うのを避ける)。
- **`ObjectiveSpawnClock.cs`** (`ObjectiveSpawnClock`) — Dragon/Baron/Herald の出現カウントダウンを毎ティック原理から導出する (`remaining = max(0, nextSpawnAt − gameTime)`、ここで `nextSpawnAt` はパッチの初回出現定数または最終キル + リスポーン)。このステートレスな再計算は、シードしたタイマーをデクリメントする脆弱な手法を置き換え、ゲーム途中参加とリプレイ巻き戻しを正しく扱う。Elder (ドレイク 4 体以上) もフラグ付けし、ポップアップパイプライン向けに出現のゼロクロスを発行する。

## Notes

- `State` は `internal`。オーバーレイは常に `StateData` 投影のみを見て、ライブエンジンそのものを見ることはない。
- `State.cs` の多くのコメントは「Phase N」のリプレイ/巻き戻し処理とメモリリーダーなし構成に言及している — このエンジンはライブ観戦とリプレイシークの両方で正確さを保つよう作られている。
