using System;
using DG.Tweening;
using UnityEngine;

public class AimingReticle : MonoBehaviour
{
    [Header("Visuals")]
    [SerializeField] private SpriteRenderer _sr;
    [SerializeField] private float _billboardLerp = 20f; // скорость разворота к камере

    [Header("Motion")]
    [SerializeField] private float _height = 0.05f;        // чуть приподнять над дорогой
    [SerializeField] private float _startAhead = 2.0f;      // старт перед врагом
    [SerializeField] private float _driftDuration = 1.25f;  // время дрейфа
    [SerializeField] private Ease  _driftEase = Ease.OutSine;

    [Header("Finish punch")]
    [SerializeField] private Vector3 _lockPunch = new(0.15f, 0.15f, 0.15f);

    [Header("Target on player (local)")]
    [Tooltip("Локальная точка на игроке, куда должен приехать прицел. (0,0,0) — позиция этого объекта.")]
    [SerializeField] private Vector3 _targetLocalOffset = Vector3.zero;

    [Header("Debug control (optional)")]
    [SerializeField] private bool _debugUseMouse = true;   // ЛКМ — начать, отпуск — остановить
    [SerializeField] private Transform _debugEnemy;        // враг для теста в редакторе

    public Action OnLocked;

    private Vector3 _initScale;
    
    private Camera _cam;
    private Sequence _seq;
    private Transform _enemy;   // источник старта дрейфа (враг)

    void Awake()
    {
        if (_sr == null) _sr = GetComponentInChildren<SpriteRenderer>(true);
        _cam = Camera.main;
        if (_sr != null) _sr.enabled = false; // по умолчанию скрыт
        _initScale = transform.localScale;
        transform.localScale = Vector3.zero;
    }

    void Update()
    {
        if (!_debugUseMouse) return;

        if (Input.GetMouseButtonDown(0))
            Begin(_debugEnemy);

        if (Input.GetMouseButtonUp(0))
            StopAim();
    }

    void LateUpdate()
    {
        if (_cam == null || _sr == null || !_sr.enabled) return;

        // Биллборд к камере (без завала по вертикали)
        var toCam = (_cam.transform.position - transform.position);
        toCam.y = 0f;
        if (toCam.sqrMagnitude > 0.0001f)
        {
            var targetRot = Quaternion.LookRotation(-toCam.normalized, Vector3.up);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRot, Time.deltaTime * _billboardLerp);
        }
    }

    /// <summary>Запустить наведение: спавн из позиции врага → дрейф к локальной точке на игроке.</summary>
    public void Begin(Transform enemy)
    {
        _enemy = enemy;
        if (_enemy == null)
            return;

        // Стартовая точка — перед врагом + прижать к земле
        Vector3 start = _enemy.position + _enemy.forward * _startAhead;
        start.y = GetGroundY(start) + _height;

        // Целевая точка — локальная позиция на игроке (родителе этого объекта)
        Vector3 end = transform.parent != null
            ? transform.parent.TransformPoint(_targetLocalOffset)
            : transform.position; // если нет родителя — остаёмся на месте

        // Включаем визуал, ставим в старт
        if (_sr != null) _sr.enabled = true;
        transform.position = start;

        // Киллим предыдущую анимацию
        _seq?.Kill();
        _seq = DOTween.Sequence();

        // Появление + дрейф + лёгкий панч в конце
        transform.localScale = Vector3.zero;
        _seq.Append(transform.DOScale(_initScale , 0.2f).SetEase(Ease.OutBack))
            .Append(transform.DOMove(end, _driftDuration).SetEase(_driftEase))
            .AppendCallback(() =>
            {
                transform.DOPunchScale(_lockPunch, 0.25f, 6, 0.6f);
                OnLocked?.Invoke();
            });
    }

    /// <summary>Остановить и спрятать прицел.</summary>
    public void StopAim()
    {
        _seq?.Kill();
        _seq = null;

        // Быстрый «схлоп» и выключение
        transform.DOKill();
        transform.DOScale(0f, 0.12f).OnComplete(() =>
        {
            if (_sr != null) _sr.enabled = false;
        });
    }

    /// <summary>Мгновенно убить объект прицела.</summary>
    public void KillNow()
    {
        _seq?.Kill();
        Destroy(gameObject);
    }

    // --- utils ---
    private float GetGroundY(Vector3 near)
    {
        if (Physics.Raycast(near + Vector3.up * 5f, Vector3.down, out var hit, 20f, ~0, QueryTriggerInteraction.Ignore))
            return hit.point.y;
        return near.y;
    }
}
