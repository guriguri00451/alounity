# スマホコントローラーアプリ

Unityゲーム用のスマホコントローラーアプリケーション。スマホのセンサーデータ（加速度・ジャイロ）をSocket.IOでUnityに送信する。

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

## 技術スタック

- **フレームワーク**: Next.js 16 (App Router)
- **言語**: TypeScript
- **リアルタイム通信**: Socket.IO
- **センサーAPI**: DeviceMotionEvent / DeviceOrientationEvent
- **スタイリング**: Tailwind CSS

## 前提条件

- Node.js 18.x 以上
- npm 9.x 以上

## セットアップ

### 1. 依存パッケージのインストール

```bash
cd controller
npm install
```

### 2. 開発サーバーの起動

```bash
npm run dev
```

開発サーバーが起動したら、[http://localhost:3000](http://localhost:3000) をブラウザで開きます。

### 3. スマホからのアクセス（開発時）

PCとスマホを同じWi-Fiネットワークに接続し、スマホのブラウザで `http://<PCのIPアドレス>:3000` にアクセスします。

**注意**: センサーAPI（DeviceMotion/DeviceOrientation）を使用するにはHTTPSが必要です。本番環境またはHTTPS対応の開発環境では、mkcert等を使用してローカルHTTPS証明書を設定してください。

## プロジェクト構造

```
controller/
├── src/
│   ├── app/           # ページ（App Router）
│   ├── components/    # UIコンポーネント
│   ├── hooks/         # カスタムフック（センサーデータ取得等）
│   └── lib/           # ユーティリティ
├── server/            # カスタムサーバー（Socket.IO統合用）
└── public/            # 静的ファイル
```

## 利用可能なスクリプト

| コマンド | 説明 |
|---------|------|
| `npm run dev` | 開発サーバーを起動 |
| `npm run build` | プロダクションビルド |
| `npm run start` | プロダクションサーバーを起動 |
| `npm run lint` | ESLintでコードチェック |

## 開発上の注意

### AIエージェント設定ファイル

このプロジェクトでは、AIエージェント用の設定ファイルはgitignoreされています：

- `CLAUDE.md`
- `.cursor/`
- `.cursorrules`
- `.github/copilot-instructions.md`
- `.windsurfrules`
- `.opencode/`
- `.claude/`
- `opencode.json`

各AIエージェントの設定については、リポジトリルートの `docs/ai-agent-setup-guide.md` を参照してください。

## トラブルシューティング

### センサーデータが取得できない

- **iOS**: iOS 13以降では、HTTPS環境で `DeviceMotionEvent.requestPermission()` によるユーザー許可が必要です
- **Android**: Chrome等のモダンブラウザでサポートされています
- **開発環境**: localhostではHTTPでも動作しますが、センサーAPIはHTTPSが必須です

### Socket.IO接続エラー

- ファイアウォールでポート3000がブロックされていないか確認
- PCとスマホが同じネットワークに接続されているか確認

## 関連ドキュメント

- [実装計画](../docs/controller-implementation-plan.md)
- [開発ルール](../AGENTS.md)
