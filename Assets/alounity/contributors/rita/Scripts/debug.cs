using UnityEngine;

public class debug : MonoBehaviour
{
  Rigidbody rb;

    public void OnPaddleInput(AxisData accel)
    {
        // accel.x, accel.y, accel.z でカヤックを動かす
        rb.AddForce(new Vector3(accel.x, 0, accel.z), ForceMode.Acceleration);
    }
}
