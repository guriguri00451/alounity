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
    [SerializeField] private ConfigurableJoint lineJoint;
    [SerializeField] private FisherLineJointConfig jointConfig;
    [Header("ControlValues")]
    [Range(-1, 1)]
    [SerializeField] private float horizontalInput;

    [Header("Settings")]
    [SerializeField] private FishConfig fishSettings;
    [SerializeField] private FishRumbleInput input;
    [SerializeField] private bool isDebugMode = false;

    [Header("FishRodParams")]
    [SerializeField] private float addUpVector = 0.1f;
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
    [Header("Sensor Thresholds")]
    [SerializeField] float castThreshold = 1.5f;
    [SerializeField] float reelThreshold = 1.5f;
    [SerializeField] float shakeThreshold = 2.0f;
    private int playerID;
    public int PlayerID
    {
        get
        {
            return playerID;
        }
        
        set 
        {
            playerID = value;
        }
    }
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
        ManageState(FisherState.Idle);
    }

    void GetHookReferences()
    {
        hookRigidbody = hookTransform.GetComponent<Rigidbody>();
        _hook = hookTransform.GetComponent<Hook>();
        _hook.onAbleCatch += AbleCatch;
        _hook.onUnableCatch += UnableCatch;
    }

    void Update()
    {
        if (isDebugMode)
        {
            if (Keyboard.current.digit1Key.wasPressedThisFrame) ManageState(FisherState.Idle);
            if (Keyboard.current.digit2Key.wasPressedThisFrame) ManageState(FisherState.Waiting);
            if (Keyboard.current.digit4Key.wasPressedThisFrame) ManageState(FisherState.Swinging);
        }
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
    /// FisherState を遷移し、状態に応じた SpringJoint パラメータを適用する。
    /// </summary>
    void ManageState(FisherState newState)
    {
        Debug.Log($"State changed: {currentState} -> {newState}");
        currentState = newState;

        jointConfig.GetFromState(currentState).ApplyTo(lineJoint);

        switch (newState)
        {
            case FisherState.Idle:
                hookRigidbody.isKinematic = true;
                hookRigidbody.isKinematic = false;
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

    void Cast(InputAction.CallbackContext _) => ExecuteCast();

    void Reel(InputAction.CallbackContext _) => ExecuteReel();

    /// <summary>
    /// スマホを振る動作に対応する入力コールバック。
    /// Swinging 中は振り回し攻撃を行う。
    /// </summary>
    void Shake(InputAction.CallbackContext ctx)
    {
        if (currentState == FisherState.Swinging)
            SwingAttack(ctx.ReadValue<float>());
    }

    /// <summary>
    /// Idle 状態のときにキャストする。針を切り離して前方に投げ、Waiting へ遷移する。
    /// </summary>
    void ExecuteCast()
    {
        Debug.Log("投げる");
        if (currentState != FisherState.Idle) return;
        ManageState(FisherState.Waiting);
        hookRigidbody.isKinematic = true;
        hookRigidbody.isKinematic = false;
        Vector3 castVector = this.transform.forward;
        castVector.y += addUpVector;
        hookRigidbody.AddForce(castVector.normalized * castPower, ForceMode.Impulse);
    }

    void ExecuteReel()
    {
        if (currentState == FisherState.Waiting || currentState == FisherState.Swinging)
        {
            Debug.Log("引きつける");
            if (isAbleCatch)
                ManageState(FisherState.Swinging);
            else
                ManageState(FisherState.Idle);
        }
    }

    /// <summary>
    /// SensorDataReceiverから呼ばれる。加速度をジェスチャー判定してCast/Reel/Shakeに変換する。
    /// </summary>
    public void OnSensorInput(SensorDataPayload data)
    {
        if (data.accel == null) return;

        //rotationの値に応じて釣り竿を振る
        float force = data.rotation.alpha;
        if (force > castThreshold)
            ExecuteReel();
        else if (force < -reelThreshold)
        {
            if(currentState == FisherState.Swinging)
            {
                SwingAttack(force);
                return;
            }
            ExecuteCast();
        }

    }


    /// <summary>
    /// 振り回し攻撃。Shakeするたびに糸を縮め、minLineLengthまで巻き取ったらIdleに戻る。
    /// </summary>
    async void SwingAttack(float inputValue)
    {
        if(isSwinging || SwingCoolTimer()) return;

        isSwinging = true;
        if(bigAttackPowerThresholdValue < inputValue)
        {
            jointConfig.GetFromAttack(2).ApplyTo(lineJoint);
        }
        else
        if(middleAttackPowerThresholdValue < inputValue)
        {
            jointConfig.GetFromAttack(1).ApplyTo(lineJoint);
        }
        else
        if(smallAttackPowerThresholdValue < inputValue)
        {
            jointConfig.GetFromAttack(0).ApplyTo(lineJoint);
        }

        //Hookの加速
        Vector3 _myPos = this.transform.position;
        Vector3 _hookPos = hookTransform.transform.position;

        Vector3 meToFishVector = _hookPos - _myPos;
        meToFishVector.y = 0f;

        hookRigidbody.isKinematic = true;
        hookRigidbody.isKinematic = false;

        Vector3 swingVector = this.transform.forward;
        hookRigidbody.AddForce(swingVector.normalized * fishAcceraratePower, ForceMode.Impulse);

        //待機
        await UniTask.Delay(swingFinishTimeMs);

        //元の長さに戻す
        jointConfig.GetFromState(FisherState.Swinging).ApplyTo(lineJoint);
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
        Fish fish = Instantiate(fishSettings.GetFish(),hookTransform).GetComponent<Fish>();
        fish.PlayerID = playerID;
        _hook.CatchFish(fish.transform);
        caughtFish = fish.GetComponent<Fish>();
        caughtFish.onDepleted += DropFish;
        hookRigidbody.isKinematic = false;
    }

    void AbleCatch()
    {
        if(caughtFish == null) isAbleCatch = true;
    }
    void UnableCatch()
    {
        isAbleCatch = false;
    }

    public void DropFish()
    {
        if(caughtFish != null) 
        {

            caughtFish.onDepleted -= DropFish;
            caughtFish.SetAttackActive(false);
            caughtFish = null;
        }
        _hook.ReleaseFish();

        ManageState(FisherState.Idle);
    }

    /// <summary>
    /// 全イベント購読を解除してリソースを解放する。
    /// </summary>
    void OnDestroy()
    {
        input.Player.Cast.performed -= Cast;
        input.Player.Reel.performed -= Reel;
        input.Player.Shake.performed -= Shake;
        input.Disable();
        input.Dispose();

        if (_hook != null)
        {
            _hook.onAbleCatch -= AbleCatch;
            _hook.onUnableCatch -= UnableCatch;
        }

        if (caughtFish != null)
        {
            caughtFish.onDepleted -= DropFish;
        }
    }
}
