using UnityEngine;

public class Health : MonoBehaviour, IHitable
{
    [Header("Health")]
    [SerializeField] private float _max = 100f;
    public float Max => _max;

    [Header("Damage (bullets)")]
    [SerializeField] private float _bulletDamage = 5f;

    [Header("Damage (collisions)")]
    [SerializeField] private float _energyToDamage = 0.02f;
    [SerializeField] private float _minRelativeSpeed = 1.5f;
    [SerializeField] private float _maxHitDamage = 50f;

    [Header("Incoming Damage Multiplier")]
    [SerializeField] private float _damageTakenMultiplier = 1f;

    public float Current { get; private set; }
    public bool IsDead => Current <= 0f;
    public bool IsUsed { get; set; }

    public System.Action<float, float> OnChanged;
    public System.Action OnDied;

    private Rigidbody _selfRb;

    private void Awake()
    {
        Current = _max;
        _selfRb = GetComponentInParent<Rigidbody>();
    }

    public void Hit(GameObject hitter)
    {
        if (IsDead || hitter == null) return;

        // 1) Столкновение (у источника есть Rigidbody) → считаем энергию, МНОЖИМ коэффициентом
        if (hitter.TryGetComponent<Rigidbody>(out var hitterRb))
        {
            Vector3 vSelf   = _selfRb ? _selfRb.linearVelocity   : Vector3.zero;
            Vector3 vHitter = hitterRb.linearVelocity;

            float relativeSpeed = (vSelf - vHitter).magnitude;
            if (relativeSpeed < _minRelativeSpeed) return;

            float m1 = _selfRb ? _selfRb.mass : 1000f;
            float m2 = hitterRb.mass;
            float mu = (m1 * m2) / Mathf.Max(m1 + m2, 0.0001f);

            float energy = 0.5f * mu * relativeSpeed * relativeSpeed;
            float damage = Mathf.Min(energy * _energyToDamage, _maxHitDamage);

            ApplyDamage(damage, ignoreMultiplier: false); // ← множим
        }
        else
        {
            // 2) Пуля/луч → фиксированный урон, БЕЗ множителя
            ApplyDamage(_bulletDamage, ignoreMultiplier: true); // ← НЕ множим
        }
    }

    public void Heal(float amount)
    {
        if (IsDead || amount <= 0f) return;
        Current = Mathf.Min(Current + amount, _max);
        OnChanged?.Invoke(Current, _max);
    }

    public void ApplyDamage(float rawDamage, bool ignoreMultiplier)
    {
        if (rawDamage <= 0f) return;

        float final = ignoreMultiplier ? rawDamage
                                       : rawDamage * Mathf.Max(_damageTakenMultiplier, 0f);

        Current -= final;
        OnChanged?.Invoke(Current, _max);

        if (Current <= 0f)
        {
            Current = 0f;
            OnDied?.Invoke();
        }
    }
}