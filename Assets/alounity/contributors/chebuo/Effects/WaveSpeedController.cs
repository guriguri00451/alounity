using UnityEngine;
using UnityEngine.VFX;

public class WaveSpeedController : MonoBehaviour
{
    [SerializeField] float stopEffectSpeed = 10f;
    [SerializeField] VisualEffect waveEffect;
    [SerializeField] Rigidbody rb;

    // Update is called once per frame
    void Update()
    {
        float speed= rb.linearVelocity.magnitude;
        if(speed < stopEffectSpeed) speed = 0;
        waveEffect.SetFloat("Speed",speed);
    }
}
