using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform target;
    public Vector3 offset = new Vector3(0, 3, -6);

    public float smoothTime = 0.3f;
    Vector3 velocity = Vector3.zero;

    Rigidbody targetRb;

    void Start()
    {
        targetRb = target.GetComponent<Rigidbody>();
    }

    void LateUpdate()
    {
        float speed = targetRb.linearVelocity.magnitude;

        // スピードで遅れを変える
        float dynamicSmooth = Mathf.Lerp(0.1f, 0.5f, speed / 10f);

        Vector3 targetPos = target.position + offset;

        transform.position = Vector3.SmoothDamp(
            transform.position,
            targetPos,
            ref velocity,
            dynamicSmooth
        );

        transform.LookAt(target);
    }
}