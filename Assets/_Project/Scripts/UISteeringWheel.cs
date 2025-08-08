using UnityEngine;
using UnityEngine.UI;

public class UISteeringWheel : MonoBehaviour
{
    [SerializeField] private RectTransform wheelTransform;
    [SerializeField] private float maxRotation = 90f;        // Максимальный угол поворота (влево/вправо)
    [SerializeField] private float rotationSpeed = 300f;     // Скорость вращения
    [SerializeField] private float returnSpeed = 200f;       // Скорость возврата в 0

    private float currentRotation = 0f;

    void Update()
    {
        float steerInput = Input.GetAxisRaw("Horizontal"); // -1, 0, 1

        if (steerInput != 0f)
        {
            // Поворачиваем руль
            currentRotation += steerInput * rotationSpeed * Time.deltaTime;
            currentRotation = Mathf.Clamp(currentRotation, -maxRotation, maxRotation);
        }
        else
        {
            // Плавное возвращение в 0
            currentRotation = Mathf.MoveTowards(currentRotation, 0f, returnSpeed * Time.deltaTime);
        }

        wheelTransform.localRotation = Quaternion.Euler(0, 0, -currentRotation);
    }
}