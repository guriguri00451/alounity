using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform target;
    public Vector3 offset = new Vector3(0, 7, -15);

    public float smoothTime = 0.3f;
    Vector3 velocity = Vector3.zero;

    [SerializeField] Transform boat;
    [SerializeField] Rigidbody targetRb;

    void Start()
    {
        //targetRb = target.GetComponent<Rigidbody>();
    }

    void LateUpdate()
{
    float speed = targetRb.linearVelocity.magnitude;
    float dynamicSmooth = Mathf.Lerp(0.1f, 0.5f, speed / 10f);

    // 回転に追従した位置
    Vector3 rotatedOffset = boat.rotation * offset;
    Vector3 targetPos = target.position + rotatedOffset;

    transform.position = Vector3.SmoothDamp(
        transform.position,
        targetPos,
        ref velocity,
        dynamicSmooth
    );

    // 進行方向に向く
    Vector3 velocityDir = targetRb.linearVelocity;
    velocityDir.y = 0f;

    if (velocityDir.magnitude > 0.1f)
    {
        Quaternion velRot = Quaternion.LookRotation(velocityDir.normalized);
        Quaternion tilt = Quaternion.Euler(30, 0, 0); // 少し前傾させる

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            velRot*tilt,
            dynamicSmooth
        );
    }
}
}