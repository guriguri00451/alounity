# Next.jsコントローラーアプリ開発ルール

## 基本構成

- `controller/` ディレクトリ配下に配置
- Next.js 16 + App Router（Pages Router禁止）
- React 19 + TypeScript strict mode

## 開発ツール

| ツール | 用途 |
|--------|------|
| Biome | Lint + Format |
| Vitest | テスト |
| React Testing Library | コンポーネントテスト |

## カスタムサーバー

Socket.IO統合のため `server/index.ts` でカスタムサーバーを実装する。

- Next.jsのデフォルトサーバーはWebSocket非対応
- `next` + `http.Server` + `socket.io` を組み合わせる
- 起動方法: `npm run dev`（tsx経由でTypeScriptを直接実行）
- Socket.IOイベント:
  - `controller:connect`: コントローラー接続
  - `controller:sensor`: センサーデータ受信
  - `sensor:data`: Unityへのセンサーデータ転送
  - `unity:connect`: Unity接続
- ルーム機能: `roomId`で同じルーム内のクライアント间でデータを転送

### HTTPS対応

- `HTTPS=true`環境変数でHTTPSモードで起動
- `npm run setup:https` でmkcertを使用した証明書生成
- `npm run dev:https` でHTTPSサーバーを起動
- 証明書は`certs/`ディレクトリに保存（.gitignore対象）
- localhost、127.0.0.1、ローカルIPアドレス用の証明書を生成

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
├── lib/             # ユーティリティ
└── __tests__/       # テストファイル
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
