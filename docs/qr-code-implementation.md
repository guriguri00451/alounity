# QRコード生成機能 実装ドキュメント

## 概要

ルームIDをQRコードとして生成し、スマホからスキャンしてルームに参加できるようにする機能。

## ファイル構成

```
Assets/alounity/contributors/rita/Scripts/
├── SocketIOManager.cs          # Socket.IO接続管理（QRコード関連のイベント発火を含む）
└── QR/
    ├── QREncoder.cs            # QRコードエンコーダーライブラリ（外部ライブラリ）
    ├── QRCodeGenerator.cs      # QRコード生成ユーティリティ
    └── QRCodeDisplay.cs        # QRコード表示コンポーネント（MonoBehaviour）
```

## 各ファイルの役割

### `QREncoder.cs`（外部ライブラリ）

- **出典**: [UniQRCode](https://github.com/wallstudio/UniQRCode) (MIT License)
- **名前空間**: `QRCodeEncoderLibrary`
- **役割**: 文字列からQRコードの2次元配列（`bool[,]`）を生成する
- **修正点**: `using System;` を追加（Unity用の修正）
- **主なメソッド**:
  - `Encode(string text)` → `bool[,]` - 文字列をQRコードマトリックスに変換

### `QRCodeGenerator.cs`

- **名前空間**: `Alounity.QR`
- **役割**: `QREncoder` を使ってQRコードをUnityの `Texture2D` として生成する
- **主なメソッド**:
  - `Generate(string text, int pixelsPerModule = 10)` → `Texture2D`
    - 引数 `text`: QRコードにエンコードする文字列
    - 引数 `pixelsPerModule`: QRコードの各モジュールを何ピクセルで描画するか（デフォルト10）
    - 戻り値: 生成された `Texture2D`（黒と白のみ）
- **処理内容**:
  1. `QREncoder.Encode()` でQRコードマトリックスを取得
  2. マトリックスサイズ × `pixelsPerModule` のテクスチャを作成
  3. 各モジュールを黒/白で塗りつぶし
  4. `FilterMode.Point` を設定してピクセルがぼやけないようにする

### `QRCodeDisplay.cs`

- **名前空間**: `Alounity.QR`
- **役割**: MonoBehaviourとしてシーンに配置し、QRコードをUIに表示する
- **Inspectorで設定するフィールド**:
  - `qrImage` (RawImage): QRコードを表示するUI要素
  - `roomIdText` (TextMeshProUGUI): ルームIDテキストを表示するUI要素
  - `rootPanel` (GameObject): QRコード表示パネルのルート（表示/非表示の切り替え用）
- **処理フロー**:
  1. `Start()` で `SocketIOManager.Instance` を取得し、`OnRoomCreated` イベントを購読
  2. 既にルーム作成済みなら即座に `ShowQRCode()` を実行
  3. ルーム作成時に `ShowQRCode()` が呼ばれる
  4. `SocketIOManager.ServerUri` と `RoomId` からURLを生成
  5. `QRCodeGenerator.Generate()` でテクスチャを生成
  6. `RawImage.texture` に設定して表示
- **公開メソッド**:
  - `Hide()`: QRコードパネルを非表示にする

### `SocketIOManager.cs`（QRコード関連の修正）

- **追加プロパティ**:
  - `ServerUri` (Uri): サーバーのURI（QRコードのURL生成に使用）
- **追加イベント**:
  - `OnRoomCreated`: ルーム作成完了時に発火（`QRCodeDisplay` が購読）
- **スレッド切り替え**:
  - Socket.IOのコールバックはバックグラウンドスレッドで実行される
  - UnityのAPI（`Texture2D`生成、`Debug.Log`など）はメインスレッドが必要
  - `UniTask.SwitchToMainThread()` でメインスレッドに切り替えてからコールバックを実行

## 処理フロー

```
┌─────────────────────────────────────────────────────────────────┐
│                         Unity起動                                │
└─────────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────────┐
│  SocketIOManager.Connect()                                       │
│  - Socket.IOサーバーに接続                                        │
│  - host:create イベントを送信                                     │
└─────────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────────┐
│  Socket.IOサーバー                                                │
│  - ルームIDを生成（6文字英数字）                                   │
│  - host:create_ack を返信                                        │
└─────────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────────┐
│  SocketIOManager.OnHostCreateAck()                               │
│  - RoomId を保存                                                 │
│  - UniTask.SwitchToMainThread() でメインスレッドに切り替え         │
│  - OnRoomCreated イベントを発火                                   │
└─────────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────────┐
│  QRCodeDisplay.ShowQRCode()                                      │
│  - URL生成: http://192.168.x.x:3000/room/ABC123                  │
│  - QRCodeGenerator.Generate() でテクスチャ生成                    │
│  - RawImage に設定                                               │
│  - ルームIDテキストを更新                                         │
│  - rootPanel を表示                                              │
└─────────────────────────────────────────────────────────────────┘
```

## シーン設定手順

1. **Canvasを作成**（Screen Space - Overlay）

2. **QRコード表示用のUIを配置**:
   ```
   Canvas
   └── QRPanel (GameObject) ← rootPanel に設定
       ├── QRImage (RawImage) ← qrImage に設定
       │   - 幅: 300, 高さ: 300
       │   - Color: 白
       └── RoomIdText (TextMeshProUGUI) ← roomIdText に設定
           - 文字サイズ: 48
           - 配置: 中央
   ```

3. **QRCodeDisplayコンポーネントをアタッチ**:
   - QRPanelオブジェクトに `QRCodeDisplay` を追加
   - Inspectorで各参照を設定

4. **初期状態**:
   - QRPanelは非アクティブにしておく（`SetActive(false)`）
   - ルーム作成時に自動でアクティブになる

## 生成されるURLの形式

```
http://{host}:{port}/room/{roomId}
```

例: `http://192.168.1.100:3000/room/YMA3FC`

- `host`: `SocketIOManager.serverUrl` から取得
- `port`: `SocketIOManager.serverUrl` から取得
- `roomId`: サーバーが生成した6文字の英数字（I, O, 0, 1を除く）

## 依存関係

| ライブラリ | バージョン | 用途 |
|-----------|-----------|------|
| UniQRCode | - | QRコードエンコード |
| UniTask | 2.x | メインスレッド切り替え |
| TextMeshPro | - | ルームIDテキスト表示 |
