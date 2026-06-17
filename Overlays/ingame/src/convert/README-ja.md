> 🌐 [English](README.md) ・ **日本語**

# ブラウザ変換 / クエリヘルパー

オーバーレイ向けに環境入力を適合させる小さなブラウザ側ユーティリティ。現状はシーン起動時に使う URL / クエリのパースのみ。

## 内容

- [`windowUtils.ts`](windowUtils.ts) — `WindowUtils.GetQueryVariable(name)` はページの `?query=` 文字列から値を読み取る。`IngameScene.preload` はこれを使って `backend` パラメータ（バックエンドホスト）を読み取り、デフォルトは `localhost`。
