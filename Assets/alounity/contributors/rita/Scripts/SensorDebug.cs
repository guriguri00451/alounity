using UnityEngine;

/// <summary>
/// SensorDataReceiver から受け取ったセンサーデータをコンソールに表示するデバッグ用
/// </summary>
public class SensorDebug : MonoBehaviour
{
    [SerializeField] SensorDataReceiver receiver;

    void OnEnable()
    {
        if (receiver == null)
        {
            receiver = GetComponent<SensorDataReceiver>();
            if (receiver == null)
            {
                Debug.LogError("[SensorDebug] SensorDataReceiver が見つかりません");
                return;
            }
        }

        receiver.onSensorDataReceived.AddListener(OnSensorData);
        receiver.onPaddleRightInput.AddListener(OnPaddleRight);
        receiver.onPaddleLeftInput.AddListener(OnPaddleLeft);
        receiver.onFisherInput.AddListener(OnFisher);
    }

    void OnDisable()
    {
        if (receiver == null) return;
        receiver.onSensorDataReceived.RemoveListener(OnSensorData);
        receiver.onPaddleRightInput.RemoveListener(OnPaddleRight);
        receiver.onPaddleLeftInput.RemoveListener(OnPaddleLeft);
        receiver.onFisherInput.RemoveListener(OnFisher);
    }

    void OnSensorData(SensorDataPayload data)
    {
        Debug.Log(
            $"[SensorDebug] 受信: role={data.role} "
            + $"accel=({data.accel.x:F2}, {data.accel.y:F2}, {data.accel.z:F2}) "
            + $"rotation=({data.rotation.alpha:F1}, {data.rotation.beta:F1}, {data.rotation.gamma:F1}) "
            + $"orientation=({data.orientation.alpha:F1}, {data.orientation.beta:F1}, {data.orientation.gamma:F1}) "
            + $"timestamp={data.timestamp}"
        );
    }

    void OnPaddleRight(AxisData accel)
    {
        Debug.Log($"[SensorDebug] 右オール入力: ({accel.x:F2}, {accel.y:F2}, {accel.z:F2})");
    }

    void OnPaddleLeft(AxisData accel)
    {
        Debug.Log($"[SensorDebug] 左オール入力: ({accel.x:F2}, {accel.y:F2}, {accel.z:F2})");
    }

    void OnFisher(SensorDataPayload data)
    {
        Debug.Log($"[SensorDebug] 釣り入力: accel=({data.accel.x:F2}, {data.accel.y:F2}, {data.accel.z:F2})");
    }
}
