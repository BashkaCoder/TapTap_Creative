using UnityEngine;

namespace _Project.Scripts
{
    public class SimpleHitable : MonoBehaviour, IHitable
    {
        [SerializeField] private LayerMask layerMask;
        [SerializeField] private Transform ParentForObstacles;
        
        public bool IsUsed { get; set; }
        
        public void Hit(GameObject hitter)
        {
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
}