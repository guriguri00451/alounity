using SocketIOClient;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// sensor:data イベントを受信し、役割に応じてゲームロジックに適用する
/// </summary>
public class SensorDataReceiver : MonoBehaviour
{
    [Header("イベント")]
    public UnityEvent<SensorDataPayload> onSensorDataReceived;

    [Header("役割別イベント")]
    public UnityEvent<AxisData> onPaddleRightInput;
    public UnityEvent<AxisData> onPaddleLeftInput;
    public UnityEvent<SensorDataPayload> onFisherInput;

    void OnEnable()
    {
        var manager = SocketIOManager.Instance;
        if (manager?.Socket != null)
        {
            manager.Socket.On("sensor:data", OnSensorData);
        }
    }

    System.Threading.Tasks.Task OnSensorData(IEventContext response)
    {
        try
        {
            var payload = response.GetValue<SensorDataPayload>(0);
            if (payload == null) return System.Threading.Tasks.Task.CompletedTask;

            onSensorDataReceived?.Invoke(payload);

            switch (payload.role)
            {
                case "paddle_right":
                    onPaddleRightInput?.Invoke(payload.accel);
                    break;

                case "paddle_left":
                    onPaddleLeftInput?.Invoke(payload.accel);
                    break;

                case "fisher":
                    onFisherInput?.Invoke(payload);
                    break;
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[SensorDataReceiver] パースエラー: {e.Message}");
        }
        return System.Threading.Tasks.Task.CompletedTask;
    }
}
