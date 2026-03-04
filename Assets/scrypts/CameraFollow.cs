using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform player;

    [Header("Camera Settings")]
    public float mouseSensitivity = 2f;
    public float distance = 5f;          // дистанция от камеры до точки взгляда
    public float smoothSpeed = 8f;

    [Header("Target Offset (local to player)")]
    public Vector3 targetOffset = new Vector3(-1f, 1.5f, 0f); // смещение точки взгляда влево и вверх

    private float currentX = 0f;
    private float currentY = 15f;
    private const float Y_ANGLE_MIN = -20f;
    private const float Y_ANGLE_MAX = 60f;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    void LateUpdate()
    {
        if (player == null) return;

        // Управление мышью
        currentX += Input.GetAxis("Mouse X") * mouseSensitivity;
        currentY -= Input.GetAxis("Mouse Y") * mouseSensitivity;
        currentY = Mathf.Clamp(currentY, Y_ANGLE_MIN, Y_ANGLE_MAX);

        // Точка, на которую смотрит камера (смещена влево и вверх от игрока)
        Vector3 lookAtPoint = player.position 
                              + player.right * targetOffset.x 
                              + player.up * targetOffset.y 
                              + player.forward * targetOffset.z;

        // Позиция камеры: она находится на расстоянии distance от lookAtPoint, 
        // в направлении, обратном взгляду (поворот от мыши)
        Quaternion rotation = Quaternion.Euler(currentY, currentX, 0);
        Vector3 desiredPosition = lookAtPoint + rotation * new Vector3(0, 0, -distance);

        // Плавное движение
        transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
        transform.LookAt(lookAtPoint);
    }
}