> 🌐 [English](readme.md) ・ **日本語**

# インゲーム放送オーバーレイ（Phaser 3 + rexUI）

> OpenForestUI の観戦 / リプレイゲーム向けフロントエンド配信オーバーレイ

![License](https://img.shields.io/badge/license-MIT-green)

これはインゲーム用のブラウザソースである。Phaser 3（WebGL、透過キャンバス）アプリで、parcel でバンドルされ、OpenForestUI デスクトップアプリが `/frontend` で配信する。起動時に `ws://localhost:9001/api` への WebSocket を開き、`Ingame` の `OverlayConfig` を要求し、バックエンドがプッシュする各 `GameHeartbeat` 状態とイベント（レベルアップ、アイテム完成、オブジェクトのキル / スポーン）を描画する。土台は ourcade の `phaser3-typescript-parcel-template` に由来する。

## 機能

このオーバーレイが描画できるもの:

- スコアボード / トップバー: レガシーの `ScoreboardVisual`、またはオプトインの放送用トップバー、加えてオプションの放送用または比較用の下部バー
- オブジェクトのスポーンタイマー（dragon / baron）と Baron / Elder Power Play（レガシーモード）
- インヒビターのリスポーンタイマー、情報サイドページ（プレイヤーごとのスタットタブ）、中央のゴールド差グラフ
- ポップアップ: プレイヤーのレベルアップ、アイテム完成、オブジェクトのキル / スポーン / ソウルポイント

## レイアウト

| パス | 役割 |
| --- | --- |
| [`src/`](src/README-ja.md) | TypeScript ソース: scene、visual、データモデル、util。完全な内訳は README を参照。 |
| `public/` | 静的アセット（画像、マスク、背景）。ビルドへそのままコピーされ `/frontend` 配下で配信。 |
| `dist/` | Parcel ビルド出力（デスクトップアプリが実際に配信するもの）。 |
| `package.json` | スクリプト（parcel 経由の `start` / `build`、eslint 経由の `lint`）と依存（`phaser`、`phaser3-rex-plugins`、`strongly-typed-events`）。 |

`npm run start` は `--public-url /frontend` でポート 10001 で配信する。`npm run build` は `dist/` へ出力する。デスクトップアプリはビルドされた `dist/` を自身で配信する。

## 前提条件

[Node.js](https://nodejs.org/en/)、[npm](https://www.npmjs.com/)、[Parcel](https://parceljs.org/) のインストールが必要。

Node.js と npm のインストールには [Node Version Manager](https://github.com/nvm-sh/nvm)（nvm）の使用を強く推奨する。

Windows ユーザーには [Node Version Manager for Windows](https://github.com/coreybutler/nvm-windows) がある。

これらの手順を全て代行する installNode.bat が同梱されている。
すでに node をインストール済み、または自分でインストールしたい場合の必要手順は以下。

`nvm` で Node.js と `npm` をインストール:

```bash
nvm install node

nvm use node
```

`nvm-windows` では 'node' を 'latest' に置き換える。

続いて Parcel をインストール:

```bash
npm install -g parcel-bundler
```

## はじめに

このリポジトリと OpenForestUI の両方がローカルマシンにインストールされていることを確認する。本プロジェクトは OpenForestUI のインストールに含まれているはずである。



installNode.bat を使わなかった場合、新しいプロジェクトフォルダへ移動して依存をインストール:

```bash
cd frontend/ingame/ # または 'my-folder-name'
npm install
```

OpenForestUI とは別にこれを単独で使う場合は開発サーバーを起動:

```
npm run start
```

本プロジェクトを別の場所でホストしたい場合は dist を Web サーバーへコピーする。OpenForestUI はデフォルトで parcel を使ってファイルを配信する。

## 開発サーバーのポート

`package.json` の `start` スクリプトを変更することで開発サーバーのポート番号を変更できる。ポート番号の指定には Parcel の `-p` オプションを使う。

スクリプトはこのようになっている:

```
parcel src/index.html -p 10001
```

10001 を任意の値に変更する。

## ライセンス

[MIT License](https://github.com/ourcade/phaser3-typescript-parcel-template/blob/master/LICENSE)
