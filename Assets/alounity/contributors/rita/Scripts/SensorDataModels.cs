/// <summary>
/// センサーデータペイロード（sensor:data イベントのデータ形式）
/// </summary>
public class SensorDataPayload
{
    public string playerId { get; set; }
    public string role { get; set; }
    public AxisData accel { get; set; }
    public RotationData rotation { get; set; }
    public OrientationData orientation { get; set; }
    public long timestamp { get; set; }
}

/// <summary>
/// 加速度データ
/// </summary>
public class AxisData
{
    public float x { get; set; }
    public float y { get; set; }
    public float z { get; set; }
}

/// <summary>
/// 回転角速度データ
/// </summary>
public class RotationData
{
    public float alpha { get; set; }
    public float beta { get; set; }
    public float gamma { get; set; }
}

/// <summary>
/// 端末の向きデータ
/// </summary>
public class OrientationData
{
    public float alpha { get; set; }
    public float beta { get; set; }
    public float gamma { get; set; }
}

/// <summary>
/// host:create_ack イベントのレスポンス
/// </summary>
public class HostCreateAckPayload
{
    public bool ok { get; set; }
    public string roomId { get; set; }
    public string error { get; set; }
}
