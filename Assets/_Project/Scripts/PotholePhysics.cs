using System.Collections;
using UnityEngine;

namespace _Project.Scripts
{
    public class PotholePhysics : MonoBehaviour, IHitable
    {
        [SerializeField] private float impulseForce = 5f;

        public bool IsUsed { get; set; }

        public void Hit(GameObject hitter)
        {
            if(IsUsed)
                return;
            
            if (!hitter.TryGetComponent<Rigidbody>(out var rb))
                return;
            
            if (!hitter.TryGetComponent<Health>(out var health))
                return;
            
            var point = hitter.transform.position + hitter.transform.forward * 1f;
            var forceDirection = Vector3.up * impulseForce;
            
            health.ApplyDamage(0.3f, false);
            
            rb.AddForceAtPosition(forceDirection, point, ForceMode.Impulse);
            IsUsed = true;

            StartCoroutine(nameof(ResetWithDelay));
        }

        private IEnumerator ResetWithDelay()
        {
            yield return new WaitForSeconds(0.5f);

            IsUsed = false;
        }
    }
}