using UnityEngine;

public class ParticleFollower : MonoBehaviour
{
    public Transform target; 
    private Vector3 lastPosition;

    private void Update()
    {
        if(target == null)
            return;
        
        transform.position = target.position;
    }
}