# 01_GameDesign (企画・ゲームルール)

## 1. ゲームの全体像
* スマホをコントローラーにして、**2チーム各3人**で操作するカヤックゲームです。
* スマホでQRコードを読み取ってUnityクライアントに接続します。
* 釣ったアイテム（魚など）を振り回して、相手チームを攻撃しながら戦います。

## 2. チーム構成
* **チーム数:** 2チーム
* **1チームの人数:** 3人（オール左 / オール右 / 釣り）

## 3. 役割分担とスマホ側の操作
3人で「右オール」「左オール」「釣り」の役割を分担します。

* **右オール / 左オール担当:**
    * スマホを振ることでカヤックを進めます（加速度センサー: DeviceMotionEvent を使用）。
* **釣り担当:**
    * 方位磁針のように方角を取得して狙いを定めます（DeviceOrientationEvent を使用）。
    * スマホを振りかぶって前に投げる動作でキャスト（釣り）を行います。
    * スマホを縦に振って引き上げます。

## 4. ゲーム内の挙動・制限ルール
* **カヤックの移動:** カヤックの進行方向はオールでしか操作できません。
* **釣り人の向き:** 釣り人は常に船の前方を向いており、釣り竿は左右斜め45度にしか回転できない仕様です。
* **ダメージシステム:**
    * 攻撃が命中するとヒットエフェクト（パーティクル・SE）が発生します。
    * 人モデルは落ちません。HPが0になるまで船は壊れません。
* **勝敗条件:** 相手チームのHPを0にしたほうが勝ちです。

## 5. アイテム（魚）の仕様
* 釣ったアイテム（魚）は原則として強制使用になります。
* ただし、ガチャのようなランダム性を楽しむため、任意で捨てられる仕組みも実装します。
* 魚の種類はランダムまたは位置に応じて変化する予定です。

| 魚の種類 | 効果 |
|---------|------|
| 通常の魚 | 相手に投げてダメージを与える |
| 速度バフ系の魚 | 命中時に相手カヤックを急加速させて弾き飛ばす（マリオカートのワンワン系） |
| 大型の魚 | 当たり判定が大きく、ダメージが高い |

## 6. ビジュアル・演出
* 人モデルにはラグドールを**常時適用**します。
* 普段の動き（オールを漕ぐ・釣りをする）自体が『Totally Accurate Battle Simulator』のようなぷらんぷらんとした物理挙動です。
* ダメージ時はヒットエフェクトのみ発生します（人が倒れたり落ちたりするのはHP0の死亡時のみ）。

## 7. マイルストーンとタスク分け

### 【MVP】(最小限のプロダクト)

#### 釣り攻撃（担当: そーま）
* 釣り針に魚アイテムがヒットしたときの判定 ([#3](https://github.com/guriguri00451/alounity/issues/3))
* Swinging状態での振り回し攻撃判定 ([#4](https://github.com/guriguri00451/alounity/issues/4))
* 魚アイテムのプレハブ作成 ([#5](https://github.com/guriguri00451/alounity/issues/5))
* MakeFisher ブランチを dev にマージ ([#6](https://github.com/guriguri00451/alounity/issues/6))

#### スマホ操作（担当: リタ）
* スマホ→Unity 間の通信方式を技術選定・設計 ([#7](https://github.com/guriguri00451/alounity/issues/7))
* スマホ側で加速度センサーを取得 ([#8](https://github.com/guriguri00451/alounity/issues/8))
* スマホ側で方位磁針（方角）を取得 ([#9](https://github.com/guriguri00451/alounity/issues/9))
* Unity 側でスマホ入力を受信 ([#10](https://github.com/guriguri00451/alounity/issues/10))
* 釣りアクションとオール操作をスマホ入力にマッピング ([#11](https://github.com/guriguri00451/alounity/issues/11))

#### カヤック移動（担当: Chebuo）
* kayakMove ブランチを dev にマージ ([#12](https://github.com/guriguri00451/alounity/issues/12))

#### ステージ（担当: 未定）
* 簡易ステージ（Plane + 障害物）の作成 ([#13](https://github.com/guriguri00451/alounity/issues/13))

---

### 【ハッカソン目標】

#### QRマッチング基盤
* Unity 側でQRコードを生成・表示 ([#14](https://github.com/guriguri00451/alounity/issues/14))
* スマホアプリ側でQRコードを読み取り ([#15](https://github.com/guriguri00451/alounity/issues/15))
* QRスキャン後にスマホ→Unity へ接続・入力送信を開始 ([#16](https://github.com/guriguri00451/alounity/issues/16))

#### マルチプレイ・ゲームループ
* 2チームのプレイヤー管理・ロール割り当て ([#17](https://github.com/guriguri00451/alounity/issues/17))
* ゲームUI（HP・スコア・タイマー） ([#18](https://github.com/guriguri00451/alounity/issues/18))
* 2チームプレイのゲームループ（勝敗判定・終了処理） ([#19](https://github.com/guriguri00451/alounity/issues/19))
* ゲームリセット・再スタート機能 ([#20](https://github.com/guriguri00451/alounity/issues/20))

#### ビジュアル・演出
* カヤック内に人モデルを配置 ([#21](https://github.com/guriguri00451/alounity/issues/21))
* ラグドールを使った人モデルの動き（漕ぎ・釣り） ([#22](https://github.com/guriguri00451/alounity/issues/22))
* ダメージを受けたときのヒットエフェクト ([#23](https://github.com/guriguri00451/alounity/issues/23))
* 魚アイテムの種類を追加（速度バフ系など） ([#24](https://github.com/guriguri00451/alounity/issues/24))
* 海のテクスチャ/Shader ([#25](https://github.com/guriguri00451/alounity/issues/25))

---

### 【ベスト目標】
* 4チームプレイへの拡張。
* しっかりとしたステージの作成。