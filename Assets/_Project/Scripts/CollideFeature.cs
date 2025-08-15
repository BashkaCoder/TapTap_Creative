using UnityEngine;

public class CollideFeature : MonoBehaviour
{
    [SerializeField] private Health _health;
    
    private void OnCollisionEnter(Collision other)
    {
        if (TryFindHitable(other.collider.gameObject, out var hitable))
        {
            hitable.Hit(gameObject);
            _health.Hit(other.gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (TryFindHitable(other.gameObject, out var hitable))
        {
            hitable.Hit(gameObject);
        }
    }
    
    private static bool TryFindHitable(GameObject go, out IHitable hitable)
    {
        if (go.TryGetComponent(out hitable)) return true;
        hitable = go.GetComponentInParent<IHitable>();
        if (hitable != null) return true;
        hitable = go.GetComponentInChildren<IHitable>();
        return hitable != null;
    }
}