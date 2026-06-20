# FishRumble!!

スマホをコントローラーとして使用するマルチプレイヤーカヤックゲーム。

## 概要

**スマホカヤックゲーム** - 2チーム各3人で対戦するリアルタイムマルチプレイヤーゲーム（3人プレイのsingleモードあり）

### ゲームプレイ

1. QRコードでスマホをUnityクライアントに接続
2. 釣りで魚を釣る
3. 釣った魚を振り回して相手チームに投げる
4. 相手チームのHPを0にしたら勝ち

### チーム構成と役割

| 役割 | 操作 | 使用センサー |
|------|------|-------------|
| 右オール / 左オール | スマホを振ってカヤックを進める | DeviceMotionEvent（加速度） |
| 釣り | 方位磁針で狙い → 振りかぶってキャスト → 縦に振って引き上げ | DeviceOrientationEvent + DeviceMotionEvent |

### ゲームモード

- **single**: 1チーム3人
- **versus**: 2チーム3vs3

## アーキテクチャ

```
スマホブラウザ → Socket.IO → Next.jsサーバー → Socket.IO → Unity
```

- **通信方式**: Socket.IOによる双方向リアルタイム通信
- **リアルタイム通信**: Socket.IO 4.8.3
- **ルーム管理**: サーバーがインメモリで管理、ホスト切断時に自動削除

## 技術スタック

| 構成要素 | 技術 | バージョン |
|---------|------|-----------|
| ゲームエンジン | Unity 6 + URP | 6000.3.17f1 |
| レンダリング | Universal Render Pipeline | 17.3.0 |
| 入力管理 | Input System | 1.19.0 |
| 非同期処理 | UniTask | - |
| コントローラーアプリ | Next.js (App Router) | 16.2.9 |
| リアルタイム通信 | Socket.IO | 4.8.3 |
| Socket.IO クライアント (Unity) | SocketIOClient (doghappy) | 4.0.4 |
| センサーAPI | DeviceMotionEvent + DeviceOrientationEvent | - |
| QRコード生成 | UniQRCode | - |

## プロジェクト構成

```
alounity/
├── Assets/           # Unity プロジェクト
│   └── alounity/
│       ├── contributors/
│       │   ├── rita/   # rita の実装
│       │   └── soma/   # soma の実装
│       └── ...
├── controller/       # Next.js コントローラーアプリ
│   ├── src/
│   │   ├── app/      # Next.js App Router
│   │   ├── components/
│   │   └── hooks/
│   └── server/       # Socket.IO サーバー
└── docs/             # ドキュメント
```

## 開発コマンド

### Unity

Unity Editor (バージョン 6000.3.17f1) を使用

### コントローラー

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

## CI

`controller/` ディレクトリに変更がある場合、PR/Push時に自動で以下を実行：

1. **Biome check** - lint + format
2. **TypeScript** - 型チェック
3. **Vitest** - テスト実行
4. **Build** - ビルド確認

## ドキュメント

- [ゲームデザイン](docs/01_GameDesign.md)
- [技術標準](docs/02_TechStandard.md)
- [アーキテクチャ](docs/03_Architecture.md)
- [クラス図](docs/04_ClassDiagram.md)
- [コントローラー仕様書](docs/controller-implementation-plan.md)
- [ルーム管理](docs/room-management-plan.md)
- [QRコード実装](docs/qr-code-implementation.md)
