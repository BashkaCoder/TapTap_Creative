using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class CarPhysicsController : MonoBehaviour
{
    public float acceleration = 20f;
    public float maxSpeed = 20f;
    public float steeringSensitivity = 2f;
    public float maxSteerAngle = 25f;
    public float brakingDrag = 2f;

    public float alignmentSpeed = 5f; // Скорость выравнивания к целевому направлению
    public Vector3 globalForward = Vector3.forward; // Направление "прямо" в мировых координатах

    private Rigidbody rb;
    private float steerInput;
    private float throttleInput;
    private bool isSteering;

    public void SetInputs(float throttle, float steer)
    {
        throttleInput = Mathf.Clamp(throttle, -1f, 1f);
        steerInput = Mathf.Clamp(steer, -1f, 1f);
        isSteering = Mathf.Abs(steerInput) > 0.1f;
    }

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.centerOfMass += Vector3.down * 0.5f; // стабилизация
    }

    void FixedUpdate()
    {
        ApplyThrottle();
        ApplySteeringOrAlign();
    }

    void ApplyThrottle()
    {
        if (Mathf.Abs(throttleInput) > 0.1f)
        {
            Vector3 force = transform.forward * throttleInput * acceleration;

            // Мягкое ограничение скорости: чем быстрее, тем слабее прирост
            float speedFactor = 1f - Mathf.Clamp01(rb.linearVelocity.magnitude / maxSpeed);
            rb.AddForce(force * speedFactor, ForceMode.Acceleration);

            rb.linearDamping = 0f;
        }
        else
        {
            rb.linearDamping = brakingDrag;
        }
    }


    void ApplySteeringOrAlign()
    {
        if (isSteering)
        {
            float speedBasedSensitivity = Mathf.Lerp(0.5f, steeringSensitivity, rb.linearVelocity.magnitude / maxSpeed);
            float steerAngle = steerInput * maxSteerAngle;
            Quaternion steerRotation = Quaternion.Euler(0, steerAngle * speedBasedSensitivity * Time.fixedDeltaTime, 0);

            rb.MoveRotation(rb.rotation * steerRotation);
        }
        else
        {
            // Автовыравнивание по глобальному направлению (например, Vector3.forward)
            Vector3 flatForward = transform.forward;
            flatForward.y = 0f;

            Vector3 flatTarget = globalForward;
            flatTarget.y = 0f;

            if (flatForward.sqrMagnitude < 0.01f || flatTarget.sqrMagnitude < 0.01f)
                return;

            Quaternion targetRotation = Quaternion.LookRotation(flatTarget, Vector3.up);
            Quaternion newRotation = Quaternion.Slerp(rb.rotation, targetRotation, alignmentSpeed * Time.fixedDeltaTime);
            rb.MoveRotation(newRotation);
        }
    }
}
