using UnityEngine;

public class CollideFeature : MonoBehaviour
{
    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.TryGetComponent(out IHitable hitable))
        {
            hitable.Hit();
        }
    }
}