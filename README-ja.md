> 🌐 [English](README.md) ・ **日本語**

# OpenForestUI

OpenForestUIはMITライセンスの完全オープンソースな大会用UIオーバーレイツールです。自由にフォーク・カスタマイズして構いません。

このリポジトリは、メンテナであるNegiが気が向いたときにUPDATEされます。現時点ではチャンピオンセレクトとインゲームオーバーレイが入っていて、ポストゲーム用は将来気が向いたら追加するかも。しらんけど。
> ## 免責事項・依頼
> ⚠️ **自己責任でご利用ください。**
> OpenForestUI および BlueStandは一切の責任を負いません。このソフトおよびバイナリは現状のまま（as is）提供され、いかなる保証もありません（[MIT License](LICENSE) を参照）。本ソフトを使ったことで生じた損害・不利益・アカウントBAN・配信トラブルなどについて、メンテナや貢献者は **一切責任を負いません**。[Riot Games の利用規約](https://www.riotgames.com/ja-jp/terms-of-service) や、参加する大会・配信のルールに反していないかは、**利用者ご自身で確認してください**。OCR 機能は**自分の画面のピクセルを読むだけ**（ゲームメモリは読まない）ですが、どう使うかの責任は利用者にあります。良識の範囲内で利活用してください。
> **リポジトリのメンテナ:** Negi - BlueStand

## Features

現状の実装済み機能:
1. レベルアップポップアップ
2. アイテム購入ポップアップ
3. ~~オブジェクトタイマー & パワープレイのポップアップ~~(Bugが存在したため停止中。)
4. 動的なゴールドグラフ
5. CS / Gold のプレイヤータブ（Gold は画面 OCR 経由。下の [画面 OCR について](#on-screen-ocr-exact-cs--gold) を参照）
6. カスタムスコアボード（トップバー ＋ スコアボード）
7. ゲーム開始時の UI 自動初期化

ほぼ正確な CS/Gold は、ゲーム中の HUD から小さな Python の OCR サイドカー（`ocr-poc/goldcap.py`）で読み取っています。観戦用の Live Client Data API が正確な値を出してくれないためです。
ただし正確である保証はしません。APIじゃないもの。

## Tech stack

- **Control** — C# / .NET 6、WPF（CommunityToolkit.Mvvm + Microsoft.Extensions.DependencyInjection + WPF-UI Fluent）。オーバーレイを配信する組み込みの HTTP/WebSocket サーバー（EmbedIO）同梱
- **ingame オーバーレイ** — TypeScript + Phaser 3（phaser3-rex-plugins）+ Parcel
- **champion-select オーバーレイ** — TypeScript + React、Create React App（webpack/Babel）系、Less
- **OCR サイドカー** — Python 3：EasyOCR（PyTorch）、OpenCV、dxcam、NumPy、Pillow
- **ゲームデータ** — League Client (LCU) API、Live Client Data API、Replay API

## Quick start

```bash
# 1. コードを取得
git clone <your-fork-url> OpenForestUI && cd OpenForestUI

# 2. C# アプリをビルド
dotnet build OpenForestUI.sln -c Debug

# 3. オーバーレイをビルド
cd Overlays/ingame  && npm install && npm run build && cd ../..
cd Overlays/pickban && npm install && npm run build && cd ../..

# 4. OCR機能の依存をインストール
py -m pip install -r requirements.txt

# 5. 起動
#    OpenForestUI.exe を起動して、下の OBS ブラウザソースを追加してください。
```

- **OBS — ingame ソース:** `http://localhost:9001/frontend`
- **OBS — champion-select ソース:** `http://localhost:9001/?backend=ws://localhost:9001/api`

初回起動時に最新の DataDragon キャッシュを自動ダウンロードします（要ネット接続）。

## Requirements

* Windows 10 20H1（May 2020 Update, Build 19041）以降
* [.NET 6 SDK](https://dotnet.microsoft.com/download/dotnet/6.0)
* [Node.js](https://nodejs.org/) + npm
* Python 3.9〜3.12 と [`requirements.txt`](requirements.txt) の依存
* 初回起動時はネット接続

## On-screen OCR (exact CS / Gold)

観戦用の Live Client Data API は CS を 10 単位に丸められたため、一般ピーポーはチームゴールドやオブジェクト討伐数にアクセスできなくなりました。そこで OpenForestUI は、Python サイドカーを伴走させることで HUD から直接これらの数字を読み取ります。

**自分の画面そのもの**をキャプチャするDXGI Desktop Duplicationを採用しているので、OCR が効くのは HUD が見えていて、かつ較正したときと同じレイアウトになっているときだけです:

- **解像度は 1920×1080に設定を** キャプチャ領域は 1080p（ボーダーレスフルスクリーン）で補正しています。他の解像度では自動でスケールしますが、LoL の HUD はきれいに比例スケールしないのでズレることがあります。その場合は再補正してください。
- **観戦の UI／HUD スケールは較正時の設定に固定すべし** 基準のレイアウトは UI スケール最大（100）で撮っています。スケールが違うとパネルが ROI からズレます。
- **オブザーバーのトップバーと詳細スコアボードは常に見える状態に** Gold・タワー・オブジェクト数はトップバーから、各プレイヤーの CS は下部中央の詳細スコアボードから読みます。ゲーム中はどちらも出したままにしてください。また、**HUDスケールは100にしてください**。
- **数字を何かで隠さないこと** 他のウィンドウ・ポップアップ・マウスカーソル・配信オーバーレイなどがその領域に重なると読み取りができず、１０に丸め込まれた数字にフォールバックします。僕にAPIをください。
- **プライマリモニタでの実行をおすすめします** デスクトップ複製キャプチャは既定でプライマリディスプレイを取得します。観戦クライアントはそちらで動かしてください。

環境が違う場合は `goldcap.py --grab` / `--roi` で ROI を再補正できます。詳細な手順・データパイプライン・トラブルシューディングは [`ocr-poc/README.md`](ocr-poc/README.md) にまとめてあります。(PoCって書いてあるのは許してほしい(´・ω・｀))

## Usage

- 各コンポーネントは個別にオン／オフできます。必要に応じてチャンピオンセレクトや ingame を有効にしてください。
- 初回起動時に最新の DataDragon キャッシュを自動ダウンロードします。
- League が既定の場所に入っていない場合は、「Riot Games」／「League of Legends」のインストール先を含むフォルダを `Config/Component.json -> LeagueInstall` に追加してください。Replay API に必要です。

### Overlay configuration

- `Config/Ingame.json` で ingame オーバーレイを設定できます。好きに編集してください。
- 使いたいフォントは、カンマ区切りの `"GoogleFonts"` リストに追加します。
- `Frontend/ingame` の画像／動画を差し替えると素材を変えられます。

### Development / verification harness

`ocr-poc/overlay-harness/` は、決め打ちのモック状態に対して ingame オーバーレイを描画し、ヘッドレスでスクショを撮ります。オーバーレイを単体で調整・検証できます。詳しくは [`docs/prm-overlay/SPEC.md`](docs/prm-overlay/SPEC.md) を参照してください。

## Repository layout

リポジトリは、いくつかのトップレベル領域に分かれています:

- **`OpenForestUI/`** — .NET 6 WPF のコントロールセンターアプリ（`OpenForestUI.exe`）。MVVM のダッシュボード、データ取り込み、組み込みのオーバーレイサーバー
- **`OpenForestUI.Common/`** & **`OpenForestUI.Farsight/`** — 共有の低レベルライブラリ（HTTP/REST・DTO・ユーティリティ・ロギング）と、オプトインのメモリリーダー（使用不可、バリデーションのために残置）
- **`LCUSharp/`** — チャンピオンセレクトのデータ源に使う、ベンダー同梱の League Client（LCU）API クライアント
- **`Overlays/`** — 配信クライアントに渡す 2 つのブラウザソースオーバーレイ（`ingame` は Phaser、`pickban` は React）
- **`docs/`** — リファレンスと設計仕様（ローカル API の可用性・メモリオフセット・オーバーレイのレイアウト仕様）
- **`ocr-poc/`** — Python の OCR サイドカーと、ヘッドレスのオーバーレイ描画ハーネス

**全ディレクトリ**とその README をクリックでたどれる完全な索引は [docs/REPO-MAP-ja.md](docs/REPO-MAP-ja.md) を参照してください。

## License

**MIT License** で配布しています。詳細は [`LICENSE`](LICENSE) を参照してください。

OpenForestUI は [LeagueBroadcast](https://github.com/floh22/LeagueBroadcast)（Lars Eble, MIT）から派生したフォークです。チャンピオンセレクトオーバーレイは [lol-pick-ban-ui](https://github.com/RCVolus/lol-pick-ban-ui) の移植で、ingame オーバーレイの土台は ourcade の [phaser3-typescript-parcel-template](https://github.com/ourcade/phaser3-typescript-parcel-template)（MIT）に基づいています。上流の著作権表記はすべて保持しています。

OpenForestUI は独立したプロジェクトで、Riot Games の公認や提携を受けたものではありません。lol-pick-ban-ui の移植およびその作者は、Riot Games とは一切提携・関係していません。
