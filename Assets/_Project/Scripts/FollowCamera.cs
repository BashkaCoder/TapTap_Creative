using UnityEngine;

public class FollowCamera : MonoBehaviour
{
    [SerializeField] private Transform target;        // Цель (машина)
    [SerializeField] private Vector3 offset = new Vector3(0f, 5f, -10f); // Смещение камеры
    [SerializeField] private float followSpeed = 5f;  // Скорость следования
    [SerializeField] private float rotateSpeed = 5f;  // Скорость поворота

    void LateUpdate()
    {
        if (target == null)
            return;

        // Желаемая позиция
        Vector3 desiredPosition = target.position + target.TransformDirection(offset);

        // Плавное следование
        transform.position = Vector3.Lerp(transform.position, desiredPosition, followSpeed * Time.deltaTime);

        // Плавный поворот к цели
        Quaternion desiredRotation = Quaternion.LookRotation(target.position - transform.position, Vector3.up);
        transform.rotation = Quaternion.Slerp(transform.rotation, desiredRotation, rotateSpeed * Time.deltaTime);
    }
}