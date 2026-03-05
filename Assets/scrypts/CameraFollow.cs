using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform player;

    [Header("Camera Settings")]
    public float mouseSensitivity = 2f;
    public float distance = 5f;
    public float smoothSpeed = 8f;

    [Header("Target Offset (local to player)")]
    public Vector3 targetOffset = new Vector3(-1f, 1.5f, 0f);

    [Header("Lock-On")]
    public KeyCode lockOnKey = KeyCode.Q;
    public float lockOnSearchRadius = 15f;
    public LayerMask enemyLayer;
    public float smoothLockSpeed = 5f;
    public float headHeightOffset = 0.2f;     // дополнительное смещение вверх/вниз

    private float currentX = 0f;
    private float currentY = 15f;
    private const float Y_ANGLE_MIN = -20f;
    private const float Y_ANGLE_MAX = 60f;

    private bool lockOnEnabled = false;
    private Transform lockOnTarget;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
    if (Input.GetKeyDown(lockOnKey))
        ToggleLockOn();

    // Автоснятие лока
    if (lockOnEnabled)
    {
        // Если цель уничтожена или стала неактивной
        if (lockOnTarget == null || !lockOnTarget)
        {
            DisableLockOn();
            return;
        }

        bool isDead = false;

        // Проверка через компонент Enemy (свойство IsDead)
        var enemy = lockOnTarget.GetComponent<Enemy>();
        if (enemy != null)
        {
            isDead = enemy.IsDead; // предполагаем, что это публичное свойство
        }

        // Если не нашли компонент Enemy, проверяем коллайдер/активность (запасной вариант)
        if (!isDead)
        {
            Collider col = lockOnTarget.GetComponent<Collider>();
            if (col != null)
                isDead = !col.enabled;
            else
                isDead = !lockOnTarget.gameObject.activeInHierarchy;
        }

        float dist = Vector3.Distance(player.position, lockOnTarget.position);
        if (isDead || dist > lockOnSearchRadius)
        {
            DisableLockOn();
        }
    }
    }

    void LateUpdate()
    {
        if (player == null) return;

        skeleton playerSkeleton = player.GetComponent<skeleton>();
        if (playerSkeleton != null && playerSkeleton.IsDead)
            return;

        // Управление мышью (всегда активно)
        currentX += Input.GetAxis("Mouse X") * mouseSensitivity;
        currentY -= Input.GetAxis("Mouse Y") * mouseSensitivity;
        currentY = Mathf.Clamp(currentY, Y_ANGLE_MIN, Y_ANGLE_MAX);

        // Точка, на которую обычно смотрит камера (смещение от игрока)
        Vector3 lookAtPoint = player.position
                              + player.right * targetOffset.x
                              + player.up * targetOffset.y
                              + player.forward * targetOffset.z;

        // Позиция камеры (вращаем вокруг lookAtPoint)
        Quaternion rotation = Quaternion.Euler(currentY, currentX, 0);
        Vector3 desiredPosition = lookAtPoint + rotation * new Vector3(0, 0, -distance);

        // Плавное движение камеры
        transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);

        // Поворот камеры
        if (lockOnEnabled && lockOnTarget != null && lockOnTarget)
        {
            Vector3 targetHeadPos = GetTargetHeadPosition(lockOnTarget);
            Vector3 directionToTarget = targetHeadPos - transform.position;
            if (directionToTarget != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, smoothLockSpeed * Time.deltaTime);
            }
        }
        else
        {
            transform.LookAt(lookAtPoint);
        }
    }

    // Точное определение позиции головы врага
    private Vector3 GetTargetHeadPosition(Transform target)
    {
        Vector3 headPos = target.position + Vector3.up * 1.5f; // запасной вариант

        // 1) Ищем кости по стандартным именам
        string[] boneNames = { "Head", "HeadTarget", "Neck", "HeadJoint" };
        foreach (string name in boneNames)
        {
            Transform bone = FindDeepChild(target, name);
            if (bone != null)
            {
                headPos = bone.position;
                break;
            }
        }

        // Если нашли кость, используем её, иначе пытаемся использовать коллайдер
        if (headPos == target.position + Vector3.up * 1.5f) // всё ещё запасной вариант
        {
            Collider col = target.GetComponent<Collider>();
            if (col != null)
            {
                // Берём центр коллайдера и поднимаемся на 80% высоты (чтобы не улетать выше макушки)
                Bounds bounds = col.bounds;
                headPos = bounds.center + Vector3.up * bounds.extents.y * 0.8f;
            }
        }

        // Добавляем ручную корректировку (может быть отрицательной)
        headPos += Vector3.up * headHeightOffset;

        return headPos;
    }

    // Поиск дочернего объекта по имени (рекурсивно)
    private Transform FindDeepChild(Transform parent, string name)
    {
        foreach (Transform child in parent)
        {
            if (child.name == name)
                return child;
            Transform result = FindDeepChild(child, name);
            if (result != null)
                return result;
        }
        return null;
    }

    void ToggleLockOn()
    {
        if (lockOnEnabled)
            DisableLockOn();
        else
            TryLockOn();
    }

    void TryLockOn()
    {
    Collider[] enemies = Physics.OverlapSphere(player.position, lockOnSearchRadius, enemyLayer);
    if (enemies.Length > 0)
    {
        float closestDist = Mathf.Infinity;
        Transform closestEnemy = null;
        foreach (Collider col in enemies)
        {
            // Пропускаем мёртвых врагов
            var enemy = col.GetComponent<Enemy>();
            if (enemy != null && enemy.IsDead)
                continue;

            float dist = Vector3.Distance(player.position, col.transform.position);
            if (dist < closestDist)
            {
                closestDist = dist;
                closestEnemy = col.transform;
            }
        }

        if (closestEnemy != null)
        {
            lockOnTarget = closestEnemy;
            lockOnEnabled = true;

            skeleton playerSkeleton = player.GetComponent<skeleton>();
            if (playerSkeleton != null)
                playerSkeleton.SetLockOnTarget(lockOnTarget);

            Debug.Log("Lock-On: " + lockOnTarget.name);
        }
    }
    else
    {
        Debug.Log("No enemies nearby to lock on.");
    }
    }
    void DisableLockOn()
    {
        if (lockOnEnabled)
        {
            lockOnEnabled = false;

            skeleton playerSkeleton = player.GetComponent<skeleton>();
            if (playerSkeleton != null)
                playerSkeleton.ClearLockOn();

            lockOnTarget = null;
            Debug.Log("Lock-On disabled.");
        }
    }
}
  