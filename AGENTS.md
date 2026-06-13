# alounity

## ゲーム概要

**スマホカヤックゲーム** - 2チーム各3人で対戦するマルチプレイヤーゲーム

### チーム構成と役割

| 役割 | 操作 | 使用センサー |
|------|------|-------------|
| 右オール / 左オール | スマホを振ってカヤックを進める | DeviceMotionEvent（加速度） |
| 釣り | 方位磁針で狙い → 振りかぶってキャスト → 縦に振って引き上げ | DeviceOrientationEvent + DeviceMotionEvent |

### ゲームフロー
1. QRコードでスマホをUnityクライアントに接続
2. 釣りで魚を釣る
3. 釣った魚を振り回して相手チームに投げる
4. 相手チームのHPを0にしたら勝ち

---

## アーキテクチャ

Unityゲーム + スマホコントローラー。複数プレイヤーがスマホをコントローラーとして使用し、センサーデータをSocket.IOで送信する。

**通信フロー:**
```
スマホブラウザ → Socket.IO → Next.jsサーバー → Socket.IO → Unity
```

**通信方向:** スマホ → Unityの**一方向**のみ

## 技術スタック

| 構成要素 | 技術 | バージョン |
|---------|------|-----------|
| ゲームエンジン | Unity 6 + URP | 6000.3.17f1 |
| レンダリング | Universal Render Pipeline | 17.3.0 |
| 入力管理 | Input System | 1.19.0 |
| 非同期処理 | UniTask | - |
| Reactive Extensions | R3 | - |
| コントローラーアプリ | Next.js (App Router) | 16.2.9 |
| リアルタイム通信 | Socket.IO | 4.8.3 |
| センサーAPI | DeviceMotionEvent + DeviceOrientationEvent | - |
| HTTPS（開発用） | mkcert | - |

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

**コントローラー:**
```bash
cd controller
npm install
npm run dev        # Next.js開発サーバーを起動

# CI関連コマンド
npm run check      # Biome (lint + format) チェック
npm run check:fix  # Biome 自動修正
npm run typecheck  # TypeScript型チェック
npm run test:run   # Vitestテスト実行
npm run build      # プロダクションビルド
```

## CI（GitHub Actions）

`controller/` ディレクトリに変更がある場合、PR/Push時に自動で以下を実行：

1. **Biome check** - lint + format
2. **TypeScript** - 型チェック
3. **Vitest** - テスト実行
4. **Build** - ビルド確認

ワークフローファイル: `.github/workflows/ci-controller.yml`

## 注意事項

- iOS 13+ではセンサーアクセスに `DeviceMotionEvent.requestPermission()` が必要
- DeviceMotion/DeviceOrientation APIにはHTTPSが必要（Secure Context）
- センサーデータは約30fpsにスロットルして応答性とネットワーク負荷のバランスを取る
- 詳細な実装計画は `docs/controller-implementation-plan.md` を参照

## Socket.IO イベント設計

| イベント名 | 方向 | データ |
|---|---|---|
| `controller:connect` | スマホ → サーバー | `{ playerId, role }` |
| `controller:sensor` | スマホ → サーバー | `{ playerId, role, accel, rotation, orientation, timestamp }` |
| `sensor:data` | サーバー → Unity | `{ playerId, role, accel, rotation, orientation, timestamp }` |
| `room:state` | サーバー → 全員 | `{ players: [...], status }` |

### 役割（role）の種類

- `paddle_right`: 右オール担当
- `paddle_left`: 左オール担当
- `fisher`: 釣り担当

## センサーデータ形式

```typescript
{
  accel: { x: number, y: number, z: number },           // 加速度 [m/s²]
  rotation: { alpha: number, beta: number, gamma: number }, // 回転角速度 [deg/s]
  orientation: { alpha: number, beta: number, gamma: number }, // 端末の向き [度]
  timestamp: number
}
```
