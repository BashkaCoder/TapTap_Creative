using UnityEngine;
using UnityEngine.UI;

public class HealthView : MonoBehaviour
{
    [SerializeField] private Health _target;
    [SerializeField] private Slider _slider;
    [SerializeField] private Vector3 _offset = new Vector3(0, 2f, 0);

    private Camera _cam;

    private void Awake()
    {
        _cam = Camera.main;
        _slider.minValue = 0f;
        _slider.maxValue = 1f;
        _slider.value    = 1f;
        if (_target != null)
        {
            _target.OnChanged += (cur, max) => _slider.value = cur / max;
        }
    } 
}