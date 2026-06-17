> 🌐 [English](README.md) ・ **日本語**

# Pickban オーバーレイ設定

チャンピオンセレクトオーバーレイ向けの、永続化されユーザーが編集可能な設定。`PickBanConfig` は `JSONConfig` であり (`OpenForestUI.Common` の config システムにより保存/読み込みされ、ファイル名は `PickBan`、バージョンは `1.0`)、`StateData.config` と定期的な `heartbeat` イベントを通じてオーバーレイへ公開される。ブロードキャストの表示設定を保持する。どのチームがどちらか、名前/スコア/色、機能トグル、そしてオプションのブロードキャスト トップバーメタデータである。

## 内容

| File | 役割 |
| --- | --- |
| `PickBanConfig.cs` | ルート config オブジェクト。serialize/default/version 処理を持つ `JSONConfig`。単一の `FrontendConfig` をラップする。 |
| `FrontendConfig.cs` | オーバーレイの表示設定: `scoreEnabled`、`spellsEnabled`、`coachesEnabled`、`blueTeam`/`redTeam` (`TeamConfig`)、`patch`。`CreateDefaultConfig()` で妥当なデフォルトを提供する。 |
| `TeamConfig.cs` | チームごとのブロードキャストメタデータ: `name`、`nameTag`、`score`、`coach`、`color`、加えてオプションのブロードキャストフィールド `region` (リーグバッジ)、`seed` (順位)、`flag` (ISO 国コード)。 |
