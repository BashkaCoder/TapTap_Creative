using UnityEngine;

public class RifleShooter : MonoBehaviour
{
    [SerializeField] private Rifle _rifle;
    
    void Update()
    {
        if (Input.GetMouseButtonDown(0)) _rifle.StartShooting();
        if (Input.GetMouseButtonUp(0)) _rifle.StopShooting();
    }
}