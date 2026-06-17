using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class BoatController : MonoBehaviour
{
    [SerializeField] Transform center;
    [SerializeField] WheelCollider[] wheels;
    [SerializeField] InputActionProperty[] boat;
    [SerializeField] OarAnimSetter[] oarAnim;
    [Header("Parametor")]
    [SerializeField] float inputValueThreshold = 0.3f;
    [SerializeField] float power=100;
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

            if(input >= inputValueThreshold)
            {
                wheels[i].motorTorque = input*power;
                if(oarAnim.Length != 0) oarAnim[i].SetSpeed(input);
            }
            else
            {
                wheels[i].motorTorque = 0;
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
