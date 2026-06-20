# 02_TechStandard (技術スタック・命名規則)

## 1. 使用技術

### Unity（ゲームクライアント）

| カテゴリ | ライブラリ / ツール | バージョン |
|---------|-----------------|-----------|
| ゲームエンジン | Unity 6 | 6000.3.17f1 |
| レンダリング | Universal Render Pipeline (URP) | 17.3.0 |
| 入力管理 | Input System | 1.19.0 |
| 非同期処理 | UniTask | - |
| Socket.IO クライアント | SocketIOClient (doghappy) | 4.0.4 |
| QRコード生成 | UniQRCode (MIT License) | - |

### コントローラーアプリ（スマホブラウザ）

| カテゴリ | ライブラリ / ツール | バージョン |
|---------|-----------------|-----------|
| フレームワーク | Next.js (App Router) | 16.2.9 |
| UIライブラリ | React | 19.2.4 |
| スタイリング | Tailwind CSS | 4.x |
| 型チェック | TypeScript | 5.x |
| リアルタイム通信 | Socket.IO Client | 4.8.3 |
| センサーAPI | DeviceMotionEvent / DeviceOrientationEvent | - |

### サーバー（リアルタイム通信中継）

| カテゴリ | ライブラリ / ツール | バージョン |
|---------|-----------------|-----------|
| ランタイム | Node.js (tsx) | - |
| Webフレームワーク | Next.js (Custom Server) | 16.2.9 |
| リアルタイム通信 | Socket.IO | 4.8.3 |
| HTTPS（開発用） | mkcert | - |

### 開発ツール・CI

| カテゴリ | ライブラリ / ツール | バージョン |
|---------|-----------------|-----------|
| Linter / Formatter | Biome | 2.5.0 |
| テスト | Vitest | 4.1.8 |
| テストユーティリティ | Testing Library (React) | 16.3.2 |
| CI | GitHub Actions | - |

## 2. 命名規則
* **PascalCase (単語ごとに頭文字を大文字):** アセット名、クラス名、メソッド名、プロパティ名。
* **camelCase (パスカルケースの頭文字を小文字):** メンバ変数、ローカル変数、パラメータ（引数）。
* **インターフェース:** `IMyInterface` のように頭文字に `I` をつけ、パスカルケースで続けます。

## 3. 実装ルール
* **変数の公開:** 原則として `public` な変数は使わず、値を公開したい場合はプロパティを使用します。
* **Singletonの利用:** `GameManager` は Singleton で作成します。
* **Managerの命名:** `Manager` という名前は、他スクリプトを複数管理していない状況では極力避けます。
* **Sceneの管理:** 開発中の Scene は、各自の個人フォルダにある Scene を使用します。

## 4. フォルダ・Scene 管理

### フォルダ構成

```
Assets/alounity/
├── Scripts/            共通スクリプト（GameManager など）
├── Prefabs/            共通プレハブ
├── Materials/
├── Models/
├── Sprites/
├── Textures/
└── contributors/
    ├── soma/           そーま個人フォルダ（釣り機能）
    ├── rita/           リタ個人フォルダ（スマホ入力）
    └── chebuo/         Chebuo個人フォルダ（カヤック移動）
```

各個人フォルダの下に `Scripts/`, `Prefabs/`, `Scenes/` などを作成して作業します。

### Scene 構成

| Scene 名 | パス | 用途 |
|---------|------|------|
| SampleScene | `Assets/Scenes/` | 統合テスト用メインシーン |
| SomaScene | `Assets/alounity/contributors/soma/Scenes/` | 釣り機能テスト用 |

## 5. スマホ通信方針
* **通信方式:** Socket.IO (WebSocket + polling フォールバック)
* **通信方向:** スマホとUnity（PC）間のSocket.IOによる双方向リアルタイム通信
* **送信データ:** 加速度センサー（DeviceMotionEvent）・方位磁針（DeviceOrientationEvent）
* **スロットリング:** 30fps（33ms間隔）でネットワーク負荷を抑制
* **スマホ側:** ブラウザベース（追加アプリ不要）
* **HTTPS:** センサーAPIはSecure Contextを要求 → mkcertでローカル証明書を生成
* **詳細設計:** `docs/03_Architecture.md` のイベント設計を参照
