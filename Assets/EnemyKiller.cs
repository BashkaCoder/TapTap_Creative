using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EnemyKiller : MonoBehaviour
{
    [Header("Refs (optional, авто-поиск если пусто)")]
    [SerializeField] private GameObject _playerRoot;
    [SerializeField] private GameObject _enemyRoot;
    [SerializeField] private Health _playerHealth;
    [SerializeField] private Health _enemyHealth;

    [Header("Behaviour")]
    [SerializeField] private float _stopDelayAfterDeath = 0.35f; // подождать, чтобы успел отыграть «взрыв»

    private bool _stopped;

    private void Awake()
    {
        AutoWireIfNeeded();

        if (_playerHealth != null) _playerHealth.OnDied += OnAnyCarDied;
        if (_enemyHealth != null)  _enemyHealth.OnDied  += OnAnyCarDied;
    }

    private void OnDestroy()
    {
        if (_playerHealth != null) _playerHealth.OnDied -= OnAnyCarDied;
        if (_enemyHealth != null)  _enemyHealth.OnDied  -= OnAnyCarDied;
    }

    private void Update()
    {
        // R — перезапуск сцены
        if (Input.GetKeyDown(KeyCode.R))
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);

        // K — мгновенно убить врага
        if (Input.GetKeyDown(KeyCode.K))
            KillEnemyNow();
    }

    private void OnAnyCarDied()
    {
        if (_stopped) return;
        StartCoroutine(StopBothAfterDelay());
    }

    private IEnumerator StopBothAfterDelay()
    {
        _stopped = true;
        yield return new WaitForSeconds(_stopDelayAfterDeath);

        StopCar(_playerRoot);
        StopCar(_enemyRoot);
    }

    private void KillEnemyNow()
    {
        if (_enemyHealth == null || _enemyHealth.IsDead) return;
        // убиваем «в лоб» без множителей
        _enemyHealth.ApplyDamage(_enemyHealth.Max + 999f, ignoreMultiplier: true);
    }

    private static void StopCar(GameObject root)
    {
        if (!root) return;

        // Выключаем управление/логику
        var playerInput = root.GetComponent<PlayerInputController>();
        if (playerInput) playerInput.enabled = false;

        var npc = root.GetComponent<NpcCarPhysicsController>();
        if (npc) npc.enabled = false;

        var carPhysics = root.GetComponent<CarPhysicsController>();
        if (carPhysics) carPhysics.enabled = false;

        // Останавливаем стрельбу
        var shooter = root.GetComponentInChildren<ShooterController>(true);
        if (shooter)
        {
            shooter.ForceStopAndDisableShooting();
            shooter.enabled = false;
        }

        var rifles = root.GetComponentsInChildren<Rifle>(true);
        for (int i = 0; i < rifles.Length; i++)
        {
            rifles[i].StopShooting();
            rifles[i].enabled = false;
        }

        // ВАЖНО: не трогаем Rigidbody, если есть контроллер разрушения — он сам занимается дрифтом/фризом.
        if (root.GetComponent<CarDestructionController>()) return;

        // Fallback (если нет CarDestructionController): мягко остановим
        var rb = root.GetComponent<Rigidbody>();
        if (rb)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            // без экстремальных damping и без перевода в kinematic
        }
    }

    private void AutoWireIfNeeded()
    {
        if (_playerRoot == null)
        {
            var pic = FindObjectOfType<PlayerInputController>();
            if (pic) _playerRoot = pic.gameObject;
        }

        if (_enemyRoot == null)
        {
            var npc = FindObjectOfType<NpcCarPhysicsController>();
            if (npc) _enemyRoot = npc.gameObject;
        }

        if (_playerHealth == null && _playerRoot != null)
            _playerHealth = _playerRoot.GetComponentInChildren<Health>();

        if (_enemyHealth == null && _enemyRoot != null)
            _enemyHealth = _enemyRoot.GetComponentInChildren<Health>();
    }
}