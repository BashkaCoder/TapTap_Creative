using System;
using System.Collections;
using UnityEngine;

namespace _Project.Scripts
{
    public class SimpleHitable : MonoBehaviour, IHitable
    {
        [SerializeField] private LayerMask layerMask;
        [SerializeField] private Transform ParentForObstacles;
        [SerializeField] private float _delayBeforeApply = 1.5f; // задержка в секундах
        
        [Header("Explosion Settings")]
        [SerializeField] private float _explosionForce = 2f;
        [SerializeField] private float _explosionRadius = 1f;
        [SerializeField] private float _upwardsModifier = 0.3f;

        [SerializeField] private Rigidbody[] _rbs;
        
        public bool IsUsed { get; set; }

        private void Awake()
        {
            foreach (var rb in _rbs)
            {
                rb.useGravity = false;
            }
        }

        public void Hit(GameObject hitter)
        {
            foreach (var rb in _rbs)
            {
                rb.useGravity = true;
            }
            if (IsUsed) return;

            if (!hitter.TryGetComponent<Health>(out var health))
                return;
        
            health.ApplyDamage(0.1f, false);
            
            IsUsed = true;
            StartCoroutine(DelayedSetUsedLayers());
        }
        
        private IEnumerator DelayedSetUsedLayers()
        {
            //ExplosionUtility.ApplyExplosionForceToChildren(ParentForObstacles, _explosionForce, _explosionRadius, _upwardsModifier);
            yield return new WaitForSeconds(_delayBeforeApply);
            SetUsedLayers();
        }
        
        public void SetUsedLayers()
        {
            int layer = Mathf.RoundToInt(Mathf.Log(layerMask.value, 2));

            foreach (var col in ParentForObstacles.GetComponentsInChildren<Collider>(true))
                col.gameObject.layer = layer;

            foreach (var rb in ParentForObstacles.GetComponentsInChildren<Rigidbody>(true))
                rb.gameObject.layer = layer;
        }

        private void OnCollisionEnter(Collision other)
        {
            foreach (var rb in _rbs)
            {
                rb.useGravity = true;
            }
        }
    }
}