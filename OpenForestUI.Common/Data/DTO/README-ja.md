> 🌐 [English](README.md) ・ **日本語**

# チャンピオン & サモナースペル DTO

チャンピオンとサモナースペルのためのデータ転送オブジェクト。各ファイルは Community Dragon のソース型（アプリの `DataDragon` プロバイダーが CDragon JSON からデシリアライズし、静的な `All` セットに保持）と、オーバーレイへ送られる/内部で使われるよりスリムな形をペアで持つ。

## 構成内容

- `Champion.cs`
  - `CDragonChampion` — Community Dragon からパースされたチャンピオンエントリ（`id`、`alias`、`name`、スプラッシュ/ローディング/スクエア画像パス）。静的な `All` セットがロード済みの全チャンピオンを保持する。
  - `Champion` — 軽量なアプリ側チャンピオン DTO（`id`、`key`、`name`、画像フィールド）。
- `SummonerSpell.cs`
  - `SummonerSpell` — CDragon からパースされたサモナースペル。`id` は [`NumberToStringJsonConverter`](../../Utils/NumberToStringJsonConverter.cs) により JSON 数値から型強制される。静的な `All` セットを持ち、`AsFrontEndSummonerSpell()` がオーバーレイ向けの形へ射影する。
  - `FrontEndSummonerSpell` — フロントエンドオーバーレイへシリアライズされるフラットな DTO（`id`/`key`/`name`/`icon`/`iconPath`）。
