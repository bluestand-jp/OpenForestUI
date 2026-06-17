> 🌐 [English](README.md) ・ **日本語**

# 静的ゲームアセットプロバイダー (Data Dragon / Community Dragon)

オーバーレイが必要とする静的な League アセット — チャンピオンスクエア、アイテムとサモナースペルのアイコン、現行ゲームバージョン — を Riot の Data Dragon と Community Dragon の CDN から取得・キャッシュする。起動の早い段階で実行され、アプリの残りは controllers の構築前に `DataDragon.FinishLoading` を待つ。

## 内容

| ファイル | 役割 |
|------|------|
| [`DataDragon.cs`](DataDragon.cs) | シングルトンのアセットプロバイダー。最新ゲームバージョンを解決し、ローカルの `./Cache/<version>/` を CDN と照合し、欠落しているチャンピオン/アイテム/サモナースペルアセットをダウンロードし (進捗は `FileDownloadComplete` 経由で起動スプラッシュへ報告)、解決済みの `GameVersion` (CDN URL、パッチ) を公開する。`Extend*` ヘルパーは Community Dragon のチャンピオン/アイテム/スペルレコードをキャッシュされたローカルパスで補強する。 |

## 補足

- キャッシュは実行ファイルの隣の `./Cache/<version>/` 配下に置かれる。CDN エンドポイントは `ComponentConfig.DataDragonConfig` (CDN、CDragonRaw、locale、patch) から取得する。`DataDragon.version` は、LCU のパッチ文字列が得られないときにアクティブなパッチを示す唯一の信頼できる情報源。
