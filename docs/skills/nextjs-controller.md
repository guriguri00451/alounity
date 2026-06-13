# Next.jsコントローラーアプリ開発ルール

## 基本構成

- `controller/` ディレクトリ配下に配置
- Next.js 15 + App Router（Pages Router禁止）
- React 19 + TypeScript strict mode

## カスタムサーバー

Socket.IO統合のため `server/index.ts` でカスタムサーバーを実装する。

- Next.jsのデフォルトサーバーはWebSocket非対応
- `next` + `http.Server` + `socket.io` を組み合わせる

## コンポーネント規約

- センサーAPI使用コンポーネントには `"use client"` 必須
- サーバーコンポーネントとクライアントコンポーネントを明確に分離
- Tailwind CSSでスタイリング

## ファイル配置

```
src/
├── app/              # ページ（App Router）
├── components/       # UIコンポーネント
├── hooks/           # カスタムフック
└── lib/             # ユーティリティ
```

## 注意事項

- 日本語でコメント記述
- センサーAPIはHTTPS必須（Secure Context）
- iOS 13+では `DeviceMotionEvent.requestPermission()` が必要

## 役割別UI設計

### 役割選択画面

接続後に役割（右オール/左オール/釣り）を選択する画面を表示。

### 役割別UI

| 役割 | 表示内容 |
|------|---------|
| 右オール/左オール | センサー値のリアルタイム表示、振る動作のフィードバック |
| 釣り | 方位角の表示（方位磁針風）、キャスト/引き上げのガイド |
