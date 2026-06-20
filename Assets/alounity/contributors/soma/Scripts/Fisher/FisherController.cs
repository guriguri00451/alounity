using R3;
using UnityEngine;
using UnityEngine.InputSystem;
using FishRumble;
using Cysharp.Threading.Tasks;

/// <summary>
/// 釣り人のステートマシンを管理する。
/// Idle → Waiting → Catching → Swinging → Idle のサイクルを制御する。
/// </summary>
public class FisherController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform rodTransform;
    [SerializeField] private Transform hookTransform;
    private Rigidbody hookRigidbody;
    [SerializeField] private SpringJoint lineSpringJoint;
    [SerializeField] private FisherSpringJointConfig springJointConfig;
    [Header("ControlValues")]
    [Range(-1, 1)]
    [SerializeField] private float horizontalInput;

    [Header("Settings")]
    [SerializeField] private FishRumbleInput input;
    [SerializeField] private bool isDebugMode = false;

    [Header("Parameters")]
    [SerializeField] private float rodRotationSpeed = 5f;
    [SerializeField] private float rodMinRotation = -45f;
    [SerializeField] private float rodMaxRotation = 45f;
    [SerializeField] private FisherState currentState = FisherState.Idle;
    [SerializeField] private float castPower = 10f;
    [SerializeField] private int CatchRequiredShakeCount = 12;
    [SerializeField] private int DropRequiredShakeCount = 5;
    [SerializeField] private float minLineLength = 0.5f;


    private int shakeCount = 0;
    private Fish caughtFish;
    private Hook _hook;
    private Quaternion initialRodRotation;

    void Start()
    {
        SubscribeInput();
        GetHookReferences();
        initialRodRotation = rodTransform.localRotation;
    }

    void GetHookReferences()
    {
        hookRigidbody = hookTransform.GetComponent<Rigidbody>();
        _hook = hookTransform.GetComponent<Hook>();
        _hook.onFishSpawned += CatchFish;
    }

    void Update()
    {
        if (isDebugMode)
        {
            if (Keyboard.current.digit1Key.wasPressedThisFrame) ManageState(FisherState.Idle);
            if (Keyboard.current.digit2Key.wasPressedThisFrame) ManageState(FisherState.Waiting);
            if (Keyboard.current.digit3Key.wasPressedThisFrame) ManageState(FisherState.Catching);
            if (Keyboard.current.digit4Key.wasPressedThisFrame) ManageState(FisherState.Swinging);
        }
        RotateRod();
    }

    void SubscribeInput()
    {
        input = new FishRumbleInput();

        input.Player.Cast.performed += Cast;
        input.Player.Shake.performed += Shake;

        input.Enable();
    }

    /// <summary>
    /// 竿の回転を管理する。horizontalInput の値に応じて rodMinRotation〜rodMaxRotation の範囲で回転させる。
    /// </summary>
    void RotateRod()
    {
        if (!isDebugMode)
        {
            horizontalInput = input.Player.Rotate.ReadValue<float>();
        }

        float targetAngle = Mathf.Lerp(rodMinRotation, rodMaxRotation, (horizontalInput + 1f) / 2f) + initialRodRotation.eulerAngles.y;
        float currentAngle = rodTransform.localEulerAngles.y;

        float smoothedAngle = Mathf.LerpAngle(currentAngle, targetAngle, Time.deltaTime * rodRotationSpeed);
        rodTransform.localEulerAngles = new Vector3(
            rodTransform.localEulerAngles.x,
            smoothedAngle,
            rodTransform.localEulerAngles.z
        );
    }

    /// <summary>
    /// FisherState を遷移し、状態に応じた SpringJoint パラメータを適用する。
    /// </summary>
    void ManageState(FisherState newState)
    {
        Debug.Log($"State changed: {currentState} -> {newState}");
        currentState = newState;

        springJointConfig.Get(currentState).ApplyTo(lineSpringJoint);

        switch (newState)
        {
            case FisherState.Idle:
                
                break;
            case FisherState.Waiting:
                _hook.Release();
                break;
            case FisherState.Catching:
                if(caughtFish == null) break;
                _hook.CatchFish(caughtFish.transform);
                break;
            case FisherState.Swinging:
                _hook.Release();
                caughtFish.Initialize();
                caughtFish.SetAttackActive(true);
                break;
        }
    }

    /// <summary>
    /// Idle 状態のときにキャストする。針を切り離して前方に投げ、Waiting へ遷移する。
    /// </summary>
    void Cast(InputAction.CallbackContext context)
    {
        if (currentState != FisherState.Idle) return;
        ManageState(FisherState.Waiting);
        hookRigidbody.AddForce(this.transform.forward * castPower, ForceMode.Impulse);
    }

    /// <summary>
    /// スマホを振る動作に対応する入力コールバック。
    /// Catching 中は糸を巻き上げ、Swinging 中は振り回し攻撃を行う。
    /// </summary>
    void Shake(InputAction.CallbackContext input)
    {
        if (currentState == FisherState.Catching)
            PullingLine();
        else if (currentState == FisherState.Waiting ||currentState == FisherState.Swinging)
            SwingAttack(input);
    }

    /// <summary>
    /// 糸を巻く。規定回数に達したら Swinging へ遷移する。
    /// </summary>
    void PullingLine()
    {
        shakeCount++;
        if (shakeCount >= CatchRequiredShakeCount)
        {
            shakeCount = 0;
            ManageState(FisherState.Swinging);
        }
    }

    /// <summary>
    /// 振り回し攻撃。Shakeするたびに糸を縮め、minLineLengthまで巻き取ったらIdleに戻る。
    /// </summary>
    void SwingAttack(InputAction.CallbackContext _input)
    {
        ShortenLine();
        if (lineSpringJoint.maxDistance <= minLineLength)
            ManageState(FisherState.Idle);
    }

    void ShortenLine()
    {
        lineSpringJoint.maxDistance = Mathf.Max(minLineLength, lineSpringJoint.maxDistance - 0.5f);
    }

    void CatchFish(Fish fish)
    {
        if (currentState != FisherState.Waiting) return;

        ManageState(FisherState.Catching);
        _hook.CatchFish(fish.transform);
        caughtFish = fish.GetComponent<Fish>();
        caughtFish.onDepleted += DropFish;
    }

    void DropFish()
    {
        caughtFish.onDepleted -= DropFish;
        if(caughtFish != null) 
            caughtFish.SetAttackActive(false);
        caughtFish = null;
        _hook.ReleaseFish();

    }
}
