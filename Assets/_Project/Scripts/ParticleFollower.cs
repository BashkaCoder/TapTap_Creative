using UnityEngine;

public class ParticleFollower : MonoBehaviour
{
    public Vector3 Offset = new(0, 0.35f, 0);
    public Transform target; 
    private Vector3 lastPosition;

    private void Update()
    {
        if(target == null)
            return;
        
        transform.position = target.position + Offset;
    }
}