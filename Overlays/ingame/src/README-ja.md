> 🌐 [English](README.md) ・ **日本語**

# インゲームオーバーレイのソース（Phaser 3 / TypeScript）

インゲーム用ブラウザソースの全 TypeScript。`main.ts` が単一の Phaser WebGL ゲームを 1 つのシーン（`IngameScene`）とともに起動する。このシーンはバックエンドへの WebSocket 接続を保持し、visual 要素を生成し、プッシュされた各状態 / イベントを対応する visual へルーティングする。各モジュールは `~/` パスエイリアス（`tsconfig.json` で設定）を使って `src/` 配下のファイルを参照する。

## 内容

| ファイル | 役割 |
| --- | --- |
| [`main.ts`](main.ts) | エントリーポイント。`Phaser.Game` 設定（1920×1080、透過、WebGL）を構築し、rexUI + WebFontLoader プラグインを登録し、`IngameScene` を開始する。 |
| [`index.html`](index.html) / [`style.css`](style.css) | ホストページ（`#gameContainer` div）と全画面透過キャンバスのスタイル。 |
| [`variables.ts`](variables.ts) | 静的設定: バックエンドのホスト / ポート（`localhost:9001`）、WS パス（`api`）、SSL フラグ、フォールバックのチームカラー、ゴールドカラー定数。 |
| [`PlaceholderConversion.ts`](PlaceholderConversion.ts) | バックエンドの `cache/...` アセットプレースホルダを絶対 URL `http(s)://host:port/cache/...` に書き換える。すでに絶対 URL のもの（DataDragon、チームロゴ）はそのまま通す。 |

## サブディレクトリ

| ディレクトリ | 役割 |
| --- | --- |
| [`scenes/`](scenes/README-ja.md) | 単一の Phaser シーン（`IngameScene`）— WebSocket クライアント、config / イベントルーター、visual オーケストレーター。 |
| [`visual/`](visual/README-ja.md) | 画面要素ごとに 1 クラス（スコアボード、タイマー、グラフ、ポップアップ…）。すべて共通の `VisualElement` 基底を継承。 |
| [`data/`](data/README-ja.md) | バックエンドがプッシュする JSON の TypeScript ミラー（`StateData`、スコアボード / チーム / オブジェクト / インヒビターのモデル）。 |
| [`data/config/`](data/config/README-ja.md) | 要素ごとの表示 / レイアウト設定を記述する `OverlayConfig` インターフェース群。 |
| [`convert/`](convert/README-ja.md) | 小さなブラウザ向けヘルパー（例: `?backend=` クエリ変数の読み取り）。 |
| [`util/`](util/README-ja.md) | 汎用ヘルパー: ゴールド / テキスト整形、フォント読み込み、色、`Vector2`、`Queue`、`Dictionary`。 |
