using UnityEngine;

public class Cart : MonoBehaviour, IHitable
{
    [SerializeField] private Transform[] _wheels;
    [SerializeField] private LayerMask layerMask;
    [SerializeField] private Transform ParentForObstacles;
    
    public bool IsUsed { get; set; }

    public void Hit(GameObject hitter)
    {
        if(IsUsed)
            return;
        
        foreach (var wheel in _wheels)
            Destroy(wheel.GetComponent<FixedJoint>());

        if (!hitter.TryGetComponent<Health>(out var health))
            return;
        
        health.ApplyDamage(0.2f, false);
        
        IsUsed = true;
        SetUsedLayers();
    }
    
    public void SetUsedLayers()
    {
        var layer = Mathf.RoundToInt(Mathf.Log(layerMask.value, 2)); 

        var colliders = ParentForObstacles.GetComponentsInChildren<Collider>(true);
        foreach (var collider in colliders)
        {
            collider.gameObject.layer = layer;
        }
        
        var rbs = ParentForObstacles.GetComponentsInChildren<Rigidbody>(true);
        foreach (var rb in rbs)
        {
            rb.gameObject.layer = layer;
        }
    }
}