> 🌐 [English](README.md) ・ **日本語**

# アプリデータ層 (設定 + アセットプロバイダー)

デスクトップアプリのデータ関連事項をまとめる: 強く型付けされた JSON 設定モデルとそのローダー、および Data Dragon アセットをダウンロード/キャッシュする静的ゲームアセットプロバイダー。[`../`](../README-ja.md) 配下の controllers と view-models 全体から利用される。

## サブディレクトリ

| ディレクトリ | 目的 |
|-----------|---------|
| [`Config/`](Config/README-ja.md) | JSON 設定スキーマ (`Component`、team configs) + それらを読み込み/移行/書き込みするプロバイダー。 |
| [`Provider/`](Provider/README-ja.md) | Data Dragon / Community Dragon 静的アセットのダウンローダーとキャッシュ (`DataDragon`)。 |
