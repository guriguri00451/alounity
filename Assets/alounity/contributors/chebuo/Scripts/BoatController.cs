using UnityEngine;
using UnityEngine.InputSystem;

public class BoatController : MonoBehaviour
{
    private Rigidbody rb;
    [SerializeField] Transform center;
    [SerializeField] WheelCollider[] wheels;
    [SerializeField] float power=100;
    [SerializeField] InputActionProperty boatL;
    [SerializeField] InputActionProperty boatR;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = this.GetComponent<Rigidbody>();
        boatL.action.Enable();
        boatR.action.Enable();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    void FixedUpdate()
    {
        if(boatL.action.ReadValue<float>()>0)wheels[0].motorTorque = power * boatL.action.ReadValue<float>();
        else wheels[0].motorTorque = 0;

        if(boatR.action.ReadValue<float>()>0)wheels[1].motorTorque = power * boatR.action.ReadValue<float>();
        else wheels[1].motorTorque = 0;
    }
}
