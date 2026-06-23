using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class BoatController : MonoBehaviour
{
    int playerID;
    public int PlayerID
    {
        get
        {
            return playerID;
        }
        
        set 
        {
            playerID = value;
        }
    }
    [SerializeField] Transform center;
    [SerializeField] WheelCollider[] wheels;
    [SerializeField] InputActionProperty[] boat;
    [SerializeField] OarAnimSetter[] oarAnim;
    [Header("Parametor")]
    [SerializeField] float driveValueThreshold = 0.8f;
    [SerializeField] float breakValueThreshold = 0.2f;
    [SerializeField] float power = 30;
    [SerializeField] float breakPower = 30;
    Rigidbody rb;

    // [0]=右パドル, [1]=左パドル。センサーから更新されなければ0のままInputActionにフォールバック
    float[] sensorInputValues = new float[2];

    public void OnPaddleRightInput(AxisData data)
        => sensorInputValues[1] = Mathf.Lerp(0,1,Mathf.Sqrt(data.x * data.x + data.y * data.y + data.z * data.z));
        

    public void OnPaddleLeftInput(AxisData data)
        => sensorInputValues[0] = Mathf.Lerp(0,1,Mathf.Sqrt(data.x * data.x + data.y * data.y + data.z * data.z));

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        for(int i=0;i<boat.Length;i++) boat[i].action.Enable();
    }

    public void FixBoat()
    {
        rb.isKinematic = true;
        rb.isKinematic = false;
    }

    void FixedUpdate()
    {
        for(int i=0;i<wheels.Length;i++)
        {
            float input = (i < sensorInputValues.Length && sensorInputValues[i] > 0f)
                ? sensorInputValues[i]
                : boat[i].action.ReadValue<float>();

            if(input >= driveValueThreshold)
            {
                wheels[i].brakeTorque = 0;
                wheels[i].motorTorque = input*power;
                if(oarAnim.Length != 0) oarAnim[i].SetSpeed(input);
            }
            else if(input >= breakValueThreshold)
            {
                if(oarAnim.Length != 0) oarAnim[i].SetSpeed(0);
            }
            else
            {
                wheels[i].brakeTorque = breakPower;
                if(oarAnim.Length != 0) oarAnim[i].SetSpeed(0);
            }
            
        }
    }
    void OnEnable()
    {
        for(int i=0;i<boat.Length;i++) boat[i].action.Enable();
    }
    void OnDisable()
    {
        for(int i=0;i<boat.Length;i++)boat[i].action.Disable();
    }
}
