# 04_ClassDiagram (クラス図)

## 1. 全体クラス図

> `<<planned>>` は未実装。それ以外は実装済み。

```mermaid
classDiagram
    class FisherControler {
        <<MonoBehaviour>>
        -FisherState currentState
        -FisherSpringJointConfig springJointConfig
        -ManageState(FisherState)
        -Cast(CallbackContext)
        -Reel(CallbackContext)
    }

    class HookMover {
        <<MonoBehaviour>>
        -float buoyancyForce
        -ApplyBuoyancy()
    }

    class FisherSpringJointConfig {
        <<ScriptableObject>>
        +Get(FisherState) SpringJointSettings
    }

    class SpringJointSettings {
        <<Serializable>>
        +ApplyTo(SpringJoint)
    }

    class FisherState {
        <<enumeration>>
        Idle
        Waiting
        Catching
        Swinging
    }

    class BoatController {
        <<MonoBehaviour>>
        -float power
        -InputActionProperty[] boat
        +Disable()
    }

    class CameraController {
        <<MonoBehaviour>>
        +Transform target
        +float smoothTime
    }

    class PlayerManager {
        <<MonoBehaviour, planned>>
        +BoatController boat
        +FisherControler fisher
        +CameraController camera
        +int teamId
    }

    class GameManager {
        <<Singleton, planned>>
        +List~PlayerManager~ players
    }

    FisherControler --> HookMover : controls
    FisherControler --> FisherSpringJointConfig : uses
    FisherControler --> FisherState : manages state
    FisherSpringJointConfig *-- SpringJointSettings : owns
    CameraController --> BoatController : follows
    PlayerManager *-- BoatController : owns
    PlayerManager *-- FisherControler : owns
    PlayerManager *-- CameraController : owns
    GameManager "1" *-- "6" PlayerManager : manages
```

---

## 2. 釣り機能 ステートマシン

> `FisherControler.ManageState()` で遷移。`Cast()` / `Reel()` が入力トリガー。

```mermaid
stateDiagram-v2
    [*] --> Idle

    Idle --> Catching : Cast入力（hookにAddForce）
    Catching --> Swinging : Reel入力 × requiredReelShakeCount 回
    Swinging --> Idle : 攻撃完了
    Catching --> Idle : タイムアウト（未実装）
```

---

## 3. 凡例

| 記法 | 意味 |
|------|------|
| `<<planned>>` | 未実装クラス（対応 Issue を参照） |
| `<<Singleton>>` | インスタンスが1つのみ |
| `<<enumeration>>` | enum 型 |
| `<<ScriptableObject>>` | Unity ScriptableObject（アセットとして保存） |
| `<<Serializable>>` | Inspector/JSON シリアライズ可能な純粋クラス |
| `-->` | 依存（参照するが所有しない） |
| `*--` | コンポジション（ライフサイクルを共にする） |
| `-` prefix | private メンバ |
| `+` prefix | public メンバ |
| `$` suffix | static メンバ |
