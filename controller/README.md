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
- **Lint/Format**: Biome
- **テスト**: Vitest + React Testing Library

## 前提条件

- Node.js 18.x 以上
- npm 9.x 以上

## セットアップ

### 1. 依存パッケージのインストール

```bash
cd controller
npm install
```

### 2. 開発サーバーの起動（HTTP）

```bash
npm run dev
```

開発サーバーが起動したら、[http://localhost:3000](http://localhost:3000) をブラウザで開きます。

### 3. HTTPS環境のセットアップ（推奨）

スマホからセンサーデータ（DeviceMotion/DeviceOrientation）を取得するにはHTTPSが必要です。

#### 3-1. mkcertのインストール

```bash
# macOS
brew install mkcert
mkcert -install

# Linux
sudo apt install mkcert
mkcert -install

# Windows
choco install mkcert
mkcert -install
```

#### 3-2. 証明書の生成

```bash
npm run setup:https
```

このスクリプトは以下を実行します：
- ローカルCAのインストール
- `localhost` と `127.0.0.1` の証明書生成
- ローカルIPアドレスの証明書生成（スマホからのアクセス用）

#### 3-3. HTTPSサーバーの起動

```bash
npm run dev:https
```

HTTPSサーバーが起動したら、[https://localhost:3000](https://localhost:3000) をブラウザで開きます。

### 4. スマホからのアクセス

PCとスマホを同じWi-Fiネットワークに接続し、スマホのブラウザで `https://<PCのIPアドレス>:3000` にアクセスします。

**注意**: mkcertで生成した証明書は、PCのブラウザでは自動的に信頼されますが、スマホでは手動で信頼させる必要がある場合があります。

#### iOSの場合
1. `https://<PCのIPアドレス>:3000` にアクセス
2. 「詳細を表示」→「このウェブサイトを訪問」をタップ
3. 設定アプリ → 一般 → 情報 → 証明書信頼設定 → ルート証明書を信頼

#### Androidの場合
Chromeではmkcertの証明書が自動的に信頼される場合があります。信頼されない場合は、設定から証明書をインストールしてください。

## プロジェクト構造

```
controller/
├── src/
│   ├── app/           # ページ（App Router）
│   ├── components/    # UIコンポーネント
│   ├── hooks/         # カスタムフック（センサーデータ取得等）
│   └── lib/           # ユーティリティ
├── server/
│   └── index.ts       # カスタムサーバー（Next.js + Socket.IO統合）
├── scripts/
│   └── setup-https.sh # HTTPS証明書生成スクリプト
├── certs/             # HTTPS証明書（.gitignore対象）
└── public/            # 静的ファイル
```

## カスタムサーバーについて

Next.jsのデフォルトサーバーはWebSocket接続をサポートしていないため、Socket.IOを使用するためにカスタムサーバーを実装しています。

- **ファイル**: `server/index.ts`
- **起動方法**: `npm run dev`（カスタムサーバー経由でNext.jsを起動）
- **ポート**: 3000
- **Socket.IO**: WebSocket + Polling対応
- **HTTPS対応**: `npm run dev:https` でHTTPSサーバーを起動可能

### HTTPS対応

カスタムサーバーはHTTPSに対応しています。`HTTPS=true` 環境変数を設定することでHTTPSモードで起動します。

- **証明書**: `certs/` ディレクトリに配置
- **ファイル名**: `server-key.pem`, `server.pem`（すべてのドメインを1つの証明書に統合）

証明書は `npm run setup:https` で生成できます。

### Socket.IOイベント

| イベント | 方向 | 説明 |
|---------|------|------|
| `controller:connect` | スマホ → サーバー | コントローラー接続 |
| `controller:sensor` | スマホ → サーバー | センサーデータ送信 |
| `sensor:data` | サーバー → Unity | センサーデータ転送 |
| `unity:connect` | Unity → サーバー | Unity接続 |

### ルーム機能

同じルーム内のクライアント間でセンサーデータを転送します。`controller:connect`または`unity:connect`イベントで`roomId`を指定することで、同じルームに参加できます。

## 利用可能なスクリプト

| コマンド | 説明 |
|---------|------|
| `npm run dev` | 開発サーバーを起動（HTTP、カスタムサーバー経由） |
| `npm run dev:https` | 開発サーバーを起動（HTTPS、証明書が必要） |
| `npm run dev:next` | Next.js開発サーバーを直接起動（Socket.IOなし） |
| `npm run setup:https` | mkcertでHTTPS証明書を生成 |
| `npm run build` | プロダクションビルド |
| `npm run start` | プロダクションサーバーを起動（カスタムサーバー経由） |
| `npm run check` | Biome (lint + format) チェック |
| `npm run check:fix` | Biome 自動修正 |
| `npm run typecheck` | TypeScript型チェック |
| `npm run test` | Vitestテスト実行（watchモード） |
| `npm run test:run` | Vitestテスト実行（1回のみ） |
| `npm run test:coverage` | テストカバレッジ取得 |

## CI（GitHub Actions）

`controller/` ディレクトリに変更がある場合、PR/Push時に自動で以下を実行：

1. **Biome check** - lint + format
2. **TypeScript** - 型チェック
3. **Vitest** - テスト実行
4. **Build** - ビルド確認

ワークフローファイル: `.github/workflows/ci-controller.yml`

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
- **解決策**: `npm run setup:https` で証明書を生成し、`npm run dev:https` でHTTPSサーバーを起動してください

### HTTPS証明書のエラー

- **証明書が見つからない**: `npm run setup:https` を実行して証明書を生成してください
- **mkcertがインストールされていない**: 前提条件のセクションを参照してインストールしてください
- **スマホで証明書が信頼されない**: セットアップのセクションの「スマホからのアクセス」を参照してください

### Socket.IO接続エラー

- ファイアウォールでポート3000がブロックされていないか確認
- PCとスマホが同じネットワークに接続されているか確認
- **AP分離が有効なWiFiネットワーク**: 一部のWiFiネットワーク（特にゲストネットワーク）は、クライアント間の通信をブロックする「AP分離」が有効になっています。この場合、スマホからPCにアクセスできません。AP分離が無効なネットワークに接続してください

## 関連ドキュメント

- [実装計画](../docs/controller-implementation-plan.md)
- [開発ルール](../AGENTS.md)
