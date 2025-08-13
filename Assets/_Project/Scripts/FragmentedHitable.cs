using System.Collections;
using UnityEngine;

public class FragmentedHitable : MonoBehaviour, IHitable
{
    [SerializeField] private Transform _wholeModel;
    [SerializeField] private Transform _slicedModel;
    [SerializeField] private LayerMask layerMask;
    [SerializeField] private Transform ParentForObstacles;
    
    [Header("Explosion Settings")]
    [SerializeField] private float _explosionForce = 1f;
    [SerializeField] private float _explosionRadius = 1f;
    [SerializeField] private float _upwardsModifier = 0.1f;
    [SerializeField] private float _delayBeforeLayerChange = 0.5f;
    
    public bool IsUsed { get; set; }

    public void Hit(GameObject hitter)
    {
        if (IsUsed) return;

        IsUsed = true;

        _wholeModel.gameObject.SetActive(false);
        _slicedModel.gameObject.SetActive(true);
        if (!hitter.TryGetComponent<Health>(out var health))
            return;
        
        health.ApplyDamage(0.2f, false);

        // гарантированно запуск из живого компонента
        StartCoroutineFromSafeHost();
    }   
    
    private void StartCoroutineFromSafeHost()
    {
        MonoBehaviour host = this; // если точно на активном объекте

        if (!this.isActiveAndEnabled)
        {
            // ищем кого-нибудь активного (например, sliced модель или родителя)
            host = _slicedModel.GetComponent<MonoBehaviour>();
            if (host == null)
            {
                host = _slicedModel.gameObject.AddComponent<DummyMono>();
            }
        }

        host.StartCoroutine(DelayedSetUsedLayers());
    }    
    
    private IEnumerator DelayedSetUsedLayers()
    {
        yield return new WaitForSeconds(_delayBeforeLayerChange);
        SetUsedLayers();
    }
    
    private void SetUsedLayers()
    {
        int layer = Mathf.RoundToInt(Mathf.Log(layerMask.value, 2));

        foreach (var col in ParentForObstacles.GetComponentsInChildren<Collider>(true))
            col.gameObject.layer = layer;

        foreach (var rb in ParentForObstacles.GetComponentsInChildren<Rigidbody>(true))
            rb.gameObject.layer = layer;
    }
    
    // fallback-монобех для запуска
    private class DummyMono : MonoBehaviour { }
}