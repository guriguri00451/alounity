# alounity

Unityゲーム + スマホコントローラー。複数プレイヤーがスマホをコントローラーとして使用し、センサーデータをSocket.IOで送信する。

## アーキテクチャ

```
alounity/
├── Assets/           # Unityプロジェクト（既存）
├── controller/       # スマホコントローラー用Next.jsアプリ（計画中）
└── docs/             # プロジェクトドキュメント
```

**通信フロー:**
```
スマホブラウザ → Socket.IO → Next.jsサーバー → Socket.IO → Unity
```

## 技術スタック

| 構成要素 | 技術 |
|---------|------|
| ゲームエンジン | Unity 6 (6000.3.17f1) + URP |
| コントローラーアプリ | Next.js 15 (App Router), React 19, TypeScript |
| リアルタイム通信 | Socket.IO |
| センサーAPI | DeviceMotionEvent + DeviceOrientationEvent |
| HTTPS（開発用） | mkcert |

## コーディング規約

### 共通
- ドキュメント・コメントは日本語で記述
- センサーデータ: 加速度 (m/s²)、回転角速度 (deg/s)、端末の向き (度)

### Next.js (controller/)
- App Routerを使用（Pages Routerではない）
- Socket.IO統合にはカスタムサーバーが必要（Next.jsのデフォルトサーバーはWebSocket非対応）
- センサーAPI使用コンポーネントには `"use client"` 必須
- サーバーコンポーネントとクライアントコンポーネントを明確に分離

### Socket.IO
- イベント命名規則: `送信元:イベント名`（例: `controller:connect`, `sensor:data`）
- センサーデータ送信は30fpsにスロットル
- 再接続ロジックを必ず実装

### Unity (Assets/)
- Unity 6 (6000.3.17f1) を使用
- Universal Render Pipeline (URP) 適用済み
- C# の命名規則に従う（PascalCase, camelCase）

## 開発コマンド

**Unity:** Unity Editor (バージョン 6000.3.17f1) を使用

**コントローラー（計画中）:**
```bash
cd controller
npm install
npm run dev        # Socket.IO付きNext.js開発サーバーを起動
```

## 注意事項

- iOS 13+ではセンサーアクセスに `DeviceMotionEvent.requestPermission()` が必要
- DeviceMotion/DeviceOrientation APIにはHTTPSが必要（Secure Context）
- センサーデータは約30fpsにスロットルして応答性とネットワーク負荷のバランスを取る
- 詳細な実装計画は `docs/controller-implementation-plan.md` を参照

## Socket.IO イベント設計

| イベント名 | 方向 | データ |
|---|---|---|
| `controller:connect` | スマホ → サーバー | `{ playerId, controllerType }` |
| `controller:sensor` | スマホ → サーバー | `{ playerId, accel, rotation, orientation, timestamp }` |
| `sensor:data` | サーバー → Unity | `{ playerId, accel, rotation, orientation, timestamp }` |
| `room:state` | サーバー → 全員 | `{ players: [...], status }` |

## センサーデータ形式

```typescript
{
  accel: { x: number, y: number, z: number },           // 加速度 [m/s²]
  rotation: { alpha: number, beta: number, gamma: number }, // 回転角速度 [deg/s]
  orientation: { alpha: number, beta: number, gamma: number }, // 端末の向き [度]
  timestamp: number
}
```
