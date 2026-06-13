# 02_TechStandard (技術スタック・命名規則)

## 1. 使用技術

| カテゴリ | ライブラリ / ツール | バージョン |
|---------|-----------------|-----------|
| ゲームエンジン | Unity 6 | 6000.3.17f1 |
| レンダリング | Universal Render Pipeline (URP) | 17.3.0 |
| 入力管理 | Input System | 1.19.0 |
| 非同期処理 | UniTask | - |
| Reactive Extensions | R3 | - |
| スマホ通信 | 未選定（[Issue #7](https://github.com/guriguri00451/alounity/issues/7) で決定予定） | - |

> スマホ通信の候補: WebSocket / UDP。Issue #7 で方式が確定し次第更新します。

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
* **通信方向:** スマホ → Unity（PC）の **一方向** 送信のみ
* **送信データ:** 加速度センサー（DeviceMotionEvent）・方位磁針（DeviceOrientationEvent）
* **通信方式:** [Issue #7](https://github.com/guriguri00451/alounity/issues/7) で選定予定
    * 候補: WebSocket、UDP（低遅延）
    * スマホ側はブラウザベース（追加アプリ不要）を優先
