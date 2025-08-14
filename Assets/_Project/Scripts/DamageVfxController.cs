using UnityEngine;
#if UNITY_VISUAL_EFFECT_GRAPH
using UnityEngine.VFX;
#endif

public class DamageVfxController : MonoBehaviour
{
    public enum DamageStage { None, LightSmoke, MediumSmoke, Fire }

    [Header("Source")]
    [SerializeField] private Health _health;

    [Header("VFX Groups (parents or individual objects)")]
    [SerializeField] private GameObject[] _lightSmoke;
    [SerializeField] private GameObject[] _mediumSmoke;
    [SerializeField] private GameObject[] _fire;

    [Header("Thresholds, damage percent [0..1]")]
    [Range(0f, 1f)] [SerializeField] private float _lightFrom  = 0.25f;
    [Range(0f, 1f)] [SerializeField] private float _mediumFrom = 0.50f;
    [Range(0f, 1f)] [SerializeField] private float _fireFrom   = 0.75f;

    private DamageStage _currentStage = DamageStage.None;

    private void Reset()
    {
        if (_health == null) _health = GetComponentInParent<Health>();
    }

    // Самое раннее место: вырубаем PlayOnAwake и стопаем всё, чтобы ничего не мигало на старте
    private void Awake()
    {
        if (_health == null) _health = GetComponentInParent<Health>();
        ForceDisablePlayOnAwake(_lightSmoke);
        ForceDisablePlayOnAwake(_mediumSmoke);
        ForceDisablePlayOnAwake(_fire);
        StopAllGroups(); // стоп + очистка
    }

    private void OnEnable()
    {
        if (_health == null) return;
        _health.OnChanged += HandleHealthChanged;
        _health.OnDied    += HandleDied;
    }

    private void Start()
    {
        if (_health == null) return;
        // корректная инициализация после того, как Health выставил Current в Awake
        HandleHealthChanged(_health.Current, _health.Max);
        Refresh();
    }

    private void OnDisable()
    {
        if (_health == null) return;
        _health.OnChanged -= HandleHealthChanged;
        _health.OnDied    -= HandleDied;
    }

    private void HandleHealthChanged(float current, float max)
    {
        float dmg = 1f - (current / Mathf.Max(max, 0.0001f)); // 0..1
        SetStage(StageFromDamage(dmg));
    }

    private void HandleDied() => SetStage(DamageStage.Fire);

    private DamageStage StageFromDamage(float d)
    {
        if (d >= _fireFrom)   return DamageStage.Fire;
        if (d >= _mediumFrom) return DamageStage.MediumSmoke;
        if (d >= _lightFrom)  return DamageStage.LightSmoke;
        return DamageStage.None;
    }

    private void SetStage(DamageStage stage)
    {
        if (_currentStage == stage) return;
        _currentStage = stage;
        Refresh();
    }

    private void Refresh()
    {
        PlayGroup(_lightSmoke,  _currentStage == DamageStage.LightSmoke);
        PlayGroup(_mediumSmoke, _currentStage == DamageStage.MediumSmoke);
        PlayGroup(_fire,        _currentStage == DamageStage.Fire);
    }

    private void StopAllGroups()
    {
        PlayGroup(_lightSmoke,  false);
        PlayGroup(_mediumSmoke, false);
        PlayGroup(_fire,        false);
    }

    private static void ForceDisablePlayOnAwake(GameObject[] group)
    {
        if (group == null) return;
        for (int i = 0; i < group.Length; i++)
        {
            var go = group[i];
            if (go == null) continue;

            var ps = go.GetComponentsInChildren<ParticleSystem>(true);
            for (int j = 0; j < ps.Length; j++)
            {
                var main = ps[j].main;
                main.playOnAwake = false;
                ps[j].Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                ps[j].Clear(true);
            }
            #if UNITY_VISUAL_EFFECT_GRAPH
            var vfx = go.GetComponentsInChildren<VisualEffect>(true);
            for (int j = 0; j < vfx.Length; j++)
            {
                vfx[j].playOnAwake = false;
                vfx[j].Stop();
            }
            #endif
        }
    }

    private static void PlayGroup(GameObject[] group, bool play)
    {
        if (group == null) return;
        for (int i = 0; i < group.Length; i++)
            PlayAllInHierarchy(group[i], play);
    }

    private static void PlayAllInHierarchy(GameObject root, bool play)
    {
        if (root == null) return;

        var psList = root.GetComponentsInChildren<ParticleSystem>(true);
        for (int i = 0; i < psList.Length; i++)
        {
            var ps = psList[i];
            if (play)
            {
                if (!ps.isPlaying) ps.Play(true);
            }
            else
            {
                if (ps.isPlaying) ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                ps.Clear(true);
            }
        }

        #if UNITY_VISUAL_EFFECT_GRAPH
        var vfxList = root.GetComponentsInChildren<VisualEffect>(true);
        for (int i = 0; i < vfxList.Length; i++)
        {
            var vfx = vfxList[i];
            if (play)
            {
                if (!vfx.alive) vfx.Play();
            }
            else
            {
                if (vfx.alive) vfx.Stop();
            }
        }
        #endif
    }
}