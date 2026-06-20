using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

/// <summary>
/// SensorDataReceiver が発火するセンサーイベントを SmartphoneDevice の InputState に変換して流し込む。
/// このコンポーネントを SensorDataReceiver と同じ GameObject に置き、Inspector で receiver を設定すること。
/// </summary>
[DefaultExecutionOrder(-50)]
public class SmartphoneInputBridge : MonoBehaviour
{
    [SerializeField] SensorDataReceiver receiver;

    // 現在のデバイス状態を保持（部分更新でも他フィールドをゼロにしないため）
    SmartphoneDeviceState currentState;
    SmartphoneDevice device;

    // 左右オール：この加速度（m/s²）で float 値が 1.0 になる
    const float PaddleMaxAccel = 9.8f;
    // キャスト：この角速度（deg/s）で float 値が 1.0 になる
    const float CastMaxRotation = 180f;
    // シェイク：この角速度（deg/s）で float 値が 1.0 になる
    const float ShakeMaxRotation = 360f;

    void Awake()
    {
        device = InputSystem.AddDevice<SmartphoneDevice>("SmartphoneController");
        currentState = default;
    }

    void Start()
    {
        if (receiver == null)
        {
            Debug.LogError("[SmartphoneInputBridge] SensorDataReceiver が未設定です。Inspector で設定してください。");
            return;
        }
        receiver.onPaddleLeftInput_A.AddListener(ApplyPaddleLeft);
        receiver.onPaddleLeftInput_B.AddListener(ApplyPaddleLeft);
        receiver.onPaddleRightInput_A.AddListener(ApplyPaddleRight);
        receiver.onPaddleRightInput_B.AddListener(ApplyPaddleRight);
        receiver.onFisherInput_A.AddListener(ApplyFisher);
        receiver.onFisherInput_B.AddListener(ApplyFisher);
    }

    void OnDestroy()
    {
        if (receiver != null)
        {
            receiver.onPaddleLeftInput_A.RemoveListener(ApplyPaddleLeft);
            receiver.onPaddleLeftInput_B.RemoveListener(ApplyPaddleLeft);
            receiver.onPaddleRightInput_A.RemoveListener(ApplyPaddleRight);
            receiver.onPaddleRightInput_B.RemoveListener(ApplyPaddleRight);
            receiver.onFisherInput_A.RemoveListener(ApplyFisher);
            receiver.onFisherInput_B.RemoveListener(ApplyFisher);
        }
        if (device != null && device.added)
            InputSystem.RemoveDevice(device);
    }

    void ApplyPaddleLeft(AxisData accel)
    {
        currentState.boatL = Normalize(ComputeMagnitude(accel), PaddleMaxAccel);
        QueueState();
    }

    void ApplyPaddleRight(AxisData accel)
    {
        currentState.boatR = Normalize(ComputeMagnitude(accel), PaddleMaxAccel);
        QueueState();
    }

    void ApplyFisher(SensorDataPayload data)
    {
        if (data.orientation != null)
        {
            // alpha は 0〜360 度 → [-1, 1] に変換
            currentState.rotate = (data.orientation.alpha / 180f) - 1f;
        }
        if (data.rotation != null)
        {
            // beta（前後傾き角速度）をキャストに使う
            currentState.cast = Normalize(Mathf.Abs(data.rotation.beta), CastMaxRotation);
            // alpha（横傾き角速度）をシェイクに使う
            currentState.shake = Normalize(Mathf.Abs(data.rotation.alpha), ShakeMaxRotation);
        }
        QueueState();
    }

    void QueueState()
    {
        if (device == null || !device.added) return;
        InputSystem.QueueStateEvent(device, currentState);
    }

    static float ComputeMagnitude(AxisData accel)
    {
        if (accel == null) return 0f;
        return Mathf.Sqrt(accel.x * accel.x + accel.y * accel.y + accel.z * accel.z);
    }

    static float Normalize(float value, float max) => Mathf.Clamp01(value / max);
}
