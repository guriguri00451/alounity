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

    [Header("チーム別イベント")]
    public UnityEvent<SensorDataPayload> onTeamASensorData;
    public UnityEvent<SensorDataPayload> onTeamBSensorData;

    [Header("役割別イベント（Team A）")]
    public UnityEvent<AxisData> onPaddleRightInput_A;
    public UnityEvent<AxisData> onPaddleLeftInput_A;
    public UnityEvent<SensorDataPayload> onFisherInput_A;

    [Header("役割別イベント（Team B）")]
    public UnityEvent<AxisData> onPaddleRightInput_B;
    public UnityEvent<AxisData> onPaddleLeftInput_B;
    public UnityEvent<SensorDataPayload> onFisherInput_B;

    SynchronizationContext mainThread;

    void Start()
    {
        mainThread = SynchronizationContext.Current;
        if (mainThread == null)
        {
            Debug.LogWarning("[SensorDataReceiver] SynchronizationContext.Current is null, using fallback");
        }

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
        // バックグラウンドスレッドでデータを先に抽出
        SensorDataPayload payload = null;
        string rawText = null;
        try
        {
            payload = response?.GetValue<SensorDataPayload>(0);
            rawText = response?.RawText;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[SensorDataReceiver] データ抽出エラー: {e.Message}");
            try { Debug.LogError($"[SensorDataReceiver] RawText: {response?.RawText}"); } catch { }
            return System.Threading.Tasks.Task.CompletedTask;
        }

        void ProcessData()
        {
            try
            {
                if (payload == null)
                {
                    Debug.LogWarning($"[SensorDataReceiver] payload is null, rawText: {rawText}");
                    return;
                }

                onSensorDataReceived?.Invoke(payload);

                // チーム別イベント発火
                if (payload.team == "B")
                {
                    onTeamBSensorData?.Invoke(payload);
                }
                else
                {
                    onTeamASensorData?.Invoke(payload);
                }

                // チーム別役割イベント発火
                bool isTeamB = payload.team == "B";
                switch (payload.role)
                {
                    case "paddle_right":
                        if (payload.accel != null)
                        {
                            if (isTeamB)
                                onPaddleRightInput_B?.Invoke(payload.accel);
                            else
                                onPaddleRightInput_A?.Invoke(payload.accel);
                        }
                        break;

                    case "paddle_left":
                        if (payload.accel != null)
                        {
                            if (isTeamB)
                                onPaddleLeftInput_B?.Invoke(payload.accel);
                            else
                                onPaddleLeftInput_A?.Invoke(payload.accel);
                        }
                        break;

                    case "fisher":
                        if (isTeamB)
                            onFisherInput_B?.Invoke(payload);
                        else
                            onFisherInput_A?.Invoke(payload);
                        break;
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[SensorDataReceiver] 処理エラー: {e.Message}");
            }
        }

        if (mainThread != null)
        {
            mainThread.Post(_ => ProcessData(), null);
        }
        else
        {
            ProcessData();
        }
        return System.Threading.Tasks.Task.CompletedTask;
    }
}
