> 🌐 [English](README.md) ・ **日本語**

# ユーティリティ拡張 & ヘルパー

ソリューション全体で再利用される汎用かつ依存のないヘルパー: Farsight メモリリーダー向けのバイトバッファ拡張、カスタム JSON コンバーター、固定容量のリングバッファ、比較可能なバージョン型。`StringVersion.cs` は [GoldDiff](https://github.com/Johannes-Schneider/GoldDiff)（MIT）から流用している。

## 構成内容

- `ByteUtils.cs` — 生バッファからプリミティブを読み取る `byte[]` 拡張メソッド（`ToIntPtr`/`ToInt`/`ToUInt`/`ToShort`/`ToFloat`）、サブ配列の抽出、ASCII チェック/デコード、インプレース書き込み。Farsight メモリリーダーのコアヘルパー。
- `CircularBuffer.cs` — 汎用の固定容量 `CircularBuffer<T>`（`PushBack`/`PushFront`、インデクサ、`IEnumerable<T>`）。満杯時には最古の要素を上書きする。インターフェースは Boost の circular_buffer に着想を得ている。
- `StringVersion.cs` — `IEquatable`/比較可能なドット区切りバージョン型（Major/Minor/Patch）。`Parse`/`TryParse`、呼び出し元アセンブリのバージョンアクセサ、JSON 用の同梱 `StringVersionConverter` を備える。*（GoldDiff から流用。）*
- `NumberToStringJsonConverter.cs` — JSON 数値を `string` として読み取る（書き戻しは数値として行う）`System.Text.Json` コンバーター。`SummonerSpell.ID` で使われる。
- `HexStringJsonConverter.cs` — `int` を `0x…` 16 進文字列としてシリアライズし、パースして戻す Newtonsoft コンバーター（config 内のメモリオフセットに使われる）。
- `StringUtils.cs` — 部分文字列の全インデックスを返す `AllIndexesOf(value)` 文字列拡張。
- `DictionaryUtils.cs` — `KeyByValue(val)` 逆引き: 値が `val` と等しい最初のキーを返す。
