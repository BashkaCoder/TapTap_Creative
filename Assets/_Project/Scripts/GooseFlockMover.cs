using UnityEngine;

public class GooseFlockMover : MonoBehaviour
{
    [Header("Who moves")]
    [SerializeField] private Transform _flockRoot;          // корень стаи (его двигаем/поворачиваем)

    [Header("Path")]
    [SerializeField] private Transform[] _waypoints;        // минимум 2 точки
    [SerializeField] private bool _pingPong = true;         // туда‑обратно (иначе по кругу)

    [Header("Motion")]
    [SerializeField] private float _speed = 1.5f;
    [SerializeField] private float _reachDistance = 0.25f;  // на каком расстоянии считаем, что пришли
    [SerializeField] private float _turnSpeed = 5f;         // скорость разворота к цели

    private int _index;
    private bool _forward = true;

    private bool _isStopped;

    public void StopMovement()
    {
        _isStopped = true;
    }
    
    private void Reset()
    {
        _flockRoot = transform; // по умолчанию двигаем сам объект
    }

    private void Update()
    {
        if (_isStopped) return; // движение отключено

        if (_flockRoot == null || _waypoints == null || _waypoints.Length == 0) 
            return;

        var target = _waypoints[_index];
        var pos = _flockRoot.position;

        Vector3 to = target.position - pos;
        to.y = 0f;

        if (to.sqrMagnitude > 0.0001f)
        {
            var look = Quaternion.LookRotation(to.normalized, Vector3.up);
            _flockRoot.rotation = Quaternion.Slerp(_flockRoot.rotation, look, Time.deltaTime * _turnSpeed);
            _flockRoot.position += to.normalized * (_speed * Time.deltaTime);
        }

        if (Vector3.Distance(new Vector3(pos.x, 0, pos.z), new Vector3(target.position.x, 0, target.position.z)) <= _reachDistance)
            AdvanceIndex();
    }

    private void AdvanceIndex()
    {
        if (_pingPong)
        {
            if (_forward)
            {
                _index++;
                if (_index >= _waypoints.Length)
                {
                    _index = Mathf.Max(0, _waypoints.Length - 2);
                    _forward = false;
                }
            }
            else
            {
                _index--;
                if (_index < 0)
                {
                    _index = _waypoints.Length > 1 ? 1 : 0;
                    _forward = true;
                }
            }
        }
        else
        {
            _index = (_index + 1) % _waypoints.Length;
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (_waypoints == null || _waypoints.Length < 2) return;
        Gizmos.color = Color.yellow;
        for (int i = 0; i < _waypoints.Length - 1; i++)
            Gizmos.DrawLine(_waypoints[i].position, _waypoints[i + 1].position);

        if (_flockRoot != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(_flockRoot.position, 0.25f);
        }
    }
}