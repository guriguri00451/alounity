# Socket.IO通信層開発ルール

## アーキテクチャ

```
スマホ（ブラウザ）→ Next.jsサーバー → Unity
```

**通信方向:** スマホ → Unityの**一方向**のみ

## イベント命名規則

- 形式: `送信元:イベント名`
- 例: `controller:connect`, `sensor:data`, `room:state`

## 主要イベント

| イベント | 方向 | データ |
|---------|------|--------|
| `controller:connect` | スマホ→サーバー | `{ playerId, role }` |
| `controller:sensor` | スマホ→サーバー | `{ playerId, role, accel, rotation, orientation, timestamp }` |
| `sensor:data` | サーバー→Unity | `{ playerId, role, accel, rotation, orientation, timestamp }` |
| `room:state` | サーバー→全員 | `{ players, status }` |

## 役割別データ形式

### オール担当（右オール/左オール）

加速度データを中心に送信。振る動作の強さと頻度が重要。

```typescript
{
  playerId: string,
  role: "paddle_right" | "paddle_left",
  accel: { x: number, y: number, z: number },  // 加速度 [m/s²]
  timestamp: number
}
```

### 釣り担当

方位角＋加速度＋回転角速度を送信。狙い・キャスト・引き上げの各動作に使用。

```typescript
{
  playerId: string,
  role: "fisher",
  accel: { x: number, y: number, z: number },           // 加速度 [m/s²]
  rotation: { alpha: number, beta: number, gamma: number }, // 回転角速度 [deg/s]
  orientation: { alpha: number, beta: number, gamma: number }, // 端末の向き [度]
  timestamp: number
}
```

## センサーデータ形式

```typescript
{
  accel: { x: number, y: number, z: number },           // 加速度 [m/s²]
  rotation: { alpha: number, beta: number, gamma: number }, // 回転角速度 [deg/s]
  orientation: { alpha: number, beta: number, gamma: number }, // 端末の向き [度]
  timestamp: number
}
```

## 最適化

- センサーデータ送信は30fpsにスロットル
- バイナリ送信を検討（Socket.IOはバイナリ対応）
- 圧縮オプション有効化

## 注意事項

- 日本語でコメント記述
- 再接続ロジックを必ず実装
- ルーム概念でプレイヤーを管理
- 通信はスマホ→Unityの一方向のみ（Unity→スマホの送信は不要）
