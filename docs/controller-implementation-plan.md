# スマホコントローラー仕様書

## 概要

Unityゲームのコントローラーとしてスマホを活用する。スマホのジャイロセンサー・加速度センサーの値をSocket.IOで通信し、Unity側に送信する。

## システムアーキテクチャ

```
スマホブラウザ → Socket.IO → Next.jsサーバー → Socket.IO → Unity
```

**通信方式:** Socket.IOによる双方向リアルタイム通信

### 通信フロー詳細

1. Unityがサーバーにルームを作成（`host:create`）
2. サーバーがルームIDを発行（6文字英数字）
3. UnityがQRコードにルームURLをエンコードして表示
4. スマホでQRコードを読み取り、ルームページにアクセス
5. スマホがルームに参加（`controller:connect`）
6. スマホがセンサーデータを送信（`controller:sensor`、30fps）
7. サーバーがデータをUnityに転送（`sensor:data`）

## 技術スタック

| 層 | 技術 | バージョン |
|---|---|---|
| ゲームエンジン | Unity 6 + URP | 6000.3.17f1 |
| コントローラーアプリ | Next.js (App Router) | 16.2.9 |
| リアルタイム通信 | Socket.IO | 4.8.3 |
| Socket.IO クライアント (Unity) | SocketIOClient (doghappy) | 4.0.4 |
| センサー API | DeviceMotionEvent / DeviceOrientationEvent | - |
| QRコード生成 | UniQRCode | - |
| HTTPS（開発用） | mkcert | - |
| 言語 | TypeScript / C# | - |

## ディレクトリ構成

```
alounity/
├── Assets/alounity/            # Unityプロジェクト
│   └── contributors/
│       ├── rita/Scripts/       # rita の実装
│       │   ├── SocketIOManager.cs          # Socket.IO接続管理（Singleton）
│       │   ├── SensorDataReceiver.cs       # センサーデータ受信・振り分け
│       │   ├── SensorDataModels.cs         # データモデル定義
│       │   └── QR/                         # QRコード生成
│       └── soma/Scripts/       # soma の実装
│           ├── SmartphoneInputBridge.cs     # センサー→InputSystem変換
│           └── ...
├── controller/                 # Next.jsコントローラーアプリ
│   ├── src/
│   │   ├── app/
│   │   │   ├── layout.tsx
│   │   │   ├── page.tsx                    # ルームID入力画面
│   │   │   └── room/[roomId]/page.tsx      # コントローラー画面
│   │   ├── components/
│   │   │   ├── PermissionRequest.tsx       # iOS権限リクエスト
│   │   │   ├── RoleSelector.tsx            # 役割選択（チーム対応）
│   │   │   ├── TeamSelector.tsx            # チーム選択（versusモード）
│   │   │   ├── SensorDebugOverlay.tsx      # デバッグオーバーレイ
│   │   │   └── SensorDisplay.tsx           # センサー値表示
│   │   ├── hooks/
│   │   │   ├── useDeviceMotion.ts          # センサーデータ取得
│   │   │   └── useSocket.ts                # Socket.IO接続管理
│   │   └── lib/
│   │       └── types.ts                    # 型定義
│   └── server/
│       └── index.ts                        # カスタムサーバー（Socket.IO + ルーム管理）
└── docs/
    └── ...
```

## Socket.IO イベント仕様

### ホスト（Unity）用イベント

| イベント名 | 方向 | データ | 説明 |
|---|---|---|---|
| `host:create` | Unity → サーバー | `{ gameMode?: "single" \| "versus" }` | ルーム作成要求 |
| `host:create_ack` | サーバー → Unity | `{ ok: boolean, roomId?: string, gameMode?: string, error?: string }` | 作成結果 |
| `host:close` | Unity → サーバー | `{ roomId: string }` | ルーム閉鎖 |

### コントローラー（スマホ）用イベント

| イベント名 | 方向 | データ | 説明 |
|---|---|---|---|
| `room:exists` | スマホ → サーバー | `{ roomId: string }` | ルーム存在確認 |
| `room:exists_ack` | サーバー → スマホ | `{ exists: boolean, gameMode?: string, takenRoles?: Record<string, string[]> }` | ルーム情報 + チーム別占有役割 |
| `controller:connect` | スマホ → サーバー | `{ roomId: string, role: string, team: string }` | ルーム参加 |
| `server:ack` | サーバー → スマホ | `{ received: boolean, playerId?: string, error?: string }` | 参加結果 |
| `controller:sensor` | スマホ → サーバー | `{ roomId, role, team, accel, rotation, orientation, timestamp }` | センサーデータ送信 |

### 共通イベント

| イベント名 | 方向 | データ | 説明 |
|---|---|---|---|
| `sensor:data` | サーバー → Unity | `{ playerId, role, team, accel, rotation, orientation, timestamp }` | センサーデータ転送 |
| `room:closed` | サーバー → スマホ/Unity | `{ roomId: string, reason: string }` | ルーム閉鎖通知 |
| `room:players_update` | サーバー → スマホ/Unity | `{ players: { playerId, role, team }[] }` | プレイヤー変更通知 |

## 役割とチーム

### 役割（role）

| 役割 | 使用センサー | 検出する動作 | Unity側での用途 |
|------|-------------|-------------|----------------|
| `paddle_right` | DeviceMotionEvent (acceleration) | スマホを振る動作 | カヤック右側の推進力 |
| `paddle_left` | DeviceMotionEvent (acceleration) | スマホを振る動作 | カヤック左側の推進力 |
| `fisher` | DeviceOrientationEvent + DeviceMotionEvent | 方位角＋振りかぶり＋キャスト＋引き上げ | 釣り針の方向制御、キャスト、リール |

- 各役割は1チーム内で1デバイスのみ占有可能
- 重複接続はサーバーが拒否（`server:ack { received: false, error: "この役割は既に使用されています" }`）

### チーム（team）

| チーム | 説明 |
|--------|------|
| `A` | チームA。singleモードでは全員がチームA |
| `B` | チームB。versusモードのみ存在 |

- singleモード: 3人全員がチームA
- versusモード: 3vs3（チームA 3人 + チームB 3人）

## センサーデータ形式

```typescript
{
  accel: { x: number, y: number, z: number },           // 加速度 [m/s²]
  rotation: { alpha: number, beta: number, gamma: number }, // 回転角速度 [deg/s]
  orientation: { alpha: number, beta: number, gamma: number }, // 端末の向き [度]
  timestamp: number
}
```

### データ加工ルール

- 加速度: `accelerationIncludingGravity` ではなく `acceleration` を使用（重力分離済み）
- 送信頻度: 30fps（33ms間隔でスロットル）

## 釣りアクションのステートマシン

```
Idle → Waiting（キャスト入力：振りかぶり検出）
     → Catching（キャスト実行：前に投げる）
     → Swinging（魚ヒット：魚を振り回す）
     → Idle（攻撃完了 / 魚を手放す）
```

## ルーム管理仕様

### ルームのライフサイクル

1. **作成**: Unityが `host:create` を送信 → サーバーがルームIDを採番
2. **参加**: スマホが `controller:connect` を送信 → サーバーが役割重複チェック後、参加許可
3. **運用**: センサーデータの転送、プレイヤー状態のリアルタイム通知
4. **終了**: Unity切断時に自動削除、または Unityが `host:close` で明示的削除

### ルームID

- 6文字英数字（I/O/0/1除外、30⁶通り）
- サーバー側で採番

### プレイヤー管理

- サーバーが `Map<string, RoomState>` でインメモリ管理
- 各プレイヤーの役割・チームを管理（`Map<socketId, { role, team }>`）
- プレイヤー切断時に役割を自動解放
- 役割変更時は `room:players_update` で全クライアントに通知

## QRコード

- Unityがルーム作成成功時にQRコードを生成・表示
- URL形式: `http(s)://{host}:{port}/room/{roomId}`
- 使用ライブラリ: UniQRCode（`QREncoder.cs`）

## Unity側実装詳細

### SocketIOManager

- Singletonパターン
- `host:create` / `host:close` イベント送信
- `host:create_ack` イベント受信
- サーバーURIを公開（QRコード生成用）
- 再接続ロジック実装済み

### SensorDataReceiver

- `sensor:data` イベント受信
- チーム別イベント発火:
  - `onTeamASensorData` / `onTeamBSensorData`
- 役割別イベント発火:
  - `onPaddleRightInput_A` / `onPaddleRightInput_B`
  - `onPaddleLeftInput_A` / `onPaddleLeftInput_B`
  - `onFisherInput_A` / `onFisherInput_B`
- singleモードでは `_A` イベントのみ使用
- バックグラウンドスレッド対応（`SynchronizationContext` でメインスレッドにディスパッチ）

### SmartphoneInputBridge

- SensorDataReceiverのイベントをUnity InputSystemの `SmartphoneDevice` に変換
- 加速度 → パドル強度（9.8 m/s² で最大値1.0）
- 角速度 + 方位角 → 釣り操作

## 開発環境セットアップ

```bash
cd controller
npm install
npm run dev                  # HTTPモード（localhost）
HTTPS=true npm run dev       # HTTPSモード（要mkcert証明書）
```

### CI

| チェック | コマンド |
|---------|---------|
| Biome (lint + format) | `npm run check` |
| TypeScript型チェック | `npm run typecheck` |
| テスト | `npm run test:run` |
| ビルド | `npm run build` |

## 注意事項

- iOS 13+ではセンサーアクセスに `DeviceMotionEvent.requestPermission()` が必要
- DeviceMotion / DeviceOrientation APIにはHTTPSが必要（Secure Context）
- センサーデータは30fpsにスロットル
- SocketIOClient v4.x のコールバックはバックグラウンドスレッドで実行されるため、Unity APIを使う前に `UniTask.SwitchToMainThread()` でメインスレッドに切り替える必要がある
- 同一LAN内でPCとスマホを接続して動作確認
- mkcertの証明書をスマホに信頼させる必要がある場合がある
