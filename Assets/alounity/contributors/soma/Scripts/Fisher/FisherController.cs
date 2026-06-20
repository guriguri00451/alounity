using R3;
using UnityEngine;
using UnityEngine.InputSystem;
using FishRumble;
using Cysharp.Threading.Tasks;
using System.Threading;
using System.Threading.Tasks;

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
    [SerializeField] private GameObject fishPrefab;
    [SerializeField] private FishRumbleInput input;
    [SerializeField] private bool isDebugMode = false;

    [Header("FishRodParams")]
    [SerializeField] private float rodRotationSpeed = 5f;
    [SerializeField] private float rodMinRotation = -45f;
    [SerializeField] private float rodMaxRotation = 45f;
    [SerializeField] private FisherState currentState = FisherState.Idle;
    [SerializeField] private float castPower = 10f;
    [SerializeField] private int CatchRequiredShakeCount = 12;
    [Header("AttackParams")]
    [SerializeField] private float fishAcceraratePower;
    [SerializeField] private float smallAttackPowerThresholdValue;
    [SerializeField] private float middleAttackPowerThresholdValue;
    [SerializeField] private float bigAttackPowerThresholdValue;
    [SerializeField] private int swingFinishTimeMs = 1200;
    [SerializeField] private float swingCoolTime = 3f;


    private int shakeCount = 0;
    private Fish caughtFish;
    private Hook _hook;
    private Quaternion initialRodRotation;
    private float swingCoolTimer;
    private bool isAbleCatch;
    private bool isSwinging;

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
        _hook.onAbleCatch += AbleCatch;
    }

    void Update()
    {
        if (isDebugMode)
        {
            if (Keyboard.current.digit1Key.wasPressedThisFrame) ManageState(FisherState.Idle);
            if (Keyboard.current.digit2Key.wasPressedThisFrame) ManageState(FisherState.Waiting);
            if (Keyboard.current.digit4Key.wasPressedThisFrame) ManageState(FisherState.Swinging);
        }
        RotateRod();
    }

    void SubscribeInput()
    {
        input = new FishRumbleInput();

        input.Player.Cast.performed += Cast;
        input.Player.Reel.performed += Reel;
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

        springJointConfig.GetFromState(currentState).ApplyTo(lineSpringJoint);

        switch (newState)
        {
            case FisherState.Idle:
                hookRigidbody.linearVelocity = Vector3.zero;
                hookRigidbody.angularVelocity = Vector3.zero;
                break;
            case FisherState.Waiting:
                _hook.Release();
                break;
            case FisherState.Swinging:
                CatchFish();
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
        Debug.Log("投げる");
        if (currentState != FisherState.Idle) return;
        ManageState(FisherState.Waiting);
        hookRigidbody.isKinematic = true;
        hookTransform.position = Vector3.zero;
        hookRigidbody.isKinematic = false;
        hookRigidbody.AddForce(this.transform.forward * castPower, ForceMode.Impulse);
    }

    void Reel(InputAction.CallbackContext context)
    {
        Debug.Log("引きつける");
        if (currentState != FisherState.Waiting) return;

        if(isAbleCatch)
        {
            ManageState(FisherState.Swinging);
        }
        else
        {
            ManageState(FisherState.Idle);
        }
    }

    /// <summary>
    /// スマホを振る動作に対応する入力コールバック。
    /// Swinging 中は振り回し攻撃を行う。
    /// </summary>
    void Shake(InputAction.CallbackContext input)
    {
        if (currentState == FisherState.Swinging)
            SwingAttack(input);
    }


    /// <summary>
    /// 振り回し攻撃。Shakeするたびに糸を縮め、minLineLengthまで巻き取ったらIdleに戻る。
    /// </summary>
    async void SwingAttack(InputAction.CallbackContext _input)
    {
        if(isSwinging || SwingCoolTimer()) return;

        Debug.Log("Swing");
        isSwinging = true;
        float inputValue = _input.ReadValue<float>();
        if(bigAttackPowerThresholdValue < inputValue)
        {
            springJointConfig.GetFromAttack(2).ApplyTo(lineSpringJoint);
        }
        else
        if(middleAttackPowerThresholdValue < inputValue)
        {
            springJointConfig.GetFromAttack(1).ApplyTo(lineSpringJoint);
        }
        else
        if(smallAttackPowerThresholdValue < inputValue)
        {
            springJointConfig.GetFromAttack(0).ApplyTo(lineSpringJoint);
        }

        //Hookの加速
        Vector3 _myPos = this.transform.position;
        Vector3 _hookPos = hookTransform.transform.position;

        Vector3 meToFishVector = _hookPos - _myPos;
        meToFishVector.y = 0f;

        hookRigidbody.isKinematic = true;
        hookRigidbody.isKinematic = false;

        Vector3 attackForce = Quaternion.Euler(0f, 45f, 0f) * meToFishVector.normalized * fishAcceraratePower;
        hookRigidbody.AddForce(attackForce, ForceMode.Impulse);

        //待機
        await UniTask.Delay(swingFinishTimeMs);

        //元の長さに戻す
        springJointConfig.GetFromState(FisherState.Swinging).ApplyTo(lineSpringJoint);
        hookRigidbody.isKinematic = true;
        hookRigidbody.isKinematic = false;
        isSwinging = false;
    }

    bool SwingCoolTimer()
    {
        swingCoolTimer = Time.deltaTime;
        float beforeTime = Time.deltaTime - swingCoolTimer;

        if(beforeTime > swingCoolTime) 
        {
            swingCoolTime = 0;
            return true;
        }

        return false;
    }


    void CatchFish()
    {
        if (currentState != FisherState.Waiting && caughtFish != null) return;

        hookRigidbody.isKinematic = true;
        Fish fish = Instantiate(fishPrefab,hookTransform).GetComponent<Fish>();
        _hook.CatchFish(fish.transform);
        caughtFish = fish.GetComponent<Fish>();
        caughtFish.onDepleted += DropFish;
        hookRigidbody.isKinematic = false;
    }

    void AbleCatch()
    {
        isAbleCatch = true;
    }
    void UnableCatch()
    {
        isAbleCatch = false;
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
