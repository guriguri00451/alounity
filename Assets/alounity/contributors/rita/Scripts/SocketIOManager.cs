using System;
using SocketIOClient;
using UnityEngine;

/// <summary>
/// Socket.IO接続を管理するSingleton
/// </summary>
public class SocketIOManager : MonoBehaviour
{
    [SerializeField] string serverUrl = "http://localhost:3000";
    [SerializeField] string roomId = "default";
    [SerializeField] bool autoConnect = true;
    [SerializeField] float reconnectDelay = 3f;

    static SocketIOManager instance;
    SocketIO socket;
    bool isReconnecting;

    public static SocketIOManager Instance => instance;
    public SocketIO Socket => socket;
    public bool IsConnected { get; private set; }

    public event Action OnConnected;
    public event Action OnDisconnected;
    public event Action<string> OnConnectionError;

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        if (autoConnect)
        {
            Connect();
        }
    }

    public async void Connect()
    {
        if (socket != null)
        {
            Disconnect();
        }

        socket = new SocketIO(new Uri(serverUrl));

        socket.OnConnected += OnSocketConnected;
        socket.OnDisconnected += OnSocketDisconnected;
        socket.OnError += OnSocketError;

        try
        {
            await socket.ConnectAsync();
        }
        catch (Exception e)
        {
            Debug.LogError($"[SocketIO] 接続失敗: {e.Message}");
            ScheduleReconnect();
        }
    }

    async void OnSocketConnected(object sender, EventArgs e)
    {
        IsConnected = true;
        isReconnecting = false;
        Debug.Log($"[SocketIO] 接続完了: {serverUrl}");

        await socket.EmitAsync("unity:connect", new object[] { new { roomId } });

        OnConnected?.Invoke();
    }

    void OnSocketDisconnected(object sender, string reason)
    {
        IsConnected = false;
        Debug.Log($"[SocketIO] 切断: {reason}");
        OnDisconnected?.Invoke();
        ScheduleReconnect();
    }

    void OnSocketError(object sender, string error)
    {
        Debug.LogError($"[SocketIO] エラー: {error}");
        OnConnectionError?.Invoke(error);
        ScheduleReconnect();
    }

    void ScheduleReconnect()
    {
        if (isReconnecting) return;
        isReconnecting = true;
        Invoke(nameof(Connect), reconnectDelay);
    }

    public void Disconnect()
    {
        if (socket != null)
        {
            socket.OnConnected -= OnSocketConnected;
            socket.OnDisconnected -= OnSocketDisconnected;
            socket.OnError -= OnSocketError;
            _ = socket.DisconnectAsync();
            socket = null;
        }
        IsConnected = false;
    }

    void OnDestroy()
    {
        Disconnect();
    }

    void OnApplicationQuit()
    {
        Disconnect();
    }
}
