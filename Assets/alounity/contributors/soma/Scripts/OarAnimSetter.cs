using UnityEngine;

[RequireComponent(typeof(Animator))]
public class OarAnimSetter : MonoBehaviour
{
    private Animator anim;
    [SerializeField] private float offsetTime;
    private float currentSpeed;
    [SerializeField] private float maxSpeedRate = 1.4f;
    [SerializeField] private float maxTime = 1.0f;
    [SerializeField] private float noInputTimeThreshold = 0.3f;
    [SerializeField] private float decreaseTime = 1f;
    private float noInputTimer;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        anim = this.transform.GetComponent<Animator>();
        anim.SetFloat("CycleOffset",offsetTime);
    }

    public void SetSpeed(bool hasInput)
    {
        if (hasInput)
        {
            noInputTimer = 0f;
            float increaseSpeedRate = maxSpeedRate / maxTime;
            currentSpeed = Mathf.Min(currentSpeed + increaseSpeedRate * Time.fixedDeltaTime, maxSpeedRate);
        }
        else
        {
            noInputTimer += Time.fixedDeltaTime;
            if (noInputTimer >= noInputTimeThreshold)
            {
                float decreaseSpeedRate = maxSpeedRate / decreaseTime;
                currentSpeed = Mathf.Max(currentSpeed - decreaseSpeedRate * Time.fixedDeltaTime, 0f);
            }
        }
        anim.SetFloat("Speed",currentSpeed);
    }
}
