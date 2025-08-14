using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class CarDestructionController : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private Health _health;
    [SerializeField] private GameObject _explosionFxPrefab; // частицы взрыва (без аудио)
    [SerializeField] private Material _destroyedMaterial;   // материал «чёрный металл»

    [Header("Drift (at death)")]
    [SerializeField] private float _driftSideImpulse = 800f; // боковой импульс
    [SerializeField] private float _driftTorqueY = 600f;     // разворот по Y (добавочный)
    [SerializeField] private bool _driftRight = true;        // направление заноса (право/лево)

    [Header("Explosion kick")]
    [SerializeField] private float _upImpulse = 10f;         // подскок вверх
    [SerializeField] private float _randomTorque = 200f;     // случайный крутящий момент

    [Header("Stop")]
    [SerializeField] private float _freezeDelay = 2.0f;      // через сколько заморозить
    [SerializeField] private bool _freezeKinematic = true;   // сделать rb.isKinematic = true

    [SerializeField] private ShooterController shooter;
    [SerializeField] private ParticleSystem _particleSystem;
    [SerializeField] private Transform _moneyParent;
    [SerializeField] private Material _dollarMaterial;

    private Rigidbody _rb;
    private bool _destroyed;

    private void Reset()
    {
        if (_health == null) _health = GetComponent<Health>();
        if (shooter == null) shooter = GetComponentInChildren<ShooterController>(true);
    }

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        if (shooter == null) shooter = GetComponentInChildren<ShooterController>(true);

        if (_health != null)
            _health.OnDied += OnDied;
    }

    private void OnDestroy()
    {
        if (_health != null)
            _health.OnDied -= OnDied;
    }

    private void OnDied()
    {
        if (_destroyed) return;
        _destroyed = true;

        // сразу отдаём объект под физику (важно для «смерти от пули»)
        _rb.isKinematic = false;

        DisableControl();

        // FX
        if (_explosionFxPrefab)
            Instantiate(_explosionFxPrefab, transform.position + Vector3.up * 0.8f, Quaternion.identity);
        if (_particleSystem != null) _particleSystem.Play();

        // Перекраска
        if (_destroyedMaterial) ReplaceAllMaterials(_destroyedMaterial);

        // Импульсы — строго в следующий физ.тик (после выключения контроллеров движения)
        StartCoroutine(ApplyDeathForcesNextFixed());

        // Фриз через задержку
        StartCoroutine(FreezeAfterDelay(_freezeDelay));

        // Деньги — один раз
        if (_moneyParent != null)
        {
            foreach (Transform child in _moneyParent)
            {
                if (child.TryGetComponent<BoxCollider>(out _))
                {
                    var r = child.GetComponent<MeshRenderer>();
                    if (r) r.sharedMaterial = _dollarMaterial;

                    var rbMoney = child.GetComponent<Rigidbody>();
                    if (rbMoney == null) rbMoney = child.gameObject.AddComponent<Rigidbody>();
                    rbMoney.useGravity = true;
                }
            }
        }
    }

    private IEnumerator ApplyDeathForcesNextFixed()
    {
        // дождаться следующего FixedUpdate, чтобы любые MovePosition/Rotation от контроллеров не стерли импульс
        yield return new WaitForFixedUpdate();

        _rb.isKinematic = false;
        _rb.constraints = RigidbodyConstraints.None; // чтобы точно крутило

        Vector3 side = _driftRight ? transform.right : -transform.right;
        _rb.AddForce(side * _driftSideImpulse, ForceMode.Impulse);

        Vector3 torqueDir = new Vector3(
            Random.Range(-0.5f, 0.5f) * _driftTorqueY,
            (_driftRight ? -1f : 1f) * _driftTorqueY,
            Random.Range(-0.5f, 0.5f) * _driftTorqueY
        );
        _rb.AddTorque(torqueDir, ForceMode.Impulse);

        _rb.AddForce(Vector3.up * _upImpulse, ForceMode.Impulse);
        _rb.AddTorque(Random.onUnitSphere * _randomTorque, ForceMode.Impulse);
    }

    private void DisableControl()
    {
        // управление машиной
        var playerInput = GetComponent<PlayerInputController>();
        if (playerInput) playerInput.enabled = false;

        var npc = GetComponent<NpcCarPhysicsController>();
        if (npc) npc.enabled = false;

        var playerPhysics = GetComponent<CarPhysicsController>();
        if (playerPhysics) playerPhysics.enabled = false;

        // --- СТРЕЛЬБА: многоступенчатый стоп ---
        if (shooter)
        {
            shooter.ForceStopAndDisableShooting(); // стоп + запрет
            shooter.enabled = false;               // на всякий
        }

        // Фоллбэк: вырубить все ружья в детях, даже если shooter не назначен
        var rifles = GetComponentsInChildren<Rifle>(true);
        for (int i = 0; i < rifles.Length; i++)
        {
            rifles[i].StopShooting();
            rifles[i].enabled = false;
        }
    }

    private void ReplaceAllMaterials(Material mat)
    {
        var rends = GetComponentsInChildren<MeshRenderer>(true);
        foreach (var r in rends)
        {
            var arr = r.materials;     // instance
            for (int i = 0; i < arr.Length; i++)
                arr[i] = mat;
            r.materials = arr;
        }
    }

    private IEnumerator FreezeAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        // обнуляем скорости — используем стандартные поля, если ваши расширения недоступны
        _rb.linearVelocity = Vector3.zero;
        _rb.angularVelocity = Vector3.zero;

        if (_freezeKinematic)
            _rb.isKinematic = true;
        // иначе можно было бы временно поднять damping, но обычно не нужно
    }
}
