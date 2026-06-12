# 04_ClassDiagram (クラス図)

## 1. 全体クラス図

> `<<planned>>` は未実装（対応 Issue 参照）。矢印の意味は§3 凡例を参照。

```mermaid
classDiagram
    class BoatController {
        <<MonoBehaviour>>
        -Transform center
        -WheelCollider[] wheels
        -float power
        -InputActionProperty[] boat
        +Start()
        +FixedUpdate()
        +Disable()
    }

    class CameraController {
        <<MonoBehaviour>>
        +Transform target
        +Vector3 offset
        +float smoothTime
        -Vector3 velocity
        -Rigidbody targetRb
        +Start()
        +LateUpdate()
    }

    class FisherController {
        <<MonoBehaviour, planned>>
        +Start()
        +Update()
    }

    class HookMover {
        <<MonoBehaviour, planned>>
    }

    class GameManager {
        <<Singleton, planned>>
        -GameManager instance$
        +Instance GameManager$
    }

    CameraController --> BoatController : follows
    FisherController *-- HookMover : owns
    GameManager --> BoatController : manages
    GameManager --> FisherController : manages
```

---

## 2. 釣り機能 ステートマシン

> `FisherController` の状態遷移（[Issue #3](https://github.com/guriguri00451/alounity/issues/3), [#4](https://github.com/guriguri00451/alounity/issues/4) で実装予定）

```mermaid
stateDiagram-v2
    [*] --> Idle

    Idle --> Waiting : キャスト入力（スマホを振りかぶる）
    Waiting --> Catching : キャスト実行（スマホを前に投げる）
    Catching --> Swinging : 魚がヒット
    Catching --> Idle : タイムアウト・逃げられた
    Swinging --> Idle : 攻撃完了 / 魚を手放す
```

---

## 3. 凡例

| 記法 | 意味 |
|------|------|
| `<<planned>>` | 未実装クラス（対応 Issue を参照） |
| `<<Singleton>>` | インスタンスが1つのみ（GameManager） |
| `-->` | 依存（参照するが所有しない） |
| `*--` | コンポジション（ライフサイクルを共にする） |
| `o--` | 集約（独立して存在できる） |
| `-` prefix | private メンバ |
| `+` prefix | public メンバ |
| `$` suffix | static メンバ |
