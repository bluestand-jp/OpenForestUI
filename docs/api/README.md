> 🌐 **English** ・ [日本語](README-ja.md)

# LoL ローカル API（`https://127.0.0.1:2999`）完全リファレンス

> **目的:** League of Legends クライアントがローカルで公開する API を全エンドポイント網羅し、
> 本フォーク（Vanguard 互換・メモリ読み無し・観戦/放送用途）で **何が取れて何が取れないか** を確定させる。
>
> **採取条件:** 2026-06-13、稼働中の **観戦/リプレイ** セッション（`gameMode=CLASSIC`, `mapNumber=11`=Summoner's Rift, `gameTime≈26:31`）に対し全エンドポイントを実スキャン（`ocr-poc/overlay-harness/scan_api.py`）。値はその瞬間の実レスポンス。
>
> **重要な前提:** ここは **観戦（spectator/replay）** での結果。`activeplayer*` 系はライブ（自分がプレイ中）でのみ返る。放送＝観戦なので「観戦で取れるか」が我々にとっての真の可否。
>
> **★検証済み (2026-06-13, 実ライブ観戦):** ライブ観戦は内部的に **live feed の chunk を遅延再生するリプレイ再生器** で、API挙動は .rofl リプレイと **完全同一**。実測＝`/replay/playback` の length≈time（ライブの先端を再生）・`/replay/game`=200・`activeplayer`=400・**ドラゴンが取られた後も `/eventdata` に `DragonKill`=0**・CS=10刻み丸め・`/replay/render`=200。→ **本ドキュメントの制約はライブ放送にもそのまま適用**。objective数はライブでもAPI不可＝**OCR必須で確定**。一方 `/replay/render` のHUD制御はライブ観戦でも使える。

---

## 目次

1. [概要・接続](#1-概要接続)
2. [Live Client Data API](#2-live-client-data-api)
   - 2.1 [`/allgamedata`](#21-liveclientdataallgamedata)
   - 2.2 [`/playerlist` と per-player 群](#22-liveclientdataplayerlist--per-player-群)
   - 2.3 [`/eventdata`（★objective欠落）](#23-liveclientdataeventdata)
   - 2.4 [`/gamestats`](#24-liveclientdatagamestats)
   - 2.5 [`/activeplayer*`（観戦で401相当・HP/MPの所在）](#25-liveclientdataactiveplayer-群観戦では400)
3. [Replay API](#3-replay-api観戦リプレイで利用可)
   - 3.1 [`/replay/render`（★カメラ＋HUD制御66項目）](#31-replayrender--カメラ--hud-制御66フィールド)
   - 3.2 [`/replay/playback`](#32-replayplayback)
   - 3.3 [`/replay/recording`](#33-replayrecording)
   - 3.4 [`/replay/sequence`](#34-replaysequence)
   - 3.5 [`/replay/game` `/replay/banners` `/replay/particles`](#35-replaygame--replaybanners--replayparticles)
4. [RPC / 内省（`/Help` `/Subscribe` async）](#4-rpc--内省helpsubscribe-async)
5. [データ可否マトリクス（本プロジェクト向け結論）](#5-データ可否マトリクス本プロジェクト向け結論)
6. [実装上の注意](#6-実装上の注意)

---

## 1. 概要・接続

- ベース URL: **`https://127.0.0.1:2999`**（HTTPS・**自己署名証明書** → `curl -k` / `verify=False` 必須。PowerShell 5.1 の `Invoke-RestMethod -SkipCertificateCheck` は不可）。
- ゲーム/観戦/リプレイが起動中のみ応答。終了で全滅。
- ポート 2999 には **3系統** が同居する:
  | 系統 | パス接頭辞 | 用途 |
  |---|---|---|
  | **Live Client Data API** | `/liveclientdata/*` | ゲーム内データ（選手・イベント・ゲーム時間） |
  | **Replay API** | `/replay/*` | 観戦/リプレイのカメラ・HUD・再生・録画制御 |
  | **RPC / Swagger** | `/Subscribe` `/Help` `/swagger/*` `/async/*` | 低レベルRPCと自己記述 |
- **Swagger**: `GET /swagger/v3/openapi.json`（OpenAPI3, 57KB）/ `GET /swagger/v2/swagger.json`。
  ⚠️ **Swagger に定義されているのは Replay API のスキーマだけ**（`Render`/`Recording`/`Playback`/`Sequence`/`Game`/`Banners`/`Vector*`/`Color*`/`KeyFrame*` ほか39個）。**Live Client Data の応答モデルは Swagger に無い** → Live系のフィールドは本ドキュメントの実キャプチャが正典。
- 全 paths（swagger より）:
  ```
  GET  /liveclientdata/{allgamedata, activeplayer, activeplayerabilities,
       activeplayername, activeplayerrunes, eventdata, gamestats,
       playeritems, playerlist, playermainrunes, playerscores, playersummonerspells}
  GET/POST /replay/{playback, recording, render, sequence, particles}
  GET  /replay/game     GET/PUT /replay/banners
  POST /{Subscribe, Unsubscribe, Help, Cancel, Exit, AsyncDelete, AsyncResult, AsyncStatus}
  GET  /async/v1/result/{token}    GET/DELETE /async/v1/status/{token}
  GET  /swagger/{v1/api-docs, v2/swagger.json, v3/openapi.json}
  ```

---

## 2. Live Client Data API

### 2.1 `/liveclientdata/allgamedata`

全部入り（他エンドポイントの和集合）。1回で取得したいときの定番。≈47KB。

```
activePlayer : object   // 観戦では {error:"Spectator mode doesn't currently support this feature"}
allPlayers   : array    // ← §2.2 の per-player を10人分
events       : { Events: array }   // ← §2.3
gameData     : object   // ← §2.4 gamestats と同じ
```

### 2.2 `/liveclientdata/playerlist` と per-player 群

`GET /liveclientdata/playerlist` → **10人の配列**。各要素のスキーマ（実測・観戦で全取得可）:

| フィールド | 型 | 例 / 備考 |
|---|---|---|
| `championName` | string | `"エイトロックス"`（**ゲームロケール依存=日本語**） |
| `rawChampionName` | string | `"Character_Aatrox_Name"` → DataDragon champ key 導出に使う正規名 |
| `skinID` / `skinName` / `rawSkinName` | int / string / string | `31` / `"プレステージ DRX エイトロックス"` |
| `summonerName` | string | `"釈迦釈迦ぽてと#1173"`（= riotId と同値） |
| `riotId` / `riotIdGameName` / `riotIdTagLine` | string | `"…#1173"` / `"…"` / `"1173"`（per-player 照会キー） |
| `team` | string | `"ORDER"`（青） / `"CHAOS"`（赤） |
| `position` | string | `"TOP" / "JUNGLE" / "MIDDLE" / "BOTTOM" / "UTILITY"`（空のこともある） |
| `level` | int | `17` ✅ |
| `isDead` | bool | ✅ |
| `respawnTimer` | float | 秒。生存時 `0.0` ✅ |
| `isBot` | bool | |
| `items` | array | **6アイテム＋トリンケット=最大7**。各要素↓ ✅ |
| `runes` | object | `keystone` ＋ `primaryRuneTree` ＋ `secondaryRuneTree`（**メインのみ**。全ルーンは観戦不可の `activeplayerrunes`）↓ |
| `scores` | object | KDA/CS/ward ↓ ✅ |
| `summonerSpells` | object | `summonerSpellOne/Two` ↓（**種類のみ・CD無し**）✅ |
| `screenPositionCenter` | string `"x,y"` | チャンプの**画面ピクセル座標**。⚠️ 観戦カメラ依存。今回は `3.4e38`(=FLT_MAX)=**画面外/未計算**。カメラに映っている時のみ有効 |
| `screenPositionBottom` | string `"x,y"` | 同上（足元） |

**`items[]` 要素:** `{ itemID:int, displayName:str, rawDisplayName:str, rawDescription:str, count:int, price:int, slot:int(0-6), consumable:bool, canUse:bool }`

**`scores`:** `{ kills:int, deaths:int, assists:int, creepScore:int, wardScore:float }`
⚠️ **`creepScore` は観戦で10刻みに切り捨て**（既知）。正確CSは詳細スコアボードの **OCR** で補完済み。**gold / XP / damage は存在しない。**

**`runes`（メイン）:** `keystone{ id:int, displayName, rawDisplayName, rawDescription }`, `primaryRuneTree{…}`, `secondaryRuneTree{…}`（id 例: keystone 8010=征服者, primary 8000=栄華, secondary 8400=不滅）。

**`summonerSpells`:** `summonerSpellOne{ displayName, rawDisplayName, rawDescription }`, `summonerSpellTwo{…}`。
`rawDisplayName` 例 `"GeneratedTip_SummonerSpell_SummonerFlash_DisplayName"` → `SummonerFlash` 抽出で DataDragon spell key に。**残クールダウンは取得不可。**

**個別エンドポイント**（`?riotId=<GameName%23TAG>` または `?summonerName=`。`#` は `%23` にURLエンコード）:
| エンドポイント | 返すもの |
|---|---|
| `GET /liveclientdata/playerscores?riotId=…` | `scores` だけ |
| `GET /liveclientdata/playeritems?riotId=…` | `items[]` だけ |
| `GET /liveclientdata/playermainrunes?riotId=…` | `runes`（メイン）だけ |
| `GET /liveclientdata/playersummonerspells?riotId=…` | `summonerSpells` だけ |

### 2.3 `/liveclientdata/eventdata`

`{ Events: [ {EventID:int, EventName:str, EventTime:float, …name固有…}, … ] }`

実測のイベント種別と固有フィールド（観戦・26:31時点・全51件）:
| EventName | 固有フィールド | 件数 |
|---|---|---|
| `GameStart` | — | 1 |
| `MinionsSpawning` | — | 1 |
| `FirstBlood` | `Recipient` | 1 |
| `ChampionKill` | `KillerName`, `VictimName`, `Assisters[]` | 37 |
| `Multikill` | `KillerName`, `KillStreak` | 5 |
| `Ace` | `Acer`, `AcingTeam` | 1 |
| `FirstBrick` | `KillerName` | 1 |
| `TurretKilled` | `KillerName`, `TurretKilled`(例`Turret_TChaos_L2_P3_…`), `Assisters[]` | 4 |

> ### ★ 重大: objective モンスターのキルイベントが出ない
> **`DragonKill` / `HordeKill`(ヴォイドグラブ) / `BaronKill` / `HeraldKill` / `AtakhanKill` / `InhibKilled` は 0 件**。
> GameStart も FirstBlood も入っている（=取りこぼしではない）のに objective だけ欠落。**15:23 時点・26:31 時点の両方で確認** → **観戦時の API はこれらを emit しない**（既知の「`/eventdata` は replay/spectator で疎」を確証）。
> **帰結:** ドラゴン/グラブ/バロン/ヘラルドの数は **イベント由来では構造的に取得不可** → **ネイティブHUDのOCRが唯一の正確な手段**（CS と同方式）。
> タワーは `TurretKilled` で取れるが、`KillerName` がミニオン（`Minion_T100…`）のこともあり**チーム帰属の判定に注意**（`Turret_TChaos…`=赤側タワーが壊された→青の戦果、で判定する方が堅い）。

### 2.4 `/liveclientdata/gamestats`

`{ gameMode:str("CLASSIC"), gameTime:float(秒), mapName:str("Map11"), mapNumber:int(11), mapTerrain:str("Default") }`

### 2.5 `/liveclientdata/activeplayer` 群（観戦では 400）

観戦では4本とも **HTTP 400** `{errorCode:"RPC_ERROR", httpStatus:400, message:"Spectator mode doesn't currently support this feature"}`（`activeplayername` だけ `"Unknown"` を 200 で返す）。

| エンドポイント | ライブ（自分プレイ中）で返すもの | 観戦 |
|---|---|---|
| `/activeplayer` | `championStats`（**HP/MP/全戦闘ステータス**）, `abilities`, `currentGold`✱, `fullRunes`, `level`, `summonerName` | ❌400 |
| `/activeplayerabilities` | Q/W/E/R/Passive のレベル・名前 | ❌400 |
| `/activeplayerrunes` | 全ルーンページ（perk一覧） | ❌400 |
| `/activeplayername` | 自分の riotId | `"Unknown"` |

> **HP/MP の所在はここだけ。** `activeplayer.championStats` に `currentHealth` / `maxHealth` / `resourceValue` / `resourceMax` / `resourceType` ほか全戦闘ステータス（AD/AP/armor/MR/AS/MS/abilityHaste/各種貫通…）が入る——が **(a) 自分1人分のみ (b) 観戦では 400**。よって **放送（観戦）では全選手のHP/MPは API から一切取れない**（gold/XP と同じ構造的欠落）。
> ✱ `currentGold` も active player 限定 → 観戦では取れない（チーム合計goldはOCR、個人goldは推定）。
> ※ championStats の全項目は Swagger 未定義かつ観戦で叩けないため、Riot公開仕様ベース。ライブ環境で要実検証。

---

## 3. Replay API（観戦/リプレイで利用可）

`/replay/*` は観戦・リプレイのディレクター機能。**観戦放送でそのまま使える**（本スキャンも観戦で 200）。GET で現在値、POST で設定（`/replay/banners` のみ PUT）。

### 3.1 `/replay/render` — ★カメラ＋HUD制御（66フィールド）

GET/POST。**O/U/N キー送出（`InputUtils`）の代替**になりうる最重要エンドポイント。HUD要素を**APIで精密に出し入れ**できる。

**HUD/インターフェーストグル（bool）:**
`interfaceAll`, `interfaceFrames`(選手フレーム), `interfaceScoreboard`(詳細SB), `interfaceScore`(上部スコア), `interfaceMinimap`, `interfaceTimeline`, `interfaceNeutralTimers`(**オブジェクトタイマー**), `interfaceChat`, `interfaceAnnounce`, `interfaceKillCallouts`, `interfaceQuests`, `interfaceReplay`, `interfaceTarget`
**ヘルスバー（bool）:** `healthBarChampions`, `healthBarMinions`, `healthBarStructures`, `healthBarWards`, `healthBarPets`
**ワールド表示（bool）:** `champions`, `characters`, `minions`, `particles`, `environment`, `fogOfWar`, `floatingText`, `banners`, `outlineHover`, `outlineSelect`
**カメラ:** `cameraMode`(`"top"`等), `cameraPosition{x,y,z}`, `cameraRotation{x,y,z}`, `cameraAttached`, `cameraLockX/Y/Z`, `cameraMoveSpeed`, `cameraLookSpeed`, `fieldOfView`, `nearClip`, `farClip`, `selectionName`, `selectionOffset{x,y,z}`
**ポストプロセス:** `depthOfField*`(Enabled/Near/Mid/Far/Width/Circle/Debug), `depthFog*`/`heightFog*`(Enabled/Start/End/Intensity/Color{r,g,b,a}), `skybox*`(Path/Rotation/Radius/Offset), `sunDirection{x,y,z}`, `navGridOffset`, `simulateAllParticlesWhileOffScreen`

> **活用案（本プロジェクト）:** `POST /replay/render {"interfaceFrames":false, "interfaceNeutralTimers":false, "interfaceScoreboard":true}` のように、**キーストローク不要で**「選手フレームとネイティブのオブジェクトタイマーは消し、OCR用の詳細SBは残す」を確実に実現できる。現状の `UseAutoInitUI`（O/U/N送出）より堅牢。
> ⚠️ ただし詳細SBやヘルスバーを**ゲーム側で消すと dxcam も読めなくなる**（OCRのソースが消える）。「OCRで読む要素はゲーム側で残し、OBS側でオーバーレイで覆う」原則は維持すること。

### 3.2 `/replay/playback`

GET/POST。`{ length:float(総尺秒), time:float(現在秒), paused:bool, seeking:bool, speed:float }`。
POST で再生/一時停止/シーク/速度変更（既に本アプリのリプレイ制御で使用）。

### 3.3 `/replay/recording`

GET/POST。`{ recording:bool, path:str, codec:"webm", width:int(1280), height:int(720), framesPerSecond:int(60), lossless:bool, enforceFrameRate:bool, startTime:float, endTime:float, currentTime:float, replaySpeed:float }`。API から**動画録画**を制御可能。

### 3.4 `/replay/sequence`

GET/POST。`/replay/render` の各パラメータを **キーフレーム配列**で時間補間（ディレクターのカメラ演出）。各キー = `{ time:float, value, blend:EasingType }`。未設定時は全 null。

### 3.5 `/replay/game` / `/replay/banners` / `/replay/particles`

- `/replay/game` (GET): `{ processID:int }`
- `/replay/banners` (GET/PUT): `{ visible:bool }`
- `/replay/particles` (GET/POST): パーティクル制御

---

## 4. RPC / 内省（`/Help` `/Subscribe` async）

REST の下層にある RPC 機構（POST）。本プロジェクトでは通常不要だが完全性のため:
- `POST /Help` → `BindingFullApiHelp { functions[], types[], events[] }`：**全RPC関数・型・イベントの自己記述**（REST に出ない関数まで列挙される深掘り口）。
- `POST /Subscribe` / `Unsubscribe`：イベント購読（コールバック）。
- `POST /AsyncResult` / `AsyncStatus` / `AsyncDelete`, `GET /async/v1/result|status/{token}`：非同期呼び出し。
- `POST /Cancel`, `POST /Exit`。

---

## 5. データ可否マトリクス（本プロジェクト向け結論）

| 欲しいデータ | API 観戦可否 | 取得元 / 代替 |
|---|---|---|
| チャンピオン / スキン | ✅ | `playerlist.championName` / `rawChampionName` |
| レベル | ✅ | `playerlist.level` |
| アイテム6＋トリンケット | ✅ | `playerlist.items[]` |
| サモナースペル（種類） | ✅ | `playerlist.summonerSpells`（アイコン化可） |
| サモスペ残クールダウン | ❌ | API に無し |
| ルーン（メイン） | ✅ | `playerlist.runes` |
| ルーン全ページ | ❌(観戦) | `activeplayerrunes`=400 |
| KDA | ✅ | `playerlist.scores` |
| CS | △ | `scores.creepScore` は**10刻み丸め** → 正確値は**OCR**で補完済 |
| ward score | ✅ | `playerlist.scores.wardScore` |
| 死亡/復活タイマー | ✅ | `playerlist.isDead` / `respawnTimer` |
| 位置（レーン） | ✅ | `playerlist.position` |
| チャンプ画面座標 | △ | `screenPositionCenter`（カメラに映る時のみ・FLT_MAXで未計算） |
| **HP / MP（現在値）** | ❌ | `activeplayer.championStats`=**観戦400**。全員分は構造的に不可 → バー塗りのピクセル計測 or 非表示 |
| **個人 gold** | ❌ | API に無し（`currentGold`は active 限定）→ 推定値表示（方針例外） |
| **チーム合計 gold** | ❌ | API に無し → 上部HUDの**OCR** |
| **XP** | ❌ | API に一切無し（既調査） |
| **与ダメージ** | ❌ | API に無し |
| **ドラゴン/グラブ/バロン/ヘラルド 数** | ❌ | `/eventdata` に **emit されない** → ネイティブHUDの**OCR**（要実装） |
| タワー数 | △ | `/eventdata TurretKilled` で取れる（killer帰属に注意） |
| ゲーム時間/モード/マップ | ✅ | `gamestats` |
| イベント（kill/ace等） | ✅ | `eventdata`（ただし objective monster は除く） |
| **HUD/カメラ制御** | ✅ | `POST /replay/render`（キー送出より堅牢） |
| 再生制御 / 録画 | ✅ | `/replay/playback` / `/replay/recording` |

**要点:** 観戦放送で API から取れるのは **静的・スコア系**（チャンプ/レベル/アイテム/スペル種別/KDA/ward/死亡/位置）。**リアルタイム数値系（HP・MP・gold・XP・damage）と objective モンスター数は API では取れず**、OCR か推定か非表示で対処する——これが本フォークの「厳格精度・メモリ読み無し」設計の制約境界。

---

## 6. 実装上の注意

- **TLS:** 自己署名 → `curl -k`、Python は `ssl.CERT_NONE`。
- **ロケール:** `championName`/`displayName` 等はゲームのロケール（今回日本語）。アイコン解決は `raw*Name`（英語キー）から行う。
- **エンコード:** UTF-8。コンソール出力は `PYTHONIOENCODING=utf-8` 推奨（cp932 で文字化け/サロゲート崩れ）。
- **ポーリング:** Live Client Data はステートレス（毎回フルスナップショット）。`/eventdata` は累積（全履歴を返す）。
- **観戦判定:** `activeplayer` が 400 を返す＝観戦。`/replay/game` が 200＝Replay API 利用可（観戦/リプレイ）。
- **再スキャン:** `ocr-poc/overlay-harness/scan_api.py`（生レスポンスは `api_scan_raw.json`、いずれも gitignore 対象）。パッチでスキーマが変わったら再実行。

---

*生成: 2026-06-13 / 対象パッチの DataDragon ≈ 16.12.1 / Summoner's Rift（Map11）観戦セッション実測。*
