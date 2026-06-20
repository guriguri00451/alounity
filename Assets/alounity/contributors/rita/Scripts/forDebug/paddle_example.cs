using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PaddleExample : MonoBehaviour
{
    Rigidbody rb;
    
    [Header("オール設定")]
    [SerializeField] float forceMultiplier = 50f;
    [SerializeField] float shakeThreshold = 0.5f;
    
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void OnPaddleInput(AxisData accel)
    {
        // 加速度の大きさを計算（ベクトルの長さ）
        float magnitude = Mathf.Sqrt(accel.x * accel.x + accel.y * accel.y + accel.z * accel.z);
        
        // 閾値以下の動きは無視
        if (magnitude < shakeThreshold) return;
        
        // 振る強さに応じて力をかける（上方向に推進力）
        float paddleForce = magnitude * forceMultiplier;
        rb.AddForce(0, paddleForce, 0, ForceMode.Acceleration);
        
        Debug.Log($"[Paddle] 強度: {magnitude:F2}, 力: {paddleForce:F2}");
    }
}
