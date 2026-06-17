> 🌐 [English](README.md) ・ **日本語**

# 永続化される JSON 設定 (スキーマ + ローダー)

アプリの設定層。ディスク上の `Config/*.json` ファイルへ/からシリアライズされる強く型付けされた config モデルと、それらを読み込み・バージョン移行・書き込みするプロバイダーを定義する。[`../../Controllers`](../../Controllers/README-ja.md) の `ConfigController` がライブインスタンスを保持し、`FileSystemWatcher` 経由でホットリロードする。

## 内容

| ファイル | 役割 |
|------|------|
| [`JSONConfig.cs`](JSONConfig.cs) | すべての config ファイルの抽象基底。契約 — `Name`、`FileVersion`、serialize/deserialize、`RevertToDefault`、`UpdateConfigVersion` — に加え、`Reload()` とホットリロード購読者向けの `ConfigUpdate` イベントを宣言する。 |
| [`JSONConfigProvider.cs`](JSONConfigProvider.cs) | `./Config` 配下で config ファイルを読み書きするシングルトン (必要に応じてフォルダと `Teams/` サブフォルダを作成)。ファイルが欠落/空/破損していれば既定値を復元し、`FileVersion` が古い場合はバージョン移行を実行する。チームごとの config ファイル (`ReadTeam`/`WriteTeam`) も扱う。 |
| [`ComponentConfig.cs`](ComponentConfig.cs) | メインの `Component.json` スキーマ (現行バージョン `1.6`)。ネストされたセクション: `DataDragon`、`PickBan`、`Ingame` (objectives、team-info トグル、`ObjectiveTimingsConfig` のパッチタイミング、`UseMemoryReader`、`TournamentName`)、`Replay`、`PostGame`、`App` (ログレベル、frontend ポート、League インストールパス)。新規インストール時の既定値を保持する。 |
| [`ExtendedTeamConfig.cs`](ExtendedTeamConfig.cs) | チームごとに保存される config (`Config/Teams/<TeamName>.json`): `TeamConfig` にチームのアイコン位置を加えたもの。Saved Teams 機能で使われる。"default" を持たないため `RevertToDefault`/`GETDefaultString` は throw する。 |

## 補足

- `ObjectiveTimingsConfig` はオブジェクトのスポーン/リスポーン時刻 (ゲーム時間の秒) を、コードではなく **パッチデータ** として保持する。そのためシーズンの再調整は `Component.json` の編集で済む。既定値はパッチ 26.x に追従する。
- `IngameComponentConfig.UseMemoryReader` は既定で `false` (Vanguard セーフ)。オフセット (`Farsight.json`) はそれがオンのときだけロードされる。
