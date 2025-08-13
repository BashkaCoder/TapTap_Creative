using System.Collections;
using UnityEngine;

public class GooseHitable : MonoBehaviour, IHitable
{
    [Header("VFX")]
    [SerializeField] private ParticleSystem _featherVfxPrefab;
    [SerializeField] private float _vfxScale = 1f;

    [Header("Physics")]
    [SerializeField] private Rigidbody _rb;              // корень гуся
    [SerializeField] private Collider[] _colliders;      // все коллайдеры гуся
    [SerializeField] private float _impulse = 6f;        // толчок от удара
    [SerializeField] private float _torque = 4f;         // кручение

    [Header("Animation")]
    [SerializeField] private Animator _animator;         // можно не задавать

    [Header("Collision")]
    [SerializeField] private float _ignoreSeconds = 2.0f; // сколько не сталкиваться с источником (0 = навсегда)
    
    private bool _alreadyHit;

    private void Reset()
    {
        _animator = GetComponent<Animator>();
        _rb = GetComponentInChildren<Rigidbody>();
        _colliders = GetComponentsInChildren<Collider>(true);
    }

    public bool IsUsed { get; set; }

    public void Hit(GameObject instigator)
    {
        if (_alreadyHit) return;
        _alreadyHit = true;

        // Останавливаем стаю, если есть
        var flockMover = GetComponentInParent<GooseFlockMover>();
        if (flockMover != null)
            flockMover.StopMovement();

        SpawnFeathers(instigator);

        if (_rb != null)
        {
            _rb.isKinematic = false;
            _rb.useGravity = true;

            var dir = (transform.position - instigator.transform.position).normalized;
            var v = instigator.TryGetComponent<Rigidbody>(out var irb) ? irb.linearVelocity : Vector3.zero;

            _rb.AddForce(dir * _impulse + v * 0.25f, ForceMode.VelocityChange);
            _rb.AddTorque(Random.onUnitSphere * _torque, ForceMode.VelocityChange);
        }

        if (_animator != null) _animator.enabled = false;

        IgnoreCollisionWith(instigator, _ignoreSeconds);
    }

    private void SpawnFeathers(GameObject instigator)
    {
        if (_featherVfxPrefab == null) return;

        // Точка контакта около ближней точки коллайдера к инстигатору
        var selfCol = _colliders != null && _colliders.Length > 0 ? _colliders[0] : GetComponentInChildren<Collider>();
        var pos = selfCol != null
            ? selfCol.ClosestPoint(instigator.transform.position)
            : transform.position + (transform.up * 0.3f);

        var vfx = Instantiate(_featherVfxPrefab, pos, Quaternion.identity);
        vfx.transform.localScale *= _vfxScale;
        vfx.Play();
        Destroy(vfx.gameObject, 3f);
    }

    private void IgnoreCollisionWith(GameObject instigator, float seconds)
    {
        if (_colliders == null || _colliders.Length == 0) _colliders = GetComponentsInChildren<Collider>(true);
        var otherCols = instigator.GetComponentsInChildren<Collider>(true);
        foreach (var a in _colliders)
            foreach (var b in otherCols)
                if (a && b) Physics.IgnoreCollision(a, b, true);

        if (seconds > 0)
            StartCoroutine(ReenableAfter(instigator, seconds));
    }

    private IEnumerator ReenableAfter(GameObject instigator, float seconds)
    {
        yield return new WaitForSeconds(seconds);
        var otherCols = instigator.GetComponentsInChildren<Collider>(true);
        foreach (var a in _colliders)
            foreach (var b in otherCols)
                if (a && b) Physics.IgnoreCollision(a, b, false);
    }
}