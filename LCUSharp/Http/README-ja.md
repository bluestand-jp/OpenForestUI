> 🌐 [English](README.md) ・ **日本語**

# LCU HTTP リクエスト層

`https://127.0.0.1:<port>/` を通じて League Client と通信するための REST プラミング。認証済みの `HttpClient`（Basic `riot:<token>`、自己署名証明書を受け入れ）を構築し、Newtonsoft.Json 経由で JSON をシリアライズ／デシリアライズし、クエリ文字列付きの相対 URL を組み立てる。特定のクライアントルートに対する型付きラッパーは [`Endpoints/`](Endpoints/README-ja.md) 配下にある。

## 内容

| ファイル | 役割 |
| --- | --- |
| [`RequestHandler.cs`](RequestHandler.cs) | `internal abstract` 基底クラス。手動の証明書処理と寛容な証明書検証コールバックを備えた `HttpClient` を生成し、クエリパラメータ文字列を構築し、`HttpRequestMessage` を準備し、レスポンスボディを読み取る。 |
| [`LeagueRequestHandler.cs`](LeagueRequestHandler.cs) | 具象ハンドラ。`Basic riot:<token>` ヘッダと `BaseAddress` を設定し、`ChangeSettings(port, token)` が再接続時にクライアントを再構築する。リクエストを送信し、（任意で）JSON レスポンスを型付きオブジェクトにデシリアライズする。 |
| [`ILeagueRequestHandler.cs`](ILeagueRequestHandler.cs) | `Port`、`Token`、`ChangeSettings`、および `GetJsonResponseAsync` / `GetResponseAsync<T>` リクエストメソッドを公開する公開インターフェース。 |

## サブディレクトリ

| ディレクトリ | 目的 |
| --- | --- |
| [`Endpoints/`](Endpoints/README-ja.md) | 特定の LCU ルート（riotclient UX 制御、プロセス制御）に対する型付きラッパー。 |
