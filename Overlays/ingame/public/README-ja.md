> 🌐 [English](README.md) ・ **日本語**

# Ingame overlay 静的アセット

OpenForestUI の組み込み HTTP サーバーが `/frontend` で配信し、Phaser 3 の ingame overlay（`Overlays/ingame/src`）が読み込む静的ファイル。アセットパスはコードから `frontend/<path>` として参照される（例: [`scenes/IngameScene.ts`](../src/scenes/IngameScene.ts) でプリロード）。スコアボード・インフォページ・オブジェクティブバーの描画に使う背景プレート、オブジェクティブ/ドラゴンアイコン、レーングリフ、ポップアップ動画、アルファマスク、放送用テーマアートを保持する。

## サブディレクトリ

| Dir | 用途 |
| --- | --- |
| [`backgrounds/`](backgrounds/README-ja.md) | スコアボード・インフォページ・グラフ・インヒビター・オブジェクティブバー用の背景プレートとループ動画バッキング |
| [`images/`](images/README-ja.md) | 汎用オーバーレイアイコン（オブジェクティブ、タワー、セパレーター）とテーマ別アイコンサブツリー |
| [`masks/`](masks/README-ja.md) | オーバーレイ面をクリップするアルファ/ビットマップマスク（チャンプカバー、アイテムテキスト、グラフ、インフォページ） |
