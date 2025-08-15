using System.Collections;
using UnityEngine;

public class Rifle : MonoBehaviour
{
    [Header("Effects")]
    [SerializeField] private ParticleSystem _muzzleFlash;
    [SerializeField] private BulletTrail _bulletTrailPrefab;
    [SerializeField] private WFX_LightFlicker _muzzleLight;

    [Header("Shooting")]
    [SerializeField] private Transform _firePoint; // точка вылета пули
    [SerializeField] private float _shootDistance = 500f;
    [SerializeField] private LayerMask _hitMask;
    [SerializeField] private GameObject _bulletImpactEffectPrefab;
    [SerializeField] private GameObject _bulletHoleDecalPrefab;
    [SerializeField] private float _decalLifetime = 3.5f;
    
    [Header("Fire Rate")]
    [SerializeField] private float _minFireDelay = 0.07f;
    [SerializeField] private float _maxFireDelay = 0.12f;

    [Header("Spread")]
    [SerializeField] private float _spreadAngle = 3f; // в градусах
    
    private Coroutine _shootingCoroutine;

    public void StartShooting()
    {
        if (!isActiveAndEnabled) return;
        if (_shootingCoroutine != null) return;

        _muzzleFlash?.Play();
        _muzzleLight?.StartFlicker();
        _shootingCoroutine = StartCoroutine(ShootingRoutine());
    }

    public void StopShooting()
    {
        _muzzleFlash?.Stop();
        _muzzleLight?.StopFlicker();

        if (_shootingCoroutine != null)
        {
            StopCoroutine(_shootingCoroutine);
            _shootingCoroutine = null;
        }
    }

    private IEnumerator ShootingRoutine()
    {
        while (true)
        {
            FireOnce();
            float delay = Random.Range(_minFireDelay, _maxFireDelay);
            yield return new WaitForSeconds(delay);
        }
    }

    private void OnDisable()
    {
        // Если объект уходит в SetActive(false) / убивается — корутина и эффекты должны прекратиться
        StopShooting();
    }
    
    private void FireOnce()
    {
        Vector3 start = _firePoint.position;
        Vector3 baseDirection = _firePoint.forward;
        Vector3 spreadDirection = GetRandomDirectionInCone(baseDirection, _spreadAngle);
        Vector3 end = start + spreadDirection * _shootDistance;

        if (Physics.Raycast(start, spreadDirection, out RaycastHit hit, _shootDistance, _hitMask))
        {
            end = hit.point;

            // Эффекты
            if (_bulletImpactEffectPrefab)
            {
                var impact = Instantiate(_bulletImpactEffectPrefab, hit.point, Quaternion.LookRotation(hit.normal));
                Destroy(impact, 2f);
            }

            if (_bulletHoleDecalPrefab)
            {
                var decal = Instantiate(_bulletHoleDecalPrefab, hit.point + hit.normal * 0.01f, Quaternion.LookRotation(-hit.normal));
                decal.transform.SetParent(hit.collider.transform);
                Destroy(decal, _decalLifetime);
            }

            // Вызов Hit()
            if (hit.collider.TryGetComponent<IHitable>(out var hitable))
            {
                hitable.Hit(gameObject);
            }

            if (hit.collider.TryGetComponent<Rigidbody>(out var rb))
            {
                rb.useGravity = true;
            }
            
            // Маленький физический взрыв
            ApplyMicroExplosion(hit.point);
        }

        var trail = Instantiate(_bulletTrailPrefab);
        trail.Init(start, end);
    }
    
    private Vector3 GetRandomDirectionInCone(Vector3 forward, float angle)
    {
        float halfAngleRad = Mathf.Deg2Rad * angle * 0.5f;

        // Случайная точка на круге радиуса sin(θ)
        float randomRadius = Mathf.Tan(halfAngleRad);
        Vector2 randomPoint = Random.insideUnitCircle * randomRadius;

        // Построим отклонённый вектор
        Vector3 right = Vector3.Cross(Vector3.up, forward).normalized;
        if (right == Vector3.zero) right = Vector3.right; // подстраховка

        Vector3 up = Vector3.Cross(forward, right).normalized;
        Vector3 deviated = (forward + randomPoint.x * right + randomPoint.y * up).normalized;

        return deviated;
    }
    
    private void ApplyMicroExplosion(Vector3 center)
    {
        float radius = 0.25f; // небольшой радиус
        float force = 0.25f; // сила микро-взрыва
        float upwardsModifier = 0.1f;

        Collider[] colliders = Physics.OverlapSphere(center, radius);
        foreach (var collider in colliders)
        {
            if (collider.attachedRigidbody != null && !collider.attachedRigidbody.isKinematic)
            {
                collider.attachedRigidbody.AddExplosionForce(force, center, radius, upwardsModifier, ForceMode.Impulse);
            }
        }
    }
}