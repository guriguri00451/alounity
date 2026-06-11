using UnityEngine;
using UnityEngine.InputSystem;
using FishRumble;
public class FisherControler : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform RodTransform;
    [SerializeField] private Rigidbody hookRigidbody;
    [SerializeField] private SpringJoint hookSpringJoint;
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

    void RotateRod()
    {
        if(!isDebugMode)
        {
            horizontalInput = input.Fisher.Rotate.ReadValue<float>();
        }

        float targetAngle = Mathf.Lerp(rodMinRotation, rodMaxRotation, (horizontalInput + 1f) / 2f);
        float currentAngle = RodTransform.localEulerAngles.y;
        // localEulerAngles は 0〜360 で返るので -180〜180 に変換する
        if (currentAngle > 180f) currentAngle -= 360f;

        float smoothedAngle = Mathf.Lerp(currentAngle, targetAngle, Time.deltaTime * rodRotationSpeed);
        RodTransform.localEulerAngles = new Vector3(
            RodTransform.localEulerAngles.x,
            smoothedAngle,
            RodTransform.localEulerAngles.z
        );
    }

    void ManageState(FisherState newState)
    {
        currentState = newState;
        springJointConfig.Get(currentState).ApplyTo(hookSpringJoint);
    }

    void Cast(InputAction.CallbackContext context)
    {
        if (currentState != FisherState.Idle) return;
        hookRigidbody.isKinematic = false;
        hookRigidbody.AddForce(Vector3.forward + castDirection, ForceMode.Impulse);
    }

    void Reel(InputAction.CallbackContext context)
    {
        reelCount++;
        if (reelCount >= requiredReelShakeCount)
        {
            hookRigidbody.isKinematic = false;
            hookRigidbody.AddForce(Vector3.back * 10, ForceMode.Impulse);
            ManageState(FisherState.Swinging);
            reelCount = 0;
        }
    }
}
