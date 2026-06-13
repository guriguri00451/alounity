# Unityゲーム開発ルール

## バージョン

- Unity 6 (6000.3.17f1) を使用
- Universal Render Pipeline (URP) 適用済み

## ディレクトリ構成

```
Assets/
├── alounity/              # 共有フォルダ
│   ├── Contributors/      # 個人フォルダ
│   │   ├── [ユーザー名]/
│   │   │   └── Scenes/    # 個人のシーンファイル
│   │   └── ...
│   └── ...
├── Plugins/               # プラグイン（CW LeanTouch等）
└── Resources/             # リソースファイル
```

### 作業フォルダについて

- **共有フォルダ**: `Assets/alounity/` - 全員がアクセス可能
- **個人フォルダ**: `Assets/alounity/Contributors/[ユーザー名]/` - 各メンバーの作業場所
- Sceneは個人フォルダ内のSceneを使用すること

## Socket.IO連携

- Unity側はSocket.IOクライアントとして接続
- ライブラリ候補: `SocketIoClientDotNet` または `NativeWebSocket`
- サーバーから受信したセンサーデータをゲームロジックで使用
- 通信方向: スマホ→Unityの**一方向**のみ

## 主要クラス一覧

| クラス名 | パス | 役割 |
|---------|------|------|
| `FisherController` | `contributors/soma/Scripts/` | 釣り人の状態管理・入力処理（Idle → Waiting → Catching → Swinging） |
| `HookMover` | `contributors/soma/Scripts/` | 釣り針の物理演算・浮力・水中抵抗制御 |
| `BoatController` | `contributors/chebuo/Scripts/` | カヤック移動（WheelCollider ベース） |
| `CameraController` | `contributors/chebuo/Scripts/` | ボート追従カメラ（SmoothDamp） |
| `GameManager` | `Scripts/`（新規予定） | Singleton。ゲーム状態管理・チーム管理 |

## スマホ入力のマッピング

| 役割 | 受信データ | マッピング先 |
|------|-----------|-------------|
| 右オール | `accel` | `BoatController` の右側推進力 |
| 左オール | `accel` | `BoatController` の左側推進力 |
| 釣り | `orientation` + `accel` + `rotation` | `FisherController` のステート遷移 |

## コーディング規約

### 命名規則

| 対象 | 規則 | 例 |
|------|------|-----|
| クラス名、メソッド名、プロパティ名 | PascalCase | `PlayerController`, `GetHealth()` |
| メンバ変数、ローカル変数、パラメータ | camelCase | `playerHealth`, `moveSpeed` |
| インターフェース | I + PascalCase | `IMyInterface` |

### public変数

- 原則として `public` 変数は使わない
- 値を公開したい場合は **プロパティ** を使用する

```csharp
// Bad
public int health;

// Good
public int Health { get; private set; }
```

### Singleton

- `GameManager` はSingletonパターンで実装する
- "Manager"という名前は、他スクリプトを複数管理していない状況では極力避ける

### その他

- MonoBehaviour のライフサイクルを意識した設計
- 日本語でコメント記述

## 注意事項

- Unity Editor (6000.3.17f1) で動作確認
- URP設定は `UniversalRenderPipelineGlobalSettings.asset` で管理
- Input System は `InputSystem_Actions.inputactions` で定義
