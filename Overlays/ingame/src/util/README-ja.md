> 🌐 [English](README.md) ・ **日本語**

# オーバーレイユーティリティ（汎用ヘルパー）

シーンと visual 要素全体で共有される、小さく依存のないヘルパー: 整形、フォント読み込み、色変換、ジオメトリ、単純なデータ構造。

## 内容

| ファイル | 役割 |
| --- | --- |
| [`Utils.ts`](Utils.ts) | `ConvertGold`（ゴールド値を `"X.Yk"` として整形）。`LoadFont(name, url)` は `FontFace` をドキュメントへ登録する `Promise` を返す。 |
| [`TextUtils.ts`](TextUtils.ts) | `AutoSizeFont` は Phaser の `Text` のフォントサイズを幅 / 高さのボックスに収まるまで縮小する。 |
| [`ColorUtils.ts`](ColorUtils.ts) | `GetRGBAString(color, alpha)` は Phaser の色から CSS の `rgba(...)` 文字列を構築する。 |
| [`Vector2.ts`](Vector2.ts) | 位置 / サイズ全般で使う 2D ベクトル型。大きさ / 正規化 / 逆ベクトルのヘルパーと静的 `add`/`mul`/`dot` を持つ。 |
| [`Queue.ts`](Queue.ts) | 汎用 FIFO `Queue<T>`（head/tail インデックスベース）。`RegionMask` のスロットごとのアニメーションキューを支える。 |
| [`Dictionary.ts`](Dictionary.ts) | `add`/`remove`/`get`/`keys`/`values`/`containsKey` を持つ汎用の文字列キー `Dictionary<T>`。 |
