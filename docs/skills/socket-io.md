# Socket.IO通信層開発ルール

## アーキテクチャ

```
スマホ（ブラウザ）→ Next.jsサーバー → Unity
```

## イベント命名規則

- 形式: `送信元:イベント名`
- 例: `controller:connect`, `sensor:data`, `room:state`

## 主要イベント

| イベント | 方向 | データ |
|---------|------|--------|
| `controller:connect` | スマホ→サーバー | `{ playerId, controllerType }` |
| `controller:sensor` | スマホ→サーバー | `{ playerId, accel, rotation, timestamp }` |
| `sensor:data` | サーバー→Unity | `{ playerId, accel, rotation, timestamp }` |
| `room:state` | サーバー→全員 | `{ players, status }` |

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
