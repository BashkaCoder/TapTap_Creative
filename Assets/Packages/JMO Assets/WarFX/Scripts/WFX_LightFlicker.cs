using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Light))]
public class WFX_LightFlicker : MonoBehaviour
{
    public float time = 0.05f;

    private Coroutine _flickerCoroutine;
    private Light _light;
    private float _timer;

    private void Awake()
    {
        _light = GetComponent<Light>();
        _light.enabled = false;
    }

    public void StartFlicker()
    {
        if (_flickerCoroutine != null) return;
        _flickerCoroutine = StartCoroutine(Flicker());
    }

    public void StopFlicker()
    {
        if (_flickerCoroutine != null)
        {
            StopCoroutine(_flickerCoroutine);
            _flickerCoroutine = null;
        }

        if (_light != null)
            _light.enabled = false;
    }

    private IEnumerator Flicker()
    {
        while (true)
        {
            _light.enabled = !_light.enabled;

            _timer = time;
            while (_timer > 0)
            {
                _timer -= Time.deltaTime;
                yield return null;
            }
        }
    }
}
