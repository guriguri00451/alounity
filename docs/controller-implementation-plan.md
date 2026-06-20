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
│   │   │   ├── page.tsx       # ルームID入力画面
│   │   │   └── room/
│   │   │       └── [roomId]/
│   │   │           └── page.tsx  # コントローラー画面（役割選択・センサー送信）
│   │   ├── components/        # UIコンポーネント
│   │   │   ├── PermissionRequest.tsx  # iOS権限リクエスト
│   │   │   ├── RoleSelector.tsx       # 役割選択
│   │   │   ├── SensorDebugOverlay.tsx # デバッグオーバーレイ
│   │   │   └── SensorDisplay.tsx      # センサー値表示
│   │   ├── hooks/             # カスタムフック
│   │   │   ├── useDeviceMotion.ts  # センサーデータ取得
│   │   │   └── useSocket.ts        # Socket.IO接続管理
│   │   └── lib/
│   │       └── types.ts       # 型定義
│   ├── server/
│   │   └── index.ts           # カスタムサーバー（Socket.IO統合、HTTPS対応、ルーム管理）
│   ├── scripts/
│   │   └── setup-https.sh     # HTTPS証明書生成スクリプト
│   ├── public/
│   ├── package.json
│   ├── tsconfig.json
│   ├── next.config.ts
│   └── certs/                 # mkcert証明書（.gitignore）
├── docs/
│   ├── room-management-plan.md
│   └── qr-code-implementation.md
├── .gitignore
└── ...
```

## 技術スタック

| 層 | 技術 | バージョン |
|---|---|---|
| フロントエンド | Next.js 16.x (App Router), React 19 | 16.2.9 |
| リアルタイム通信 | Socket.IO (client: `socket.io-client`, server: `socket.io`) | 4.8.3 |
| センサー API | Device Orientation Events（`DeviceMotionEvent` / `DeviceOrientationEvent`） | - |
| HTTPS | mkcert（ローカル開発用） | - |
| 言語 | TypeScript | - |
| Unity Socket.IO | SocketIOClient (doghappy) | 3.1.2 / 4.0.4 |

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

### ホスト（Unity）用イベント

| イベント名 | 方向 | データ |
|---|---|---|
| `host:create` | Unity → サーバー | `{}` |
| `host:create_ack` | サーバー → Unity | `{ ok: boolean, roomId?: string, error?: string }` |
| `host:close` | Unity → サーバー | `{ roomId: string }` |

### コントローラー（スマホ）用イベント

| イベント名 | 方向 | データ |
|---|---|---|
| `room:exists` | スマホ → サーバー | `{ roomId: string }` |
| `room:exists_ack` | サーバー → スマホ | `{ exists: boolean }` |
| `controller:connect` | スマホ → サーバー | `{ roomId: string, role: string }` |
| `server:ack` | サーバー → スマホ | `{ received: boolean, playerId?: string, error?: string }` |
| `controller:sensor` | スマホ → サーバー | `{ roomId, role, accel, rotation, orientation, timestamp }` |

### 共通イベント

| イベント名 | 方向 | データ |
|---|---|---|
| `sensor:data` | サーバー → Unity | `{ playerId, role, accel, rotation, orientation, timestamp }` |
| `room:closed` | サーバー → スマホ | `{ roomId: string, reason: string }` |

### ルーム管理

- サーバーが `Map<string, RoomState>` でアクティブルームをインメモリ管理
- ルームIDはサーバー側で採番（6文字英数字、I/O/0/1除外）
- ホスト（Unity）切断時に自動でルーム削除
- 存在しないルームへの接続は `server:ack { received: false }` で拒否

## 実装フェーズ

### Phase 1: プロジェクトセットアップ

#### 1-1. Next.jsプロジェクト作成 ✅ 完了

- `controller/` 配下にNext.js 16.xをセットアップ
- TypeScript, App Router, Tailwind CSS
- 依存関係追加: `socket.io`, `socket.io-client`

#### 1-2. カスタムサーバー構成 ✅ 完了

- Socket.IOをNext.jsと統合するため、カスタムサーバー（`server/index.ts`）を作成
- Next.jsのデフォルトサーバーではSocket.IOのWebSocket接続を扱えないため、`next` + `http.Server` + `socket.io` を組み合わせたカスタムサーバーを使用
- ポート: 3000
- Socket.IOイベント: `controller:connect`, `controller:sensor`, `sensor:data`, `unity:connect`
- ルーム機能: `roomId`ベースのルーム管理

#### 1-3. HTTPS環境構築（mkcert） ✅ 完了

- `scripts/setup-https.sh` で証明書生成スクリプトを作成
- カスタムサーバーでHTTPS対応を実装（`HTTPS=true`環境変数でHTTPSモード起動）
- `npm run setup:https` でmkcertを使用した証明書生成
- `npm run dev:https` でHTTPSサーバーを起動
- `certs/` ディレクトリに証明書を保存（`.gitignore`に追加済み）
- localhost、127.0.0.1、ローカルIPアドレス用の証明書を生成

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

#### 3-4. Unity側Socket.IOクライアント実装 ⏳ 実装済み

**技術選定:** SocketIOClient（ https://github.com/doghappy/socket.io-client-csharp ）

`SocketIoClientDotNet`（Quobject）は2019年でメンテナンス停止、依存DLLが`.NET Standard 2.0`未対応だったため、現在もメンテナンスが続く`SocketIOClient`（doghappy）に変更。

**特徴:**
- Socket.IO v2/v3/v4 完全対応（サーバーv4と一致）
- .NET Standard 2.0 対応
- 本体は `SocketIOClient.dll` 1つだが、内部で `Microsoft.Extensions.DependencyInjection` / `System.Text.Json` 等に依存
- 依存DLLは NuGet から netstandard2.0 版を取得して `Assets/Plugins/` に一括配置が必要（計13ファイル）
- デフォルトシリアライザ: System.Text.Json
- Newtonsoft.Json シリアライザも選択可能（別パッケージ）

**作成ファイル:**
```text
Assets/alounity/
└── Scripts/
    └── Network/
        ├── SocketIOManager.cs      # Socket.IO接続管理（Singleton）
        ├── SensorDataReceiver.cs   # センサーデータ受信・適用
        └── SensorDataModels.cs     # データモデル定義
```

**SocketIOClientの導入方法:**
1. NuGetから`SocketIOClient`パッケージをダウンロード
2. `lib/netstandard2.0/SocketIOClient.dll` を `Assets/Plugins/` に配置
3. Unity Editorをリフレッシュ

**使用例:**
```csharp
using SocketIOClient;
using Cysharp.Threading.Tasks;

var socket = new SocketIO("http://localhost:3000");

// 接続完了時のハンドラ（イベントベース）
socket.OnConnected += async (sender, e) => {
    Debug.Log("Connected to server");
    await socket.EmitAsync("host:create");
};

// イベント受信ハンドラ（v4.x API: IEventContext を使用）
socket.On("host:create_ack", async (IEventContext response) => {
    var data = response.GetValue<HostCreateAckPayload>(0);
    if (data.ok) {
        Debug.Log($"Room created: {data.roomId}");
        await UniTask.SwitchToMainThread(); // Unity APIを使う場合はメインスレッドに切り替え
    }
});

await socket.ConnectAsync();
```

**注意:** SocketIOClient v4.x ではコールバックがバックグラウンドスレッドで実行されるため、
Unity API（`Texture2D`生成、`Debug.Log`など）を使う前に `UniTask.SwitchToMainThread()` で
メインスレッドに切り替える必要があります。

**websocket-sharpが非推奨の理由:**
- Socket.IOプロトコル未対応
- サーバー側に raw WebSocket のエンドポイント追加が必要（工数増）
- ルーム管理・再接続を自前実装

### Phase 4: 動作確認・最適化

#### 4-1. スマホからのアクセス確認

- PCとスマホを同一LANに接続
- `https://<PCのローカルIP>:3000` でアクセス
- mkcertの証明書をスマホに信頼させる必要がある場合がある

#### 4-2. 通信最適化

- センサーデータの送信間隔調整（30fps程度を推奨）
- バイナリ送信の検討（Socket.IOはバイナリ対応済み）
- 圧縮オプション有効化

#### 4-3. Unity側接続

- Phase 3の3-4で実装済み
- SocketIoClientDotNetを使用してサーバーから受信したセンサーデータをUnity側で処理

### Phase 5: ルーム管理 ✅ 完了

#### 5-1. サーバー側ルーム管理

- `Map<string, RoomState>` でアクティブルームをインメモリ管理
- `generateRoomId()` で6文字英数字を生成（I/O/0/1除外、30⁶通り）
- `host:create` / `host:create_ack` イベントでルーム作成（サーバー採番）
- `host:close` イベントでルーム閉鎖
- `room:exists` / `room:exists_ack` イベントで事前存在確認
- ホスト切断時に自動ルーム削除

#### 5-2. Unity側ルーム作成

- `SocketIOManager` で `host:create` を送信 → サーバーから `roomId` を受信
- `ServerUri` プロパティでサーバーURIを公開（QRコード生成用）
- `OnRoomCreated` イベントでルーム作成完了を通知
- UniTaskでメインスレッド切り替えを実装

#### 5-3. コントローラー側ルーム参加

- `/` ページ: ルームID入力フォーム（6文字、大文字自動変換）
- `/room/[roomId]` ページ: コントローラー画面（役割選択 → センサー送信）
- `room:exists` で事前検証 → 存在しないルームはエラー表示

### Phase 6: QRコード生成 ✅ 完了

#### 6-1. QRコードエンコーダー導入

- UniQRCode（MIT License）から `QREncoder.cs` を取得
- `Assets/alounity/contributors/rita/Scripts/QR/` に配置

#### 6-2. QRコード生成・表示

- `QRCodeGenerator.cs`: 文字列から `Texture2D` を生成するstaticユーティリティ
- `QRCodeDisplay.cs`: MonoBehaviourでUIにQRコードを表示
- `SocketIOManager.OnRoomCreated` を購読して自動表示
- 生成URL形式: `http://{host}:{port}/room/{roomId}`

詳細: `docs/qr-code-implementation.md` を参照

## 最初の実装マイルストーン

1. Next.jsプロジェクト作成 & 依存関係インストール
2. カスタムサーバー（HTTPS + Socket.IO）セットアップ
3. ページ作成（コントローラー画面のUI）
4. センサーイベント実装（権限リクエスト + センサー値取得）
5. センサー値を画面に表示
6. Socket.IOでサーバーにセンサーデータ送信
7. サーバー側で受信確認（コンソール出力）
