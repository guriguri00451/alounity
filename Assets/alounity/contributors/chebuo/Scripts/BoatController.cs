using UnityEngine;
using UnityEngine.InputSystem;

public class BoatController : MonoBehaviour
{
    [SerializeField] Transform center;
    [SerializeField] WheelCollider[] wheels;
    [SerializeField] float power=100;
    [SerializeField] InputActionProperty[] boat;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        for(int i=0;i<boat.Length;i++) boat[i].action.Enable();
    }
    
    void FixedUpdate()
    {
        for(int i=0;i<wheels.Length;i++)
        {
            wheels[i].motorTorque = boat[i].action.ReadValue<float>()*power;
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
