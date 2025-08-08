using UnityEngine;

public class RifleShooter : MonoBehaviour
{
    [SerializeField] private Rifle _rifle;
    [SerializeField] private Animator _animator;

    private static readonly int ShootingParam = Animator.StringToHash("Shooting");

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            _rifle.StartShooting();
            _animator.SetBool(ShootingParam, true); // запуск анимации
        }

        if (Input.GetMouseButtonUp(0))
        {
            _rifle.StopShooting();
            _animator.SetBool(ShootingParam, false); // возврат к Sitting
        }
    }
}