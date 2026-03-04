using UnityEngine;

public class LookAtCamera : MonoBehaviour
{
    void LateUpdate()
    {
        // Направление от текста к камере
        Vector3 direction = Camera.main.transform.position - transform.position;
        // Поворачиваем текст так, чтобы его передняя сторона (ось Z) смотрела на камеру
        transform.rotation = Quaternion.LookRotation(-direction);
    }
}