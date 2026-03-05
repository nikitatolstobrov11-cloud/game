using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    [Header("Target")]
    public Transform player;                // цель (скелет)

    [Header("Movement")]
    public float detectionRange = 10f;       // дистанция обнаружения
    public float attackRange = 2f;           // дистанция атаки
    public float moveSpeed = 2f;             // скорость передвижения
    public float rotationSpeed = 5f;         // скорость поворота к игроку

    [Header("Attack")]
    public float attackCooldown = 1.5f;      // перезарядка атаки
    public int damage = 10;                   // урон за удар

    [Header("Shield Block")]
    [Range(0, 180)]
    public float blockAngle = 45f;            // угол, в котором игрок может заблокировать удар

    private Animator animator;
    private float lastAttackTime;
    private bool isAttacking = false;
    private Enemy enemy;                     // ссылка на компонент врага (для проверки смерти)
    private skeleton playerSkeleton;          // ссылка на компонент игрока

    void Start()
    {
    animator = GetComponent<Animator>();
    enemy = GetComponent<Enemy>();

    if (player == null)
        player = GameObject.FindGameObjectWithTag("Player")?.transform;

    if (player != null)
        playerSkeleton = player.GetComponent<skeleton>();

    // Добавить эту строку:
    if (enemy != null)
        enemy.playerRef = player;
    }

    void Update()
    {
        // Если враг мёртв – ничего не делаем
        if (enemy != null && enemy.IsDead) return;

        // Если игрок мёртв – останавливаемся
        if (playerSkeleton != null && playerSkeleton.IsDead)
        {
            animator.SetFloat("Speed", 0);
            return;
        }

        if (player == null) return;

        float distance = Vector3.Distance(transform.position, player.position);

        // Поворачиваемся к игроку
        Vector3 direction = (player.position - transform.position).normalized;
        direction.y = 0;
        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
        }

        // Логика состояний
        if (distance <= attackRange && Time.time > lastAttackTime + attackCooldown)
        {
            Attack();
        }
        else if (distance <= detectionRange && distance > attackRange)
        {
            MoveTowardsPlayer(direction);
        }
        else
        {
            animator.SetFloat("Speed", 0);
        }
    }

    void MoveTowardsPlayer(Vector3 direction)
    {
        transform.position += direction * moveSpeed * Time.deltaTime;
        animator.SetFloat("Speed", moveSpeed);
    }

    void Attack()
    {
        if (isAttacking) return;
        isAttacking = true;
        animator.SetTrigger("Attack");
        lastAttackTime = Time.time;
        Invoke(nameof(ResetAttack), attackCooldown);
    }

    void ResetAttack()
    {
        isAttacking = false;
    }

    public void DealDamageToPlayer()
    {
        if (enemy != null && enemy.IsDead) return;
        if (playerSkeleton == null || playerSkeleton.IsDead) return;

        float distance = Vector3.Distance(transform.position, player.position);
        if (distance <= attackRange + 0.5f)
        {
            if (playerSkeleton.IsBlocking() && playerSkeleton.hasShield && IsPlayerFacingMe())
            {
                Debug.Log("🛡️ Удар заблокирован щитом!");

                // Визуально-звуковая обратная связь
                playerSkeleton.PlayBlockSound();
                playerSkeleton.PlayBlockAnimation();
                playerSkeleton.PlayBlockEffect();

                // Урон не наносится (нет вызова TakeDamage)
            }
            else
            {
                playerSkeleton.TakeDamage(damage);
                Debug.Log($"⚔️ Враг нанёс {damage} урона");
            }
        }
    }

    private bool IsPlayerFacingMe()
    {
        Vector3 directionToEnemy = (transform.position - player.position).normalized;
        directionToEnemy.y = 0;

        Vector3 playerForward = player.forward;
        playerForward.y = 0;

        float angle = Vector3.Angle(playerForward, directionToEnemy);
        return angle <= blockAngle;
    }

    private void OnDrawGizmosSelected()
    {
        if (player == null) return;

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        if (Application.isPlaying)
        {
            Vector3 forward = player.forward;
            forward.y = 0;
            Vector3 leftBoundary = Quaternion.Euler(0, -blockAngle, 0) * forward;
            Vector3 rightBoundary = Quaternion.Euler(0, blockAngle, 0) * forward;

            Debug.DrawRay(player.position, leftBoundary * 3f, Color.green);
            Debug.DrawRay(player.position, rightBoundary * 3f, Color.green);
        }
    }
}