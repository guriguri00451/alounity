# スマホコントローラー実装計画

## 概要

Unityゲームのコントローラーとしてスマホを活用する。スマホのジャイロセンサー・加速度センサーの値をsocket.ioで通信し、Unity側に送信する。

## 役割別センサーマッピング

| 役割 | 使用センサー | 検出する動作 | Unity側での用途 |
|------|-------------|-------------|----------------|
| 右オール | DeviceMotionEvent (acceleration) | スマホを振る動作 | カヤック右側の推進力 |
| 左オール | DeviceMotionEvent (acceleration) | スマホを振る動作 | カヤック左側の推進力 |
| 釣り | DeviceOrientationEvent + DeviceMotionEvent | 方位角＋振りかぶり＋キャスト＋引き上げ | 釣り針の方向制御、キャスト、リール |

## 釣りアクションのステートマシン

```
Idle → Waiting（キャスト入力：振りかぶり検出）
     → Catching（キャスト実行：前に投げる）
     → Swinging（魚ヒット：魚を振り回す）
     → Idle（攻撃完了 / 魚を手放す）
```


## ディレクトリ構成

```
alounity/
├── Assets/                    # Unityプロジェクト（既存）
├── controller/                # Next.jsプロジェクト（新規）
│   ├── src/
│   │   ├── app/
│   │   │   ├── layout.tsx
│   │   │   ├── page.tsx       # コントローラー画面
│   │   │   └── api/
│   │   │       └── socket/
│   │   │           └── route.ts  # Socket.IO統合用
│   │   ├── components/
│   │   │   ├── SensorDisplay.tsx    # センサー値表示コンポーネント
│   │   │   └── PermissionRequest.tsx # iOS権限リクエスト
│   │   ├── hooks/
│   │   │   ├── useDeviceMotion.ts   # DeviceMotionEvent + DeviceOrientationEventフック
│   │   │   └── useSocket.ts         # Socket.IO通信フック
│   │   └── lib/
│   │       └── socket-client.ts     # Socket.IOクライアント
│   ├── server/
│   │   └── index.ts           # カスタムサーバー（Socket.IO統合）
│   ├── public/
│   ├── package.json
│   ├── tsconfig.json
│   ├── next.config.ts
│   └── certs/                 # mkcert証明書（.gitignore）
├── .gitignore
└── ...
```

## 技術スタック

| 層 | 技術 | バージョン |
|---|---|---|
| フロントエンド | Next.js (App Router), React 19 | 16.2.9 |
| リアルタイム通信 | Socket.IO (client: `socket.io-client`, server: `socket.io`) | 4.8.3 |
| センサー API | Device Orientation Events（`DeviceMotionEvent` / `DeviceOrientationEvent`） | - |
| HTTPS | mkcert（ローカル開発用） | - |
| 言語 | TypeScript | - |

## 通信アーキテクチャ

**Next.jsサーバー経由方式**を採用。

```
スマホ（ブラウザ）
    ↓ Socket.IO（WebSocket）
Next.jsサーバー（controller/）
    ↓ Socket.IO（WebSocket）
Unity
```

### 選定理由

- 開発すべきサーバーが1つで済む
- Next.jsのAPI RoutesでSocket.IOサーバーを同居させられる
- ルーム管理、プレイヤー管理などのゲームロジックをJavaScript/TypeScriptで統一できる
- スマホ側のUI（ボタン表示やステータス表示）もNext.jsで実装できる

## Socket.IOイベント設計

| イベント名 | 方向 | データ |
|---|---|---|
| `controller:connect` | スマホ → サーバー | `{ playerId, controllerType }` |
| `controller:sensor` | スマホ → サーバー | `{ playerId, accel: {x,y,z}, rotation: {alpha,beta,gamma}, timestamp }` |
| `server:ack` | サーバー → スマホ | `{ received: true }` |
| `unity:connect` | Unity → サーバー | `{ roomId }` |
| `sensor:data` | サーバー → Unity | `{ playerId, accel, rotation, timestamp }` |
| `room:state` | サーバー → 全員 | `{ players: [...], status }` |

## 実装フェーズ

### Phase 1: プロジェクトセットアップ

#### 1-1. Next.jsプロジェクト作成

- `controller/` 配下にNext.js 15をセットアップ
- TypeScript, App Router, Tailwind CSS
- 依存関係追加: `socket.io`, `socket.io-client`

#### 1-2. カスタムサーバー構成

- Socket.IOをNext.jsと統合するため、カスタムサーバー（`server/index.ts`）を作成
- Next.jsのデフォルトサーバーではSocket.IOのWebSocket接続を扱えないため、`next` + `http.Server` + `socket.io` を組み合わせたカスタムサーバーを使用
- ポート: 3000（HTTPS）

#### 1-3. HTTPS環境構築（mkcert）

- `mkcert`をHomebrewでインストール
- ローカルCAを作成
- `localhost` と `*.local`（スマホからのアクセス用）の証明書を作成
- `certs/` ディレクトリに保存（`.gitignore`に追加）
- Next.jsカスタムサーバーでHTTPS化

### Phase 2: センサーデータ取得

#### 2-1. iOS権限リクエスト実装

- `components/PermissionRequest.tsx`
- iOS 13+では `DeviceMotionEvent.requestPermission()` でユーザー許可が必要
- ボタンタップで権限リクエスト→許可後にセンサーデータ取得開始

#### 2-2. センサーイベントフック実装

- `hooks/useDeviceMotion.ts`
- 取得データ:
  - **加速度**: `acceleration` (x, y, z) [m/s²]（DeviceMotionEvent）
  - **加速度（重力含む）**: `accelerationIncludingGravity` (x, y, z)（DeviceMotionEvent）
  - **回転角速度**: `rotationRate` (alpha, beta, gamma) [deg/s]（DeviceMotionEvent）
  - **端末の向き**: `alpha, beta, gamma` [度]（DeviceOrientationEvent）
- 送信頻度: 約60Hz（`devicemotion` イベントの発火頻度に依存）
- デバウンス/スロットル処理で送信間隔を調整（例: 30fpsに制限）

#### 2-3. センサー値表示コンポーネント

- `components/SensorDisplay.tsx`
- リアルタイムで加速度・ジャイロの値を画面に表示
- デバッグ用。数値＋簡易バーで可視化

### Phase 3: Socket.IO通信

#### 3-1. Socket.IOサーバー実装

- `server/index.ts` 内にSocket.IOサーバーを統合
- CORS設定
- 接続/切断イベントのハンドリング

#### 3-2. Socket.IOクライアント（スマホ側）

- `hooks/useSocket.ts`
- サーバーに接続、センサーデータをemit
- 再接続ロジック含む

#### 3-3. プレイヤー管理

- 接続時にプレイヤーIDを割り当て（UUID）
- ルーム概念: 同一ルーム内のプレイヤーのセンサーデータをUnityに転送

### Phase 4: 動作確認・最適化

#### 4-1. スマホからのアクセス確認

- PCとスマホを同一LANに接続
- `https://<PCのローカルIP>:3000` でアクセス
- mkcertの証明書をスマホに信頼させる必要がある場合がある

#### 4-2. 通信最適化

- センサーデータの送信間隔調整（30fps程度を推奨）
- バイナリ送信の検討（Socket.IOはバイナリ対応済み）
- 圧縮オプション有効化

#### 4-3. Unity側接続（次回以降）

- UnityからSocket.IOクライアントとして接続（`SocketIoClientDotNet`や`NativeWebSocket`等）
- サーバーから受信したセンサーデータをUnity側で処理

## 最初の実装マイルストーン

1. Next.jsプロジェクト作成 & 依存関係インストール
2. カスタムサーバー（HTTPS + Socket.IO）セットアップ
3. ページ作成（コントローラー画面のUI）
4. センサーイベント実装（権限リクエスト + センサー値取得）
5. センサー値を画面に表示
6. Socket.IOでサーバーにセンサーデータ送信
7. サーバー側で受信確認（コンソール出力）
