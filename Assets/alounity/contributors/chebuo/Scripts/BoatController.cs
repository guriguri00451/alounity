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

    // [0]=右パドル, [1]=左パドル。センサーから更新されなければ0のままInputActionにフォールバック
    float[] sensorInputValues = new float[2];

    public void OnPaddleRightInput(AxisData data)
        => sensorInputValues[0] = Mathf.Clamp01(Mathf.Abs(data.z));

    public void OnPaddleLeftInput(AxisData data)
        => sensorInputValues[1] = Mathf.Clamp01(Mathf.Abs(data.z));

    void Start()
    {
        for(int i=0;i<boat.Length;i++) boat[i].action.Enable();
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
