using System.Collections;
using UnityEngine;

public class ShooterController : MonoBehaviour
{
    [Header("General")]
    [SerializeField] private bool _isPlayer;

    [Header("Animation & Weapon")]
    [SerializeField] private Animator _animator;
    [SerializeField] private Rifle _rifle;

    [Header("Pose Transforms")]
    [SerializeField] private Transform _npcRootTransform;
    [SerializeField] private Transform _sittingPose;
    [SerializeField] private Transform _shootingPose;
    [SerializeField] private float _transitionDuration = 0.25f;

    [Header("Cooldown")]
    [SerializeField] private float _cooldownTime = 2f;

    [SerializeField] private AimingReticle reticle;
    [SerializeField] private Health _health;

    private static readonly int Shooting = Animator.StringToHash("Shooting");
    private Coroutine _moveRoutine;
    private Coroutine _cooldownRoutine;

    private bool _canShoot = true;
    private Health _ownerHealth;

    private void Awake()
    {
        if (_sittingPose != null && _npcRootTransform != null)
        {
            _npcRootTransform.position = _sittingPose.position;
            _npcRootTransform.rotation = _sittingPose.rotation;
        }

        // Автоподписка на смерть хоста
        _ownerHealth = GetComponentInParent<Health>();
        if (_ownerHealth != null)
            _ownerHealth.OnDied += OnOwnerDied;
    }

    private void Start()
    {
        if (_isPlayer)
            StartShooting(); // игрок начинает стрелять сразу\

        _health.OnDied += () =>
        {
            gameObject.SetActive(false);
            _npcRootTransform.gameObject.SetActive(false);
        };
    }

    private void Update()
    {
        if (_isPlayer || !_canShoot) return;
        
        if (Input.GetMouseButtonDown(0))
        {
            StartShooting();
            reticle.Begin(transform);   // когда враг «высунулся»
        }

        if (Input.GetMouseButtonUp(0))
        {
            StopShooting();
            reticle.StopAim();
        }
    }

    public void Hit(GameObject source)
    {
        if (!_isPlayer) return; // только игроку нужен кулдаун

        if (_cooldownRoutine != null)
            StopCoroutine(_cooldownRoutine);

        _cooldownRoutine = StartCoroutine(ShootingCooldown());
    }

    private void StartShooting()
    {
        if (!_canShoot) return;
        if (!isActiveAndEnabled) return;
        if (_rifle == null || _animator == null) return;

        _animator.SetBool(Shooting, true);
        _rifle.StartShooting();
        MoveToPose(_shootingPose);
    }

    private void StopShooting()
    {
        _animator?.SetBool(Shooting, false);
        _rifle?.StopShooting();
        MoveToPose(_sittingPose);
    }

    private IEnumerator ShootingCooldown()
    {
        StopShooting();
        _canShoot = false;

        yield return new WaitForSeconds(_cooldownTime);

        _canShoot = true;
        StartShooting(); // авто-возврат к стрельбе (если игрок)
    }

    private void MoveToPose(Transform targetPose)
    {
        if (_moveRoutine != null)
            StopCoroutine(_moveRoutine);

        _moveRoutine = StartCoroutine(MoveRoutine(targetPose));
    }

    private IEnumerator MoveRoutine(Transform target)
    {
        Vector3 startPos = _npcRootTransform.position;
        Quaternion startRot = _npcRootTransform.rotation;
        float elapsed = 0f;

        while (elapsed < _transitionDuration)
        {
            float t = elapsed / _transitionDuration;
            _npcRootTransform.position = Vector3.Lerp(startPos, target.position, t);
            _npcRootTransform.rotation = Quaternion.Slerp(startRot, target.rotation, t);
            elapsed += Time.deltaTime;
            yield return null;
        }

        _npcRootTransform.position = target.position;
        _npcRootTransform.rotation = target.rotation;
    }
    
    public void ForceStopAndDisableShooting()
    {
        // Полностью останавливаем и запрещаем
        if (_cooldownRoutine != null)
        {
            StopCoroutine(_cooldownRoutine);
            _cooldownRoutine = null;
        }

        _canShoot = false;         // запрет новых стартов
        _animator.SetBool(Shooting, false);

        if (_rifle != null)
            _rifle.StopShooting();

        // Возвращаем позу сидя (если задана)
        if (_npcRootTransform != null && _sittingPose != null)
            MoveToPose(_sittingPose);
    }

    private void OnDestroy()
    {
        if (_ownerHealth != null)
            _ownerHealth.OnDied -= OnOwnerDied;
    }
    
    private void OnDisable()
    {
        // Если объект/компонент выключают — стрельба обязана остановиться
        _canShoot = false;
        _animator?.SetBool(Shooting, false);
        _rifle?.StopShooting();

        if (_moveRoutine != null)
        {
            StopCoroutine(_moveRoutine);
            _moveRoutine = null;
        }
        if (_cooldownRoutine != null)
        {
            StopCoroutine(_cooldownRoutine);
            _cooldownRoutine = null;
        }
    }
    
    private void OnOwnerDied()
    {
        // Однокнопочно: стоп, запрет, и сам компонент отключить
        ForceStopAndDisableShooting();
        enabled = false;
    }
    
    public void EnableShooting()
    {
        _canShoot = true;
    }
    
    public void HandleSelfCollision()
    {
        if (!_isPlayer) return; // только игрок реагирует
        if (_cooldownRoutine != null)
            StopCoroutine(_cooldownRoutine);

        _cooldownRoutine = StartCoroutine(ShootingCooldown());
    }
}