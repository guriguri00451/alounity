using System.Threading;
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

    SynchronizationContext mainThread;

    void Start()
    {
        mainThread = SynchronizationContext.Current;

        var manager = SocketIOManager.Instance;
        Debug.Log($"[SensorDataReceiver] Start called. manager={(manager != null)}, connected={manager?.IsConnected}");
        if (manager == null)
        {
            Debug.LogError("[SensorDataReceiver] SocketIOManager.Instance is null");
            return;
        }

        if (manager.IsConnected && manager.Socket != null)
        {
            manager.Socket.On("sensor:data", OnSensorData);
            Debug.Log("[SensorDataReceiver] Registered sensor:data callback (direct)");
        }
        else
        {
            manager.OnConnected += RegisterSensorHandler;
            Debug.Log("[SensorDataReceiver] Waiting for connection to register callback");
        }
    }

    void RegisterSensorHandler()
    {
        var manager = SocketIOManager.Instance;
        if (manager?.Socket != null)
        {
            manager.Socket.On("sensor:data", OnSensorData);
            Debug.Log("[SensorDataReceiver] Registered sensor:data callback (deferred)");
        }
        manager.OnConnected -= RegisterSensorHandler;
    }

    System.Threading.Tasks.Task OnSensorData(IEventContext response)
    {
        mainThread.Post(_ =>
        {
            try
            {
                var payload = response.GetValue<SensorDataPayload>(0);
                if (payload == null) return;

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
        }, null);
        return System.Threading.Tasks.Task.CompletedTask;
    }
}
