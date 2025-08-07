using UnityEngine;

public class PlayerInputController : MonoBehaviour
{
    private CarPhysicsController car;

    void Awake()
    {
        car = GetComponent<CarPhysicsController>();
    }

    void Update()
    {
        float throttle = Input.GetAxis("Vertical");
        float steer = Input.GetAxis("Horizontal");
        car.SetInputs(throttle, steer);
    }
}