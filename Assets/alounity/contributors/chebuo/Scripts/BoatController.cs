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
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        for(int i=0;i<boat.Length;i++) boat[i].action.Enable();
    }
    
    void FixedUpdate()
    {
        for(int i=0;i<wheels.Length;i++)
        {
            float input = boat[i].action.ReadValue<float>();

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
