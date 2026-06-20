using System;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using SocketIOClient;
using UnityEngine;

public class SocketIOManager : MonoBehaviour
{
    public enum GameMode { Single, Versus }

    [SerializeField] string serverUrl = "http://localhost:3000";
    [SerializeField] GameMode gameMode = GameMode.Versus;
    [SerializeField] bool autoConnect = true;
    [SerializeField] float reconnectDelay = 3f;

    static SocketIOManager instance;
    SocketIO socket;
    bool isReconnecting;
    Uri serverUri;

    public static SocketIOManager Instance => instance;
    public SocketIO Socket => socket;
    public bool IsConnected { get; private set; }
    public string RoomId { get; private set; } = "";
    public Uri ServerUri => serverUri;
    public GameMode Mode => gameMode;
    public string GameModeString => gameMode == GameMode.Versus ? "versus" : "single";

    public event Action OnConnected;
    public event Action OnRoomCreated;
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

        RoomId = "";
        serverUri = new Uri(serverUrl);

        socket = new SocketIO(serverUri);

        socket.OnConnected += OnSocketConnected;
        socket.OnDisconnected += OnSocketDisconnected;
        socket.OnError += OnSocketError;

        socket.On("host:create_ack", OnHostCreateAck);

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
        // メインスレッドに切り替える
        await UniTask.SwitchToMainThread();
        
        IsConnected = true;
        isReconnecting = false;
        Debug.Log($"[SocketIO] 接続完了: {serverUrl}");

        await socket.EmitAsync("host:create", new object[] { new { gameMode = GameModeString } });

        OnConnected?.Invoke();
    }

    async Task OnHostCreateAck(IEventContext response)
    {
        try
        {
            var data = response.GetValue<HostCreateAckPayload>(0);

            if (data.ok)
            {
                RoomId = data.roomId;
                if (!string.IsNullOrEmpty(data.gameMode))
                {
                    gameMode = data.gameMode == "versus" ? GameMode.Versus : GameMode.Single;
                }
                Debug.Log($"[Room] 作成完了: {RoomId} (mode: {gameMode})");
                
                // メインスレッドに切り替えてからコールバックを呼び出す
                await UniTask.SwitchToMainThread();
                OnRoomCreated?.Invoke();
            }
            else
            {
                Debug.LogError($"[Room] 作成失敗: {data.error}");
                ScheduleReconnect();
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"[SocketIO] host:create_ack パースエラー: {e.Message}");
            ScheduleReconnect();
        }
    }

    public void CloseRoom()
    {
        if (socket != null && !string.IsNullOrEmpty(RoomId))
        {
            _ = socket.EmitAsync("host:close", new object[] { new { roomId = RoomId } });
        }
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
            CloseRoom();
            socket.OnConnected -= OnSocketConnected;
            socket.OnDisconnected -= OnSocketDisconnected;
            socket.OnError -= OnSocketError;
            _ = socket.DisconnectAsync();
            socket = null;
        }
        RoomId = "";
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
