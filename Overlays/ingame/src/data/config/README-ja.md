> 🌐 [English](README.md) ・ **日本語**

# オーバーレイ設定スキーマ（OverlayConfig DTO）

バックエンドが接続ごとに一度プッシュする `OverlayConfig`（イベント `OverlayConfig`、type 1）の TypeScript インターフェース定義。全 visual のレイアウト、フォント、色、アニメーション、要素ごとのトグルを記述する。`IngameScene.UpdateConfig` がこれを消費して visual 要素を生成 / 更新する。

## 内容

- [`overlayConfig.ts`](overlayConfig.ts) — スキーマ全体を保持する 1 ファイル:
  - `OverlayConfigEvent` — WS エンベロープ（`type`、`eventType`、`config`）。
  - `OverlayConfig` — ルート: `Inhib`、`Score`、`ObjectiveKill`/`ObjectiveSpawn`、`ItemComplete`、`LevelUp`、`InfoPage`、`BaronPowerPlay`/`ElderPowerPlay`、`DragonTimer`/`BaronTimer`、`GoldGraph`、`GoogleFonts`、そしてオプトインの `PrmScore`。
  - `PrmScoreConfig` — 放送トップバーのオプトイン（`Enabled`、`Font`、`TournamentName`、`BottomBar`、`BottomStyle: 'prm' | 'lck'`、`DDragonVersion`）。
  - 要素ごとの表示設定: `ScoreDisplayConfig`（+ `TeamConfig`、`ElementConfig`、`DrakeConfig` など）、`InhibitorDisplayConfig`、`PowerPlayDisplayConfig`、`ObjectiveTimerDisplayConfig`、`ObjectiveKill`/`SpawnConfig`、`ScoreboardPopUpConfig`、`ItemCompletedDisplayConfig`、`LevelUpDisplayConfig`、`InfoPageDisplayConfig`、`GoldGraphDisplayConfig`、共有の `FontConfig` / `VisualElementAnimationConfig`。

特記: `GoldGraph.Enabled` と `PrmScore.*` はオプションであり、Vanguard 互換フォーク下ではオフ / undefined がデフォルトとなる（ライブ API からプレイヤーごとの `totalGold` が得られないため）。そのためシーンは不在を「無効」とみなす。
