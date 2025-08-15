using UnityEngine;
using Unity.Cinemachine;

public class BillboardToCinemachine : MonoBehaviour
{
    private Camera _cam;

    private void Start()
    {
        var brain = Camera.main?.GetComponent<CinemachineBrain>();
        _cam = brain != null ? brain.OutputCamera : Camera.main;
    }

    private void LateUpdate()
    {
        if (_cam == null) return;

        // Поворачиваем в сторону камеры
        transform.forward = _cam.transform.forward;
        // Если нужно именно "смотреть" на камеру (без инверсии осей):
        // transform.LookAt(transform.position + _cam.transform.rotation * Vector3.forward,
        //                  _cam.transform.rotation * Vector3.up);
    }
}