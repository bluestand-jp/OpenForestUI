> 🌐 [English](README.md) ・ **日本語**

# Objective（Baron / Dragon）モデル

Baron および Dragon の「power play」パネル — 主要オブジェクトが取られた後に出現し、それが有効な間にチームが獲得する gold を追跡するパネル — を支えるデータ。`BackEndObjective` はステートマシンが維持する内部アキュムレータ、`FrontEndObjective` はオーバーレイへ送出する切り詰めた形状である。

## 内容

- [`Objective.cs`](Objective.cs) — `ObjectiveType` enum（`None = -1`、`Baron = 0`、`Dragon = 1`）。
- [`BackEndObjective.cs`](BackEndObjective.cs) — アクティブなオブジェクトのバックエンド管理情報。チーム単位の開始 gold（`BlueStartGold`/`RedStartGold`）、`DurationRemaining`、`TakeGameTime`。オブジェクトの存続期間にわたる power-play の gold 差を計算するために使う。
- [`FrontEndObjective.cs`](FrontEndObjective.cs) — オーバーレイ ペイロード。整形済みの `DurationRemaining`（"MM:SS"）、`Type`、`GoldDifference`、`SpawnTimer`。
- [`UpcomingObjective.cs`](UpcomingObjective.cs) — 次回 Dragon/Baron カウントダウン用の保留中スポーン（`Element` 名 + `SpawnTimer`）。
