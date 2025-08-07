using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class NpcCarPhysicsController : MonoBehaviour
{
    public Transform[] waypoints;
    public float speed = 10f;
    public float rotationSpeed = 5f;
    public float waypointThreshold = 0.5f;

    private int currentWaypointIndex = 0;
    private Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.interpolation = RigidbodyInterpolation.Interpolate;
    }

    void FixedUpdate()
    {
        if (waypoints.Length == 0)
            return;

        Vector3 targetPos = waypoints[currentWaypointIndex].position;
        Vector3 direction = targetPos - transform.position;
        direction.y = 0;

        float distance = direction.magnitude;

        if (distance < waypointThreshold)
        {
            currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
            return;
        }

        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            Quaternion newRotation = Quaternion.Slerp(rb.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);
            rb.MoveRotation(newRotation);
        }

        Vector3 moveVector = transform.forward * speed * Time.fixedDeltaTime;
        rb.MovePosition(rb.position + moveVector);
    }
}