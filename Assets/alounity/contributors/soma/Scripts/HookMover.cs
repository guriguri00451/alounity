using UnityEngine;

/// <summary>
/// 釣り針の移動と浮力を制御するコンポーネント。
/// Y座標が0以下（水中）になると上向きの浮力を加える。
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class HookMover : MonoBehaviour
{
    [SerializeField, Tooltip("浮力の最大強度")]
    private float buoyancyForce = 9.81f;

    [SerializeField, Tooltip("浮力が最大になる深さ（m）")]
    private float maxBuoyancyDepth = 3f;

    [SerializeField, Tooltip("水中での線形抵抗（大きいほど早く静止する）")]
    private float waterLinearDamping = 3f;

    [SerializeField, Tooltip("水中での角度抵抗")]
    private float waterAngularDamping = 1f;

    private Rigidbody rb;
    private float defaultLinearDamping;
    private float defaultAngularDamping;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        defaultLinearDamping = rb.linearDamping;
        defaultAngularDamping = rb.angularDamping;
    }

    void FixedUpdate()
    {
        bool isUnderwater = transform.position.y < 0f;

        // 水中では抵抗を増やして跳ね返りを防ぐ
        rb.linearDamping  = isUnderwater ? waterLinearDamping  : defaultLinearDamping;
        rb.angularDamping = isUnderwater ? waterAngularDamping : defaultAngularDamping;

        if (isUnderwater)
        {
            ApplyBuoyancy();
        }
    }

    /// <summary>
    /// 深さに応じた浮力を Rigidbody に加える。
    /// 深いほど強くなるが maxBuoyancyDepth でクランプする。
    /// </summary>
    private void ApplyBuoyancy()
    {
        float depth = Mathf.Min(-transform.position.y, maxBuoyancyDepth);
        float forceMagnitude = buoyancyForce * (depth / maxBuoyancyDepth);
        rb.AddForce(Vector3.up * forceMagnitude, ForceMode.Acceleration);
    }
}
