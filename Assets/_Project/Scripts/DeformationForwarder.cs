using UnityEngine;

public class DeformationForwarder : MonoBehaviour
{
    [SerializeField] private SimpleDeformOnCollision deformTarget;

    private void OnCollisionEnter(Collision collision)
    {
        deformTarget?.DeformAtPoint(collision.contacts[0].point, collision.relativeVelocity.magnitude);
    }
}