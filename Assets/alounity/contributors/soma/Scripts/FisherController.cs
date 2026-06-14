using UnityEngine;
using UnityEngine.InputSystem;
using FishRumble;
public class FisherController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform rodTransform;
    [SerializeField] private Transform lineAttachPoint;
    [SerializeField] private Transform hookTransform;
    private Rigidbody hookRigidbody;
    [SerializeField] private SpringJoint lineSpringJoint;
    [SerializeField] private FisherSpringJointConfig springJointConfig;
    [Header("ControlValues")]
    [Range(-1, 1)]
    [SerializeField] private float horizontalInput;
    [Header("Settings")]
    [SerializeField] private SomaInputActions input;
    [SerializeField] private bool isDebugMode = false;
    [Header("Parameters")]
    [SerializeField] private float rodRotationSpeed = 5f;
    [SerializeField] private float rodMinRotation = -45f;
    [SerializeField] private float rodMaxRotation = 45f;
    [SerializeField] private FisherState currentState = FisherState.Idle;
    [SerializeField] private Vector3 castDirection = new Vector3(0, 0, 10);
    private int reelCount = 0;
    [SerializeField] private int requiredReelShakeCount = 5;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        SubscribeInput();
        GetHookReferences();
        SetupHook(false);
    }

    void GetHookReferences()
    {
        hookRigidbody = hookTransform.GetComponent<Rigidbody>();
        lineSpringJoint = hookTransform.GetComponent<SpringJoint>();
    }
    void Update()
    {
        if(isDebugMode)
        {
            // デバッグ時State切り替え
            if (Keyboard.current.digit1Key.wasPressedThisFrame) ManageState(FisherState.Idle);
            if (Keyboard.current.digit2Key.wasPressedThisFrame) ManageState(FisherState.Waiting);
            if (Keyboard.current.digit3Key.wasPressedThisFrame) ManageState(FisherState.Catching);
            if (Keyboard.current.digit4Key.wasPressedThisFrame) ManageState(FisherState.Swinging);   
        }
        RotateRod();
    }

    void SubscribeInput()
    {
        input = new SomaInputActions();

        input.Fisher.Cast.performed += Cast;
        input.Fisher.Reel.performed += Reel;

        input.Enable();
    }
    void StartGame()
    {
        
    }

    /// <summary>
    /// 竿の回転を管理する。horizontalInputの値に応じて、rodMinRotation〜rodMaxRotationの範囲で回転させる.
    /// </summary>
    void RotateRod()
    {
        if(!isDebugMode)
        {
            horizontalInput = input.Fisher.Rotate.ReadValue<float>();
        }

        float targetAngle = Mathf.Lerp(rodMinRotation, rodMaxRotation, (horizontalInput + 1f) / 2f);
        float currentAngle = rodTransform.localEulerAngles.y;
        // localEulerAngles は 0〜360 で返るので -180〜180 に変換する
        if (currentAngle > 180f) currentAngle -= 360f;

        float smoothedAngle = Mathf.Lerp(currentAngle, targetAngle, Time.deltaTime * rodRotationSpeed);
        rodTransform.localEulerAngles = new Vector3(
            rodTransform.localEulerAngles.x,
            smoothedAngle,
            rodTransform.localEulerAngles.z
        );
    }

    /// <summary>
    /// FisherStateを管理し、状態に応じたSpringJointのパラメータを適用する。
    /// </summary>
    /// <param name="newState"></param>
    void ManageState(FisherState newState)
    {
        currentState = newState;
        springJointConfig.Get(currentState).ApplyTo(lineSpringJoint);
    }

    /// <summary>
    /// Idle状態のときにキャストする処理。竿を前に振り、フックを飛ばす。
    /// </summary>
    /// <param name="context"></param>
    void Cast(InputAction.CallbackContext context)
    {
        // Idle状態のときにしかキャストできないようにする
        if (currentState != FisherState.Idle) return;

        // 前に投げる
        hookRigidbody.isKinematic = false;
        hookRigidbody.AddForce(lineAttachPoint.forward + castDirection, ForceMode.Impulse);
    }

    /// <summary>
    /// Catching状態のときにリールを巻く処理。一定回数巻いたらSwinging状態に移行する。
    /// </summary>
    /// <param name="context"></param>
    void Reel(InputAction.CallbackContext context)
    {
        //　Catching時以外は動作しないようにする
        if (currentState != FisherState.Catching)
        {
            PullingLine();
        }
        else if (currentState == FisherState.Swinging)
        {
            ShortenLine();
        }
    }

    void PullingLine()
    {
        reelCount++;
        if (reelCount >= requiredReelShakeCount)
        {
            hookRigidbody.isKinematic = false;
            hookRigidbody.AddForce(lineAttachPoint.forward * 10, ForceMode.Impulse);
            ManageState(FisherState.Swinging);
            reelCount = 0;
        }
    }
    void ShortenLine()
    {
        lineSpringJoint.maxDistance = Mathf.Max(0.5f, lineSpringJoint.maxDistance - 0.5f);
    }

    /// <summary>
    /// フックの物理挙動を切り替える。リリース状態なら物理挙動を有効にし、そうでないなら無効にする。
    /// </summary>
    /// <param name="isReleased"></param>
    void SetupHook(bool isReleased)
    {
        if (isReleased)
        {
            hookRigidbody.isKinematic = false;
            hookTransform.SetParent(null);
        }
        else
        {
            hookRigidbody.isKinematic = true;
            hookTransform.SetParent(lineAttachPoint);
        }
        
    }
}
