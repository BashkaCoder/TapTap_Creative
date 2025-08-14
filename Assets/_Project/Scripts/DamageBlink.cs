using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class DamageBlink : MonoBehaviour
{
    [SerializeField] private Health _health;
    [SerializeField] private Image _image;

    [Header("Alpha Blink")]
    [SerializeField, Range(0f, 1f)] private float _targetAlpha = 0.6f;
    [SerializeField, Min(0.01f)] private float _fadeIn = 0.08f;
    [SerializeField, Min(0.01f)] private float _fadeOut = 0.25f;
    [SerializeField, Min(1)] private int _blinkCount = 1;

    [Header("Behaviour")]
    [SerializeField] private bool _startHidden = true;          // стартуем скрытыми
    [SerializeField] private bool _disableWhenHidden = true;    // выключать GO между миганиями

    private float _last;
    private Color _baseColor; // RGB сохраняем, альфой управляем
    private Tween _tween;     // <-- было Tweener

    private void Awake()
    {
        if (_health == null) _health = GetComponentInParent<Health>();
        if (_image == null)  _image  = GetComponentInChildren<Image>(true);

        if (_image != null)
        {
            _image.raycastTarget = false;
            _baseColor = _image.color;

            if (_startHidden)
            {
                SetAlpha(0f);
                if (_disableWhenHidden) _image.gameObject.SetActive(false);
            }
        }
    }

    private void OnEnable()
    {
        if (_health != null)
        {
            _last = _health.Current;
            _health.OnChanged += OnChanged;
        }
    }

    private void OnDisable()
    {
        if (_health != null) _health.OnChanged -= OnChanged;
        KillTween();
    }

    private void OnChanged(float cur, float max)
    {
        if (_image == null) return;

        if (cur < _last) // стало меньше — получен урон
            Blink();

        _last = cur;
    }

    // --- Публичные методы ---
    public void ShowInstant()
    {
        KillTween();
        EnsureActive();
        SetAlpha(_targetAlpha);
    }

    public void Hide()
    {
        if (_image == null) return;
        KillTween();
        _tween = _image.DOFade(0f, _fadeOut)
            .OnComplete(() =>
            {
                if (_disableWhenHidden) _image.gameObject.SetActive(false);
            });
    }

    public void Blink()
    {
        if (_image == null) return;

        KillTween();
        EnsureActive();

        // гарантируем правильные RGB (вдруг кто-то перекрасил)
        var c = _image.color; c.r = _baseColor.r; c.g = _baseColor.g; c.b = _baseColor.b; _image.color = c;

        var seq = DOTween.Sequence();
        for (int i = 0; i < _blinkCount; i++)
        {
            seq.Append(_image.DOFade(_targetAlpha, _fadeIn));
            seq.Append(_image.DOFade(0f, _fadeOut));
        }
        seq.OnComplete(() =>
        {
            if (_disableWhenHidden) _image.gameObject.SetActive(false);
        });

        _tween = seq; // <-- теперь ок, т.к. Tween базовый
    }

    // --- Вспомогательные ---
    private void EnsureActive()
    {
        if (_disableWhenHidden && !_image.gameObject.activeSelf)
            _image.gameObject.SetActive(true);
        if (!_image.enabled) _image.enabled = true;
    }

    private void SetAlpha(float a)
    {
        var col = _image.color;
        col.a = a;
        _image.color = col;
    }

    private void KillTween()
    {
        if (_tween != null && _tween.IsActive()) _tween.Kill();
        _tween = null;
    }
}