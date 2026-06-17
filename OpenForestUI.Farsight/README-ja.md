> 🌐 [English](README.md) ・ **日本語**

# Farsight — League of Legends メモリリーダー

`OpenForestUI.Farsight` はインプロセスのメモリ読み取りライブラリである。動作中の
League of Legends クライアントにアタッチし、ゲームの **ObjectManager** をたどってユニットごとの
値を読み取る。観戦用の Live Client Data API（port 2999）が丸めたり省いたりする値、すなわち
正確な gold、XP、レベル、全チャンピオンの座標、加えてタワーやエピックモンスターを取得する。

これは Vanguard 互換性のため **オプトインかつデフォルト無効**（`FarsightController.ShouldRun = false`）
である。Riot のアンチチートが有効な状態では、外部プロセスはクライアントのモジュールを列挙できず
アタッチがクリーンに失敗するため、アプリは API 専用モードにフォールバックする。
このフラグは、ユーザーが `UseMemoryReader` を有効にしたときにのみ `ConfigController`／
`BroadcastController` によってオンに切り替えられる。なお、現行の Phase-3 ingame パイプラインでは
有効時にスナップショットは依然として生成されるが、その出力はもはや broadcast State へは
配線されていない点に注意。

## 仕組み

1. `BroadcastController` が `FarsightController` を構築し `Connect(process)` を呼ぶ。これにより
   League プロセスが `Memory.Initialize` に渡される。
2. フレームごとに `CreateSnapshot()` が ObjectManager ルートを読み取り、オブジェクトツリーを
   幅優先で走査し（オフセット 0/8/16 の red-black 風ノードトリプレット）、NetID 範囲で
   フィルタし、各ライブオブジェクトポインタをデリファレンスする。
3. すべてのポインタは `GameObject` にハイドレートされ（`LoadFromMemory`）、`Snapshot` の
   バケット（champions／turrets／dragon／baron／herald）に分類される。次のドラゴンタイプは
   `Dragon_Indicator_*` の表示名から推測される。

すべての struct／field オフセットは **パッチ固有** であり、ここにはハードコードされていない。
ランタイムに `Config/Farsight.json` から読み込まれ（`OpenForestUI/Ingame/Data/Config/FarsightConfig.cs`
でデシリアライズ）、`FarsightController.GameOffsets`（ObjectManager ツリーレイアウト）と
`GameObject.Offsets`（ユニットごとのフィールドレイアウト）に格納される。League パッチ後に
更新する方法は下記の [オフセットのリバースエンジニアリング](#オフセットのリバースエンジニアリングパッチ後の更新) を参照。

## 内容

| File | 役割 |
| --- | --- |
| [`FarsightController.cs`](FarsightController.cs) | エントリポイント。`ShouldRun` オプトインフラグを保持し、プロセスに接続し、`CreateSnapshot` で ObjectManager ツリーを走査し、オブジェクトを分類し、ツリーレイアウト用の `Offsets` クラス（`Manager`、`MapRoot`、ノードの NetID／object オフセット）を定義する。オブジェクトのブラックリスト（テストキューブ、リスポーンマーカー等）を保持する。 |
| [`Memory.cs`](Memory.cs) | `OpenProcess`／`ReadProcessMemory`／`WriteProcessMemory`／`VirtualQueryEx` をラップする低レベル Win32 ラッパー。Vanguard を考慮した `Initialize` はアクセス拒否の失敗を捕捉し API 専用モードへ降格する。[C0reExternal-Base-v2](https://github.com/C0reTheAlpaca/C0reExternal-Base-v2/blob/master/Memory.cs) から移植。 |
| [`Snapshot.cs`](Snapshot.cs) | 1 読み取りフレーム分のプレーンなデータコンテナ。チャンピオンリスト、dragon／baron／herald、turret セット、NetID→object マップ、index→NetID マップ、次のドラゴンタイプを保持する。 |
| `OpenForestUI.Farsight.csproj` | net6.0 クラスライブラリ。`OpenForestUI.Common`（ロギング、バイトバッファ拡張、`CDragonChampion`、hex-JSON コンバータ）を参照する。 |

## サブディレクトリ

| Directory | 目的 |
| --- | --- |
| [`Object/`](Object/README-ja.md) | `GameObject` ユニットモデルとそのユニットごとのメモリオフセット |

## オフセットのリバースエンジニアリング（パッチ後の更新）

League はデータを暗号化せずに保持しているため、構造をリバースして直接読み取ることができる。
以下の手順は簡略化されている。ある程度のプログラミング経験と適切なツールを前提とする。
更新したオフセットを作成し、それがまだリポジトリに無い場合は Pull Request を開いてほしい。
これはボランティア主導のオープンソースプロジェクトである。

### セットアップ

League のアンチチートはプログラム名とアイコンでツールを検出するため、League を開いた状態で
使用するツールはすべて自前でリビルドするか、[Resource Hacker](http://www.angusj.com/resourcehacker/) で
ウィンドウ名とアイコンを変更する必要がある。

**前提条件**
- （任意）[Resource Hacker](http://www.angusj.com/resourcehacker/)
- 改変した [Cheat Engine](https://www.cheatengine.org/) または [ReClass.NET](https://github.com/ReClassNET/ReClass.NET)
- [LeagueDumper](https://github.com/tarekwiz/LeagueDumper)
- [LoL Offset Dumper](https://www.unknowncheats.me/forum/league-of-legends/386218-lol-offset-dump.html) とパターンリスト（以下は 26.08.2021 時点のもの。時間とともに変わる）

### 探すもの

すべてのチャンピオンとユニットは **ObjectManager**（GameObject のツリー）に存在する。よって
(1) ObjectManager の場所と (2) 単一の GameObject の構造が必要となる。

### Step 1 — ObjectManager と Local Player

ベースオフセットは通常、各パッチ直後に UnknownCheats に投稿される。毎パッチ更新される中央の
Offsets/Patterns スレッドがあるので、それを使うこと（ただしヘルプ要求でスパムしないこと）。
自分で生成する必要がある場合は LeagueDumper と LoL Offset Dumper の手順に従う。現行のパターン:

```
ADDRESS, oLocalPlayer,        "A1 ? ? ? ? 85 C0 74 07 05 ? ? ? ? EB 02 33 C0 56", 1
ADDRESS, oObjManager,         "A1 ?? ?? ?? ?? C7 40 ?? ?? ?? ?? ?? C3", 1
ADDRESS, oObjManagerBackup,   "8B 0D ? ? ? ? E8 ? ? ? ? FF 77", 1
ADDRESS, oGameTime,           "F3 0F 11 05 ? ? ? ? 8B 49", 1
ADDRESS, oHudInstance,        "A1 ? ? ? ? F3 0F 10 44 24 08", 1
ADDRESS, oHudInstanceBackup,  "8B 0D ? ? ? ? 6A 00 8B 49 34 E8 ? ? ? ? B0", 1
ADDRESS, oUnderMouseObject,   "8B 0D ? ? ? ? 89 0D ? ? ? ? 3B 44 24 30", 2
```

### Step 2 — GameObject

1. カスタムまたはプラクティスツールのゲームを開く。
2. ReClass（または Cheat Engine — 少なくとも同等の機能）を開く。
3. `[<League of Legends.exe> + LocalPlayerOffset]` に新しいクラスを作成する。値が更新され
   始めるはずである。約 13000〜14000 バイトを追加する。リスト全体がゼロになったら割り当て範囲を
   超えている（アクセス違反）ので、削除して少し少なめに追加する。
4. ReClass では: 赤 = オフセット、緑 = 絶対アドレス、青 = ASCII、4 列の黒い hex 列 = 生の値、
   緑のスラッシュの後に小数、赤い矢印がポインタを示し、検出された文字列は青いテキストで描画される。
5. 古いオフセットを手がかりに探す場所を判断する。ライブゲーム中に値を変更して見つけることもできる。

**Tips**
- Health/MaxHealth と Mana/MaxMana は通常 `0x10` 離れている。ペアを見つけたら、アイテムを買うか
  ダメージを受けてどちらがどちらか判別する。
- チャンピオン名は ReClass リストの右端に文字列として描画されることが多い（しばしば誤っているが、
  ざっと確認するのに便利）。
- XP を上げられるようプラクティスツールのロビーを使う。レベルではなく **experience** の値を探す
  （例: レベル 2 = 280 XP、レベル 3 = 660）。表は
  [LoL wiki](https://leagueoflegends.fandom.com/wiki/Experience_(champion)) にある。
