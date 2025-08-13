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

        // 1) Выключаем управление (и у игрока, и у NPC, если есть)
        DisableControl();

        if (_moneyParent != null)
        {
            foreach (Transform child in _moneyParent)
            {
                if (child.GetComponent<BoxCollider>() != null)
                {
                    child.GetComponent<MeshRenderer>().sharedMaterial = _dollarMaterial;
                    var rbMoney = child.gameObject.AddComponent<Rigidbody>();
                    rbMoney.useGravity = true;
                }
            }
        }
        
        // 2) «Занос» — боковой импульс
        Vector3 side = _driftRight ? transform.right : -transform.right;
        _rb.AddForce(side * _driftSideImpulse, ForceMode.Impulse);

        // Крутящий момент по двум-трём осям для эффектного переворота
        Vector3 torqueDir = new Vector3(
            Random.Range(-0.5f, 0.5f) * _driftTorqueY,   // X
            (_driftRight ? -1f : 1f) * _driftTorqueY,    // Y
            Random.Range(-0.5f, 0.5f) * _driftTorqueY    // Z
        );
        _rb.AddTorque(torqueDir, ForceMode.Impulse);

        // 3) Взрывной FX (частицы). Без звука.
        if (_explosionFxPrefab)
            Instantiate(_explosionFxPrefab, transform.position + Vector3.up * 0.8f, Quaternion.identity);

        // 4) Подброс и доп. вращение
        _rb.AddForce(Vector3.up * _upImpulse, ForceMode.Impulse);
        _rb.AddTorque(Random.onUnitSphere * _randomTorque, ForceMode.Impulse);

        // 5) Перекраска всех материалов в «чёрный металл»
        if (_destroyedMaterial) ReplaceAllMaterials(_destroyedMaterial);

        // 6) Через задержку — полная остановка
        StartCoroutine(FreezeAfterDelay(_freezeDelay));
        
        //7) Разлет денег 
        if (_particleSystem != null) _particleSystem.Play();
        
        if( _moneyParent == null) return;
        foreach (Transform child in _moneyParent)
        {
            if (child.GetComponent<BoxCollider>() != null)
            {
                child.GetComponent<MeshRenderer>().sharedMaterial = _dollarMaterial;
                child.GetComponent<Rigidbody>().useGravity = true;
            }
        }
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

        _rb.linearVelocity = Vector3.zero;
        _rb.angularVelocity = Vector3.zero;

        if (_freezeKinematic)
        {
            _rb.isKinematic = true;
        }
        else
        {
            //_rb.linearDamping = 1000f;      // альтернативный «жёсткий стоп», если kinematic не хочется
            //_rb.angularDamping = 1000f;
        }
    }
}
