> 🌐 [English](REPO-MAP.md) ・ **日本語**

# リポジトリマップ

OpenForestUI は、トーナメント放送向けの League of Legends 配信オーバーレイを集約した、MIT ライセンスの公開ハブである（メンテナー: Negi - BlueStand）。.NET 6 WPF デスクトップアプリ（`OpenForestUI.exe`）が管制センターとして機能し、ライブのゲームデータ（LCU API、port 2999 の観戦用 Live Client Data API、インプロセスの **Farsight** メモリリーダー、Replay API、Python OCR サイドカー）を取り込み、埋め込み HTTP + WebSocket サーバー経由で 2 つのブラウザソースオーバーレイ（**ingame** と **pickban**）を配信する。

本ファイルはリポジトリ内の**すべての**ディレクトリのマスターインデックスである。各エントリは、そのディレクトリ自身の `README.md` へリンクする（リンクは `docs/` からの相対）。以下のエリアに分かれている: WPF アプリ（`OpenForestUI/`）、共有ライブラリ（`OpenForestUI.Common/`、`OpenForestUI.Farsight/`）、vendored の LCU クライアント（`LCUSharp/`）、オーバーレイ（`Overlays/`）、本ドキュメントセット（`docs/`）、OCR サイドカー（`ocr-poc/`）。

---

## OpenForestUI/ — WPF 管制センターアプリ

.NET 6 WPF デスクトップアプリ（`OpenForestUI.exe`）。DI コンポジションルート、MVVM ダッシュボードシェル、データ取り込み + オーバーレイサーバーのホスト。

| ディレクトリ | 目的 |
|---|---|
| [`OpenForestUI/`](../OpenForestUI/README-ja.md) | WPF 管制センターアプリ: DI コンポジションルート、MVVM ダッシュボードシェル、データ取り込み + オーバーレイサーバーホスト |
| [`OpenForestUI/Assets/`](../OpenForestUI/Assets/README-ja.md) | XAML から参照され csproj にバンドルされるブランドアセット（アイコン、アプリ内画像、ブランドフォント） |
| [`OpenForestUI/Assets/Fonts/`](../OpenForestUI/Assets/Fonts/README-ja.md) | Venus Rising ブランド書体（.otf）。ワードマーク用に WPF Resource として埋め込み |
| [`OpenForestUI/Assets/Icons/`](../OpenForestUI/Assets/Icons/README-ja.md) | .ico のウィンドウ/実行ファイルアイコン（現行の OpenForestUI.ico、レガシーの BlueEssence.ico） |
| [`OpenForestUI/Assets/Images/`](../OpenForestUI/Assets/Images/README-ja.md) | ダッシュボード UI 用 PNG ビットマップ（ロゴ、expander グリフ、チームロゴのプレースホルダー、レガシー BE マーク） |
| [`OpenForestUI/Http/`](../OpenForestUI/Http/README-ja.md) | オーバーレイへ配信する EmbedIO HTTP + WebSocket サーバー（`ws://localhost:9001/api`、`/frontend`） |
| [`OpenForestUI/OperatingSystem/`](../OpenForestUI/OperatingSystem/README-ja.md) | Win32 入力合成、WMI プロセス監視、小さな enum/color/messagebox ヘルパー |
| [`OpenForestUI/MVVM/`](../OpenForestUI/MVVM/README-ja.md) | WPF プレゼンテーション層のルート。すべての MVVM サブディレクトリをインデックス |
| [`OpenForestUI/MVVM/Behaviors/`](../OpenForestUI/MVVM/Behaviors/README-ja.md) | code-behind を置き換える attached behavior（ボーダーレスタイトルバー用 WindowDragBehavior） |
| [`OpenForestUI/MVVM/Controls/`](../OpenForestUI/MVVM/Controls/README-ja.md) | カスタムテンプレートコントロール。WinUI 風 ToggleSwitch |
| [`OpenForestUI/MVVM/Converters/`](../OpenForestUI/MVVM/Converters/README-ja.md) | XAML バインディング用 IValueConverter（enum/bool、bool/visibility、bool/color、string/image、トグルオフセット） |
| [`OpenForestUI/MVVM/Core/`](../OpenForestUI/MVVM/Core/README-ja.md) | MVVM プリミティブ: ObservableObject シム、RelayCommand、dependency-object ヘルパー、プリセット Bindings |
| [`OpenForestUI/MVVM/Core/Services/`](../OpenForestUI/MVVM/Core/Services/README-ja.md) | view-model に注入される DI サービス: ナビゲーション、config、アプリ状態、ウィンドウ制御 |
| [`OpenForestUI/MVVM/DragDrop/`](../OpenForestUI/MVVM/DragDrop/README-ja.md) | vendored の Josh Smith / Dan Crevier ListView 並べ替えヘルパー（Ingame ロスター） |
| [`OpenForestUI/MVVM/Resources/`](../OpenForestUI/MVVM/Resources/README-ja.md) | Tokens.xaml デザイントークン（最初にマージ）と ColorStyles.xaml トグルパレット |
| [`OpenForestUI/MVVM/Theme/`](../OpenForestUI/MVVM/Theme/README-ja.md) | App.xaml でマージされるコントロール別 Style/ControlTemplate ディクショナリ |
| [`OpenForestUI/MVVM/View/`](../OpenForestUI/MVVM/View/README-ja.md) | ダッシュボードシェルおよび機能別 config 用の WPF XAML ページ/ウィンドウ |
| [`OpenForestUI/MVVM/ViewModel/`](../OpenForestUI/MVVM/ViewModel/README-ja.md) | ビューの背後にあるバインド可能な view-model。DI + INavigationService で結線 |
| [`OpenForestUI/Common/`](../OpenForestUI/Common/README-ja.md) | アプリオーケストレーションの中核: コントローラ、config/データ層、オーバーレイイベント基底型 |
| [`OpenForestUI/Common/Controllers/`](../OpenForestUI/Common/Controllers/README-ja.md) | 起動、クライアント/ゲーム監視、データ取り込み、オーバーレイ配信をシーケンスするシングルトン |
| [`OpenForestUI/Common/Data/`](../OpenForestUI/Common/Data/README-ja.md) | JSON 設定モデル/ローダーと Data Dragon アセットプロバイダー |
| [`OpenForestUI/Common/Data/Config/`](../OpenForestUI/Common/Data/Config/README-ja.md) | 強い型付けの Config/*.json モデルと JSONConfigProvider（読み込み/マイグレーション/書き込み） |
| [`OpenForestUI/Common/Data/Provider/`](../OpenForestUI/Common/Data/Provider/README-ja.md) | ゲームバージョンを解決しチャンピオン/アイテム/スペルアセットをキャッシュする DataDragon シングルトン |
| [`OpenForestUI/Common/Events/`](../OpenForestUI/Common/Events/README-ja.md) | eventType ディスクリミネーターを持つ抽象 LeagueEvent / OverlayConfig 基底型 |
| [`OpenForestUI/ChampSelect/`](../OpenForestUI/ChampSelect/README-ja.md) | チャンピオンセレクトパイプライン: LCU ドラフトを取り込み、正規化し、pickban オーバーレイへイベントを push |
| [`OpenForestUI/ChampSelect/Data/`](../OpenForestUI/ChampSelect/Data/README-ja.md) | データ層: 生の LCU 入力、正規化されたオーバーレイ DTO、永続化された pickban config |
| [`OpenForestUI/ChampSelect/Data/Config/`](../OpenForestUI/ChampSelect/Data/Config/README-ja.md) | 永続化されるオーバーレイ config: チーム名/スコア/カラー、トグル、放送用トップバーのメタデータ |
| [`OpenForestUI/ChampSelect/Data/DTO/`](../OpenForestUI/ChampSelect/Data/DTO/README-ja.md) | pickban オーバーレイが消費する整形済みの pick/ban/team/version モデル |
| [`OpenForestUI/ChampSelect/Data/LCU/`](../OpenForestUI/ChampSelect/Data/LCU/README-ja.md) | League Client チャンピオンセレクトセッション JSON に対応する生の C# モデル |
| [`OpenForestUI/ChampSelect/Events/`](../OpenForestUI/ChampSelect/Events/README-ja.md) | pickban オーバーレイへの LeagueEvent ペイロード（newState、newAction、start、end、heartbeat） |
| [`OpenForestUI/ChampSelect/StateInfo/`](../OpenForestUI/ChampSelect/StateInfo/README-ja.md) | シングルトンのドラフト状態ストア、LCU→オーバーレイコンバーター、アクティブスロットロジック |
| [`OpenForestUI/Ingame/`](../OpenForestUI/Ingame/README-ja.md) | Ingame オーバーレイバックエンド: 毎ティック、ローカルの Riot データを取り込み frontend ペイロードを push |
| [`OpenForestUI/Ingame/Data/`](../OpenForestUI/Ingame/Data/README-ja.md) | 全 ingame データ型のインデックス: プロバイダー、Riot/Replay DTO、ハブモデル、frontend ペイロード、config |
| [`OpenForestUI/Ingame/Data/Config/`](../OpenForestUI/Ingame/Data/Config/README-ja.md) | オーバーレイレイアウト用の永続化 config（IngameConfig）とメモリリーダーオフセット（FarsightConfig） |
| [`OpenForestUI/Ingame/Data/Frontend/`](../OpenForestUI/Ingame/Data/Frontend/README-ja.md) | WS 経由でシリアライズされる放送向け DTO。機能別 ShouldSerialize* チェックでゲート |
| [`OpenForestUI/Ingame/Data/Hub/`](../OpenForestUI/Ingame/Data/Hub/README-ja.md) | 生 DTO と frontend の間に立つ放送用集約モデル（チーム、プレイヤー、インヒビター、ゴールド） |
| [`OpenForestUI/Ingame/Data/Hub/Objectives/`](../OpenForestUI/Ingame/Data/Hub/Objectives/README-ja.md) | Baron/Dragon パワープレイおよびスポーンパネル用のバックエンドアキュムレーター + frontend ペイロード |
| [`OpenForestUI/Ingame/Data/Provider/`](../OpenForestUI/Ingame/Data/Provider/README-ja.md) | Live Client Data および Replay API（port 2999）用のローカル Riot HTTP クライアント + objective イベント引数 |
| [`OpenForestUI/Ingame/Data/RIOT/`](../OpenForestUI/Ingame/Data/RIOT/README-ja.md) | 観戦用 `/liveclientdata/*` JSON をミラーするプレーンな C# DTO |
| [`OpenForestUI/Ingame/Data/Replay/`](../OpenForestUI/Ingame/Data/Replay/README-ja.md) | クライアント内 Replay API 用 DTO: 再生クロック、カメラ/レンダー状態、HUD トグル、キーフレーム |
| [`OpenForestUI/Ingame/Events/`](../OpenForestUI/Ingame/Events/README-ja.md) | ingame オーバーレイへの LeagueEvent 派生 DTO（Heartbeat + ポップアップ）と RiotEvent モデル |
| [`OpenForestUI/Ingame/State/`](../OpenForestUI/Ingame/State/README-ja.md) | ゲーム別状態エンジン、トグルゲートされる JSON スナップショット（StateData）、ObjectiveSpawnClock |

## OpenForestUI.Common/ — 共有低レベルライブラリ

横断的なコード（ロギング、HTTP/REST、JSON コンバーター、ユーティリティ、データモデル）の net6.0 クラスライブラリで、ソリューションの他の部分から参照される。

| ディレクトリ | 目的 |
|---|---|
| [`OpenForestUI.Common/`](../OpenForestUI.Common/README-ja.md) | 共有低レベルライブラリ: ロギング、HTTP/REST、JSON コンバーター、ユーティリティ、データモデル |
| [`OpenForestUI.Common/Data/`](../OpenForestUI.Common/Data/README-ja.md) | 共有データクラスのインデックス（アプリ向け DTO + Riot/Community Dragon の静的型） |
| [`OpenForestUI.Common/Data/DTO/`](../OpenForestUI.Common/Data/DTO/README-ja.md) | CDragon ソース型とスリムなオーバーレイ形状を対にしたチャンピオン & サモナースペル DTO |
| [`OpenForestUI.Common/Data/RIOT/`](../OpenForestUI.Common/Data/RIOT/README-ja.md) | Riot の固定テーブル: XP→レベル曲線とアイテムデータ/ゴールドコスト |
| [`OpenForestUI.Common/Http/`](../OpenForestUI.Common/Http/README-ja.md) | REST クライアント、ファイルダウンローダー、テキスト取得ヘルパー（一部は GoldDiff より適応、MIT） |
| [`OpenForestUI.Common/Utils/`](../OpenForestUI.Common/Utils/README-ja.md) | 汎用ヘルパー: バイトバッファ拡張、JSON コンバーター、循環バッファ、StringVersion |

## OpenForestUI.Farsight/ — メモリリーダーライブラリ

観戦 API が省くゴールド/XP/座標を厳密に取得するため、League の ObjectManager を辿るインプロセスのメモリリーダー。Vanguard 互換のためオプトインかつデフォルト無効。

| ディレクトリ | 目的 |
|---|---|
| [`OpenForestUI.Farsight/`](../OpenForestUI.Farsight/README-ja.md) | 観戦 API が省くデータを ObjectManager を辿って取得するメモリリーダー（オプトイン） |
| [`OpenForestUI.Farsight/Object/`](../OpenForestUI.Farsight/Object/README-ja.md) | GameObject クラスと、Farsight.json 由来のパッチ別ユニット単位フィールドオフセットテーブル |

## LCUSharp/ — League Client（LCU）API クライアント（vendored）

ローカルの LeagueClientUx LCU API（HTTP + WebSocket）に接続する vendored の MIT .NET 6 ライブラリ。OpenForestUI のチャンピオンセレクト/ピックバンオーバーレイ向けデータソース。

| ディレクトリ | 目的 |
|---|---|
| [`LCUSharp/`](../LCUSharp/README-ja.md) | vendored の LCU API クライアント（HTTP + WebSocket）— チャンピオンセレクトのデータソース |
| [`LCUSharp/Http/`](../LCUSharp/Http/README-ja.md) | 認証付き HttpClient 配管 + `https://127.0.0.1:<port>/` 上の JSON リクエスト/レスポンスハンドラー |
| [`LCUSharp/Http/Endpoints/`](../LCUSharp/Http/Endpoints/README-ja.md) | EndpointBase と、特定の League Client REST ルートに対する強い型付けのラッパー |
| [`LCUSharp/Http/Endpoints/ProcessControl/`](../LCUSharp/Http/Endpoints/ProcessControl/README-ja.md) | `process-control/v1/process/*` をラップ: quit、restart、restart-to-repair/-update |
| [`LCUSharp/Http/Endpoints/RiotClient/`](../LCUSharp/Http/Endpoints/RiotClient/README-ja.md) | `riotclient/*` をラップ: UX の show/minimize/flash/kill/launch、ズームスケールの get/set |
| [`LCUSharp/Utility/`](../LCUSharp/Utility/README-ja.md) | LeagueClientUx プロセスを特定し、lockfile を解析して（port, token）認証情報を取得 |
| [`LCUSharp/Websocket/`](../LCUSharp/Websocket/README-ja.md) | ライブクライアントイベントへの WAMP 風 WebSocket サブスクリプション。URI 別サブスクライバーへディスパッチ |

## Overlays/ — ブラウザソース配信オーバーレイ（フロントエンド）

デスクトップアプリが HTTP `/frontend` + WebSocket `/api` 経由で放送クライアントへ配信する 2 つの web オーバーレイ。

| ディレクトリ | 目的 |
|---|---|
| [`Overlays/`](../Overlays/README-ja.md) | 2 つの web オーバーレイ（ingame、pickban）のインデックス |
| [`Overlays/ingame/`](../Overlays/ingame/README-ja.md) | Parcel バンドルの Phaser WebGL ingame オーバーレイ: スコアボード、タイマー、グラフ、ポップアップ |
| [`Overlays/ingame/src/`](../Overlays/ingame/src/README-ja.md) | オーバーレイ TypeScript ルート: main.ts ブート、変数/config、scenes/visual/data/util/convert サブツリー |
| [`Overlays/ingame/src/convert/`](../Overlays/ingame/src/convert/README-ja.md) | ブート時に `?backend=` ホストパラメータを読む WindowUtils.GetQueryVariable |
| [`Overlays/ingame/src/data/`](../Overlays/ingame/src/data/README-ja.md) | visual が消費するハートビート別 JSON の TypeScript ミラー |
| [`Overlays/ingame/src/data/config/`](../Overlays/ingame/src/data/config/README-ja.md) | OverlayConfig インターフェイス群: 要素別レイアウト/フォント/トグル（PrmScore/GoldGraph フラグ含む） |
| [`Overlays/ingame/src/scenes/`](../Overlays/ingame/src/scenes/README-ja.md) | IngameScene: WS 接続を所有し状態を visual へ展開する単一 Phaser シーン |
| [`Overlays/ingame/src/util/`](../Overlays/ingame/src/util/README-ja.md) | 依存なしヘルパー: gold/テキスト整形、フォント読み込み、RGBA、Vector2、Queue、Dictionary |
| [`Overlays/ingame/src/visual/`](../Overlays/ingame/src/visual/README-ja.md) | 共有 VisualElement 基底の上に、画面要素ごとに 1 クラス |
| [`Overlays/ingame/public/`](../Overlays/ingame/public/README-ja.md) | Phaser オーバーレイが読み込む `/frontend` 配信静的アセットのルート |
| [`Overlays/ingame/public/backgrounds/`](../Overlays/ingame/public/backgrounds/README-ja.md) | スコアボード、グラフ、インヒビター、info ページ、objective バー用 PNG/MP4 パネル背景 |
| [`Overlays/ingame/public/images/`](../Overlays/ingame/public/images/README-ja.md) | 汎用 objective/tower アイコンと dragons、lanes、放送バー、popup サブツリー |
| [`Overlays/ingame/public/images/dragons/`](../Overlays/ingame/public/images/dragons/README-ja.md) | 属性ドラゴン別アイコンを scoreboard と timer のバリアントに分割 |
| [`Overlays/ingame/public/images/dragons/scoreboard/`](../Overlays/ingame/public/images/dragons/scoreboard/README-ja.md) | スコアボードに表示するドラゴン別 `*Large.png` アイコン |
| [`Overlays/ingame/public/images/dragons/timers/`](../Overlays/ingame/public/images/dragons/timers/README-ja.md) | リスポーンタイマーに表示するドラゴン別 `*Timer.png` アイコン |
| [`Overlays/ingame/public/images/lanes/`](../Overlays/ingame/public/images/lanes/README-ja.md) | top/mid/bot レーンアイコンの SVG |
| [`Overlays/ingame/public/images/prm/`](../Overlays/ingame/public/images/prm/README-ja.md) | 放送用トップバー/スコアボード用 `prm_*` objective アイコン |
| [`Overlays/ingame/public/images/scoreboardPopUps/`](../Overlays/ingame/public/images/scoreboardPopUps/README-ja.md) | 主要 objective 用のスポーン/キル/ソウルバナーアート。ObjectivePopUpVisual が使用 |
| [`Overlays/ingame/public/images/scoreboardPopUps/Baron/`](../Overlays/ingame/public/images/scoreboardPopUps/Baron/README-ja.md) | baronSpawn/baronKill ポップアップバナーの静止画とループ |
| [`Overlays/ingame/public/images/scoreboardPopUps/Dragon/`](../Overlays/ingame/public/images/scoreboardPopUps/Dragon/README-ja.md) | 属性ドラゴン別スポーン/キル/ソウルポップアップバナー。型ごとに 1 フォルダ |
| [`Overlays/ingame/public/images/scoreboardPopUps/Dragon/Chemtech/`](../Overlays/ingame/public/images/scoreboardPopUps/Dragon/Chemtech/README-ja.md) | chemtech スポーン/キル/ソウルポップアップバナー（PNG+MP4） |
| [`Overlays/ingame/public/images/scoreboardPopUps/Dragon/Cloud/`](../Overlays/ingame/public/images/scoreboardPopUps/Dragon/Cloud/README-ja.md) | cloud スポーン/キル/ソウルポップアップバナー（PNG+MP4） |
| [`Overlays/ingame/public/images/scoreboardPopUps/Dragon/Elder/`](../Overlays/ingame/public/images/scoreboardPopUps/Dragon/Elder/README-ja.md) | elder スポーン/キルポップアップバナー（ソウルバリアントなし） |
| [`Overlays/ingame/public/images/scoreboardPopUps/Dragon/Fire/`](../Overlays/ingame/public/images/scoreboardPopUps/Dragon/Fire/README-ja.md) | fire（インファーナル）スポーン/キル/ソウルポップアップバナー（PNG+MP4） |
| [`Overlays/ingame/public/images/scoreboardPopUps/Dragon/Hextech/`](../Overlays/ingame/public/images/scoreboardPopUps/Dragon/Hextech/README-ja.md) | hextech スポーン/キル/ソウルポップアップバナー（PNG+MP4） |
| [`Overlays/ingame/public/images/scoreboardPopUps/Dragon/Mountain/`](../Overlays/ingame/public/images/scoreboardPopUps/Dragon/Mountain/README-ja.md) | mountain スポーン/キル/ソウルポップアップバナー（PNG+MP4） |
| [`Overlays/ingame/public/images/scoreboardPopUps/Dragon/Ocean/`](../Overlays/ingame/public/images/scoreboardPopUps/Dragon/Ocean/README-ja.md) | ocean スポーン/キル/ソウルポップアップバナー（PNG+MP4） |
| [`Overlays/ingame/public/images/scoreboardPopUps/Herald/`](../Overlays/ingame/public/images/scoreboardPopUps/Herald/README-ja.md) | heraldSpawn/heraldKill ポップアップバナーの静止画とループ |
| [`Overlays/ingame/public/masks/`](../Overlays/ingame/public/masks/README-ja.md) | チャンプカバー、アイテムテキスト、スコアボード、グラフ、info ページ、ポップアップをクリップするマスクテクスチャ |
| [`Overlays/pickban/`](../Overlays/pickban/README-ja.md) | React/CRA ピック & バンオーバーレイ（RCVolus lol-pick-ban-ui からフォーク） |
| [`Overlays/pickban/config/`](../Overlays/pickban/config/README-ja.md) | eject 済み CRA の webpack/dev-server/babel/env/path 設定 |
| [`Overlays/pickban/config/jest/`](../Overlays/pickban/config/jest/README-ja.md) | テストで CSS とバイナリアセットの import を無害化するカスタム Jest トランスフォーマー |
| [`Overlays/pickban/public/`](../Overlays/pickban/public/README-ja.md) | HTML シェル、robots.txt、frontend-lib.js（Window.PB WebSocket クライアントブリッジ） |
| [`Overlays/pickban/scripts/`](../Overlays/pickban/scripts/README-ja.md) | webpack と jest を呼び出す eject 済み start/build/test スクリプト |
| [`Overlays/pickban/src/`](../Overlays/pickban/src/README-ja.md) | React アプリ: React をブートし、バックエンド WS 状態を購読、europe ドラフトレイアウトをレンダー |
| [`Overlays/pickban/src/assets/`](../Overlays/pickban/src/assets/README-ja.md) | センターロゴと、ライブのチャンピオンアートが届く前に使うプレースホルダー splash/ban SVG |
| [`Overlays/pickban/src/assets/fonts/`](../Overlays/pickban/src/assets/fonts/README-ja.md) | @font-face ファミリーとして宣言された TrueType フォント（Rawline、Raleway、Amarello） |
| [`Overlays/pickban/src/europe/`](../Overlays/pickban/src/europe/README-ja.md) | 欧州（EU）スタイルのドラフトレイアウトをレンダーする React コンポーネント（Overlay/Pick/Ban） |
| [`Overlays/pickban/src/europe/style/`](../Overlays/pickban/src/europe/style/README-ja.md) | LESS CSS モジュールスタイルシートとドラフトリビールアニメーション |

## docs/ — プロジェクトドキュメント & 設計仕様

OpenForestUI のリファレンスドキュメント、ビルド仕様、設計ノート（API 可用性、オフセット、オーバーレイ仕様）のインデックス。

| ディレクトリ | 目的 |
|---|---|
| [`docs/`](README-ja.md) | リファレンスドキュメント、ビルド仕様、設計ノートのインデックス |
| [`docs/api/`](api/README-ja.md) | LoL ローカル API（port 2999）のエンドポイント別決定版リファレンス |
| [`docs/data/`](data/README-ja.md) | アーカイブされたゲームデータテーブル（現在はパッチ別 Farsight メモリオフセットのアーカイブ） |
| [`docs/data/offsets/`](data/offsets/README-ja.md) | オプションのメモリリーダーが使う `Offsets-<patch>.json`（11.9→14.6）のカタログ |
| [`docs/feature-completion/`](feature-completion/README-ja.md) | メモリリーディングなしで全 Ingame タイルを機能させるための設計 |
| [`docs/lck-scoreboard/`](lck-scoreboard/README-ja.md) | 下部の比較スコアボードビジュアルの仕様 + 参照画像 |
| [`docs/prm-overlay/`](prm-overlay/README-ja.md) | 放送用トップ/ボトムバーの仕様、参照フレーム、抽出したカウンターアイコン |

## ocr-poc/ — HUD OCR サイドカー

観戦 API が丸めたり省いたりする値を読み取る Python OCR サイドカー + リファレンスパイプライン。`goldcap.py` がライブサイドカー。

| ディレクトリ | 目的 |
|---|---|
| [`ocr-poc/`](../ocr-poc/README-ja.md) | HUD から正確な CS/gold/objective カウントを読む Python OCR サイドカー |
| [`ocr-poc/overlay-harness/`](../ocr-poc/overlay-harness/README-ja.md) | モック WS 状態を ingame オーバーレイへ流し、ヘッドレスでスクリーンショットを撮る Node ハーネス |
