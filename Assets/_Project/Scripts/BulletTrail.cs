using UnityEngine;

public class BulletTrail : MonoBehaviour
{
    [SerializeField] private float _speed = 100f;
    [SerializeField] private float _lifeTime = 0.1f;

    private Vector3 _direction;

    public void Init(Vector3 start, Vector3 end)
    {
        transform.position = start;
        _direction = (end - start).normalized;
        Destroy(gameObject, _lifeTime);
    }

    private void Update()
    {
        transform.position += _direction * _speed * Time.deltaTime;
    }
}