using System;
using DG.Tweening;
using UnityEngine;

public class AimingReticle : MonoBehaviour
{
    [Header("Visuals")]
    [SerializeField] private SpriteRenderer _sr;
    [SerializeField] private float _billboardLerp = 20f;

    [Header("Intro/Outro")]
    [SerializeField] private float _introDuration = 0.22f;
    [SerializeField] private Ease  _introEase = Ease.OutBack;
    [SerializeField] private float _outroDuration = 0.18f;
    [SerializeField] private Ease  _outroEase = Ease.InBack;

    [Header("Drift around player")]
    [SerializeField] private float _height = 0.05f;              // приподнять над землей
    [SerializeField] private float _startAhead = 2.0f;            // старт перед врагом
    [SerializeField] private float _driftRadius = 0.6f;           // радиус зоны дрейфа вокруг цели
    [SerializeField] private Vector2 _driftHopDuration = new(0.18f, 0.35f);
    [SerializeField] private Ease _driftEase = Ease.InOutSine;

    [Header("Pulse (shots feel)")]
    [SerializeField] private Vector2 _pulseDuration = new(0.08f, 0.14f);
    [SerializeField] private Vector2 _pulseStrength = new(0.08f, 0.18f); // скейл‑панч 0..X
    [SerializeField] private int _pulseVibrato = 10;
    [SerializeField] private float _pulseElasticity = 0.7f;
    [SerializeField] private Vector2 _pulseInterval = new(0.05f, 0.18f); // пауза между «выстрелами»

    [Header("Target on player (local)")]
    [Tooltip("Локальная точка на игроке, возле которой крутимся.")]
    [SerializeField] private Vector3 _targetLocalOffset = Vector3.zero;

    [Header("Debug control (optional)")]
    [SerializeField] private bool _debugUseMouse = true;   // ЛКМ — начать, отпуск — остановить
    [SerializeField] private Transform _debugEnemy;

    public Action OnLocked; // не используется в этом флоу, оставил чтобы не ломать внешние подписки

    private Vector3 _initScale;
    private Camera _cam;
    private Transform _enemy;

    // Твины/состояние
    private Sequence _introSeq;
    private Tween _fadeTween;
    private Tween _driftTween;
    private Sequence _pulseSeq;
    private bool _active;

    private void Awake()
    {
        if (_sr == null) _sr = GetComponentInChildren<SpriteRenderer>(true);
        _cam = Camera.main;

        _initScale = transform.localScale;
        transform.localScale = Vector3.zero;

        if (_sr != null)
        {
            var c = _sr.color;
            c.a = 0f;
            _sr.color = c;
            _sr.enabled = false;
        }
    }

    private void Update()
    {
        if (!_debugUseMouse) return;

        if (Input.GetMouseButtonDown(0))
            Begin(_debugEnemy);

        if (Input.GetMouseButtonUp(0))
            StopAim();
    }

    private void LateUpdate()
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

    /// <summary>Запустить: появление → вечный пульс + дрейф вокруг точки у игрока.</summary>
    public void Begin(Transform enemy)
    {
        _enemy = enemy;
        if (_enemy == null || _sr == null) return;

        KillAllTweens();

        // Стартовая позиция: перед врагом и прижать к земле
        Vector3 start = _enemy.position + _enemy.forward * _startAhead;
        start.y = GetGroundY(start) + _height;

        transform.position = start;
        transform.localScale = Vector3.zero;

        // подготовить альфу и включить визуал
        var c = _sr.color;
        c.a = 0f;
        _sr.color = c;
        _sr.enabled = true;

        _active = true;

        // Интро: фейд‑ин + скейл‑ап
        _fadeTween = _sr.DOFade(1f, _introDuration).SetEase(Ease.Linear);
        _introSeq = DOTween.Sequence()
            .Append(transform.DOScale(_initScale, _introDuration).SetEase(_introEase))
            .OnComplete(() =>
            {
                StartPulseLoop();
                StartDriftLoop();
            });
    }

    /// <summary>Плавно скрыть и остановить всё.</summary>
    public void StopAim()
    {
        if (!_active) return;
        _active = false;

        // Остановить циклы
        _pulseSeq?.Kill();
        _driftTween?.Kill();

        // Аутро: фейд‑аут + скейл‑даун
        _fadeTween?.Kill();
        var fade = _sr != null ? _sr.DOFade(0f, _outroDuration).SetEase(Ease.Linear) : null;

        transform.DOKill();
        var scale = transform.DOScale(0f, _outroDuration).SetEase(_outroEase);

        // выключить спрайт после скрытия
        if (fade != null)
            fade.OnComplete(() => { if (_sr != null) _sr.enabled = false; });
        else
            scale.OnComplete(() => { if (_sr != null) _sr.enabled = false; });
    }

    /// <summary>Мгновенно уничтожить прицел.</summary>
    public void KillNow()
    {
        KillAllTweens();
        Destroy(gameObject);
    }

    // ----- Internal loops -----

    private void StartPulseLoop()
    {
        // Бесконечная «стрельба»: резкий панч по скейлу с рандомной силой + интервалы
        _pulseSeq = DOTween.Sequence().SetUpdate(false).SetAutoKill(false);

        void EnqueuePulse()
        {
            if (!_active) return;

            float dur = UnityEngine.Random.Range(_pulseDuration.x, _pulseDuration.y);
            float str = UnityEngine.Random.Range(_pulseStrength.x, _pulseStrength.y);

            // Небольшая рандомизация по осям — ощущение «нервного прицела»
            var punch = new Vector3(str * UnityEngine.Random.Range(0.9f, 1.1f),
                                    str * UnityEngine.Random.Range(0.9f, 1.1f),
                                    0f);

            _pulseSeq.Append(transform.DOPunchScale(punch, dur, _pulseVibrato, _pulseElasticity));

            float wait = UnityEngine.Random.Range(_pulseInterval.x, _pulseInterval.y);
            _pulseSeq.AppendInterval(wait);

            _pulseSeq.AppendCallback(EnqueuePulse);
        }

        EnqueuePulse();
        _pulseSeq.Play();
    }

    private void StartDriftLoop()
    {
        // Зацикленный дрейф от точки-«якоря» у игрока к случайным позициям в радиусе
        DoNextHop();

        void DoNextHop()
        {
            if (!_active) return;

            Vector3 anchor = GetTargetAnchor();
            Vector2 rnd = UnityEngine.Random.insideUnitCircle * _driftRadius;
            Vector3 target = new Vector3(anchor.x + rnd.x, 0f, anchor.z + rnd.y);
            target.y = GetGroundY(target) + _height;

            float hopDur = UnityEngine.Random.Range(_driftHopDuration.x, _driftHopDuration.y);

            _driftTween = transform.DOMove(target, hopDur)
                .SetEase(_driftEase)
                .OnComplete(DoNextHop);
        }
    }

    // ----- Utils -----

    private Vector3 GetTargetAnchor()
    {
        if (transform.parent == null)
            return transform.position;

        var world = transform.parent.TransformPoint(_targetLocalOffset);
        world.y = GetGroundY(world) + _height;
        return world;
    }

    private float GetGroundY(Vector3 near)
    {
        if (Physics.Raycast(near + Vector3.up * 5f, Vector3.down, out var hit, 20f, ~0, QueryTriggerInteraction.Ignore))
            return hit.point.y;
        return near.y;
    }

    private void KillAllTweens()
    {
        _introSeq?.Kill();
        _pulseSeq?.Kill();
        _driftTween?.Kill();
        _fadeTween?.Kill();
        transform.DOKill();
    }
}