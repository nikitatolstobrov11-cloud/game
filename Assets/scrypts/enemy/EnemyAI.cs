using UnityEngine;

public enum EnemyArchetype
{
    Aggressive,  // бежит на игрока, атакует быстро
    Patrol,      // ходит по точкам, атакует если игрок близко
    Tank,        // медленный, много HP, сильный удар
    Surrounding  // ходит вокруг игрока, атакует сбоку/сзади
}

public class EnemyAI : MonoBehaviour
{
    [Header("Архетип")]
    public EnemyArchetype archetype = EnemyArchetype.Aggressive;

    [Header("Target")]
    public Transform player;

    [Header("Дистанции")]
    public float detectionRange = 10f;
    public float attackRange    = 2f;
    public float rotationSpeed  = 5f;

    [Header("Атака")]
    public float attackCooldown = 1.5f;
    public int   damage         = 10;
    [Range(0, 180)]
    public float blockAngle     = 45f;

    // ── Патруль ──
    [Header("Патруль (только для Patrol)")]
    public Transform[] patrolPoints;
    public float patrolWaitTime = 2f;

    // ── Окружение ──
    [Header("Окружение (только для Surrounding)")]
    public float surroundRadius   = 3f;   // держать эту дистанцию
    public float surroundSpeed    = 2f;   // скорость бокового движения
    public float surroundAngleOffset = 90f; // смещение угла вокруг игрока

    // ── Приватные ──
    private Animator  _animator;
    private Enemy     _enemy;
    private skeleton  _playerSkeleton;

    private float _lastAttackTime;
    private bool  _isAttacking;

    // Патруль
    private int   _patrolIndex  = 0;
    private float _waitTimer    = 0f;
    private bool  _isWaiting    = false;

    // Окружение
    private float _surroundAngle = 0f;

    // Текущая скорость (зависит от архетипа)
    private float _moveSpeed;

    // ─────────────────────────────────────────
    void Start()
    {
        _animator = GetComponent<Animator>();
        _enemy    = GetComponent<Enemy>();

        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player")?.transform;

        if (player != null)
        {
            _playerSkeleton = player.GetComponent<skeleton>();
            if (_enemy != null) _enemy.playerRef = player;
        }

        // Скорость по архетипу
        _moveSpeed = archetype switch
        {
            EnemyArchetype.Aggressive  => 4.5f,
            EnemyArchetype.Patrol      => 2.0f,
            EnemyArchetype.Tank        => 1.5f,
            EnemyArchetype.Surrounding => 2.5f,
            _                          => 2.0f
        };

        // Для окружающего — случайный стартовый угол чтобы враги не стояли в одной точке
        _surroundAngle = Random.Range(0f, 360f);
    }

    void Update()
    {
        if (_enemy != null && _enemy.IsDead)
        {
            _animator.SetFloat("Speed", 0);
            return;
        }
        if (_playerSkeleton != null && _playerSkeleton.IsDead)
        {
            _animator.SetFloat("Speed", 0);
            return;
        }
        if (player == null) return;

        float distance = Vector3.Distance(transform.position, player.position);

        switch (archetype)
        {
            case EnemyArchetype.Aggressive:  UpdateAggressive(distance);  break;
            case EnemyArchetype.Patrol:      UpdatePatrol(distance);      break;
            case EnemyArchetype.Tank:        UpdateTank(distance);        break;
            case EnemyArchetype.Surrounding: UpdateSurrounding(distance); break;
        }
    }

    // ═══════════════════════════════════════
    // АГРЕССИВНЫЙ — бежит напролом, атакует часто
    // ═══════════════════════════════════════
    void UpdateAggressive(float distance)
    {
        RotateToPlayer();

        if (distance <= attackRange)
        {
            _animator.SetFloat("Speed", 0);
            TryAttack();
        }
        else if (distance <= detectionRange)
        {
            MoveTowards(player.position, _moveSpeed);
        }
        else
        {
            _animator.SetFloat("Speed", 0);
        }
    }

    // ═══════════════════════════════════════
    // ПАТРУЛЬ — ходит по точкам, реагирует на игрока
    // ═══════════════════════════════════════
    void UpdatePatrol(float distance)
    {
        // Игрок обнаружен — идём к нему
        if (distance <= detectionRange)
        {
            RotateToPlayer();
            if (distance <= attackRange)
            {
                _animator.SetFloat("Speed", 0);
                TryAttack();
            }
            else
            {
                MoveTowards(player.position, _moveSpeed);
            }
            return;
        }

        // Патрулирование
        if (patrolPoints == null || patrolPoints.Length == 0)
        {
            _animator.SetFloat("Speed", 0);
            return;
        }

        if (_isWaiting)
        {
            _animator.SetFloat("Speed", 0);
            _waitTimer -= Time.deltaTime;
            if (_waitTimer <= 0f) _isWaiting = false;
            return;
        }

        Transform target = patrolPoints[_patrolIndex];
        float distToPoint = Vector3.Distance(transform.position, target.position);

        if (distToPoint < 0.5f)
        {
            // Дошли до точки — ждём
            _patrolIndex = (_patrolIndex + 1) % patrolPoints.Length;
            _isWaiting   = true;
            _waitTimer   = patrolWaitTime;
            return;
        }

        // Идём к точке патруля
        RotateTo(target.position);
        MoveTowards(target.position, _moveSpeed);
    }

    // ═══════════════════════════════════════
    // ТАНК — медленный, мощный удар, не отступает
    // ═══════════════════════════════════════
    void UpdateTank(float distance)
    {
        RotateToPlayer();

        if (distance <= attackRange)
        {
            _animator.SetFloat("Speed", 0);
            TryAttack();
        }
        else if (distance <= detectionRange)
        {
            // Танк идёт медленно но неотступно
            MoveTowards(player.position, _moveSpeed);
        }
        else
        {
            _animator.SetFloat("Speed", 0);
        }
    }

    // ═══════════════════════════════════════
    // ОКРУЖАЮЩИЙ — кружит вокруг игрока, атакует сбоку
    // ═══════════════════════════════════════
    void UpdateSurrounding(float distance)
    {
        if (distance > detectionRange)
        {
            _animator.SetFloat("Speed", 0);
            return;
        }

        // Всегда смотрим на игрока
        RotateToPlayer();

        if (distance <= attackRange)
        {
            _animator.SetFloat("Speed", 0);
            TryAttack();
            return;
        }

        // Вычисляем позицию вокруг игрока
        _surroundAngle += surroundSpeed * 30f * Time.deltaTime;

        float rad    = _surroundAngle * Mathf.Deg2Rad;
        Vector3 offset = new Vector3(Mathf.Cos(rad), 0f, Mathf.Sin(rad)) * surroundRadius;
        Vector3 targetPos = player.position + offset;

        float distToTarget = Vector3.Distance(transform.position, targetPos);

        if (distToTarget > 0.3f)
            MoveTowards(targetPos, surroundSpeed);
        else
            _animator.SetFloat("Speed", 0);
    }

    // ═══════════════════════════════════════
    // ОБЩИЕ МЕТОДЫ
    // ═══════════════════════════════════════

    void MoveTowards(Vector3 target, float speed)
    {
        Vector3 dir = (target - transform.position).normalized;
        dir.y = 0;
        transform.position += dir * speed * Time.deltaTime;
        _animator.SetFloat("Speed", speed);
    }

    void RotateToPlayer()
    {
        RotateTo(player.position);
    }

    void RotateTo(Vector3 target)
    {
        Vector3 dir = (target - transform.position).normalized;
        dir.y = 0;
        if (dir == Vector3.zero) return;
        Quaternion targetRot = Quaternion.LookRotation(dir);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotationSpeed * Time.deltaTime);
    }

    void TryAttack()
    {
        if (_isAttacking) return;
        if (Time.time < _lastAttackTime + attackCooldown) return;

        _isAttacking    = true;
        _lastAttackTime = Time.time;

        // Танк бьёт медленнее но сильнее — выбираем триггер
        string trigger = archetype == EnemyArchetype.Tank ? "AttackHeavy" : "Attack";

        // Fallback — если нет AttackHeavy используем Attack
        _animator.SetTrigger(trigger);

        Invoke(nameof(ResetAttack), attackCooldown);
    }

    void ResetAttack() => _isAttacking = false;

    // Вызывается из анимации (Animation Event)
    public void DealDamageToPlayer()
    {
        if (_enemy != null && _enemy.IsDead) return;
        if (_playerSkeleton == null || _playerSkeleton.IsDead) return;

        float distance = Vector3.Distance(transform.position, player.position);
        if (distance > attackRange + 0.5f) return;

        if (_playerSkeleton.IsBlocking() && _playerSkeleton.hasShield && IsPlayerFacingMe())
        {
            _playerSkeleton.PlayBlockSound();
            _playerSkeleton.PlayBlockAnimation();
            _playerSkeleton.PlayBlockEffect();
            Debug.Log("🛡️ Удар заблокирован!");
        }
        else
        {
            // Танк бьёт в 2.5 раза сильнее
            int finalDamage = archetype == EnemyArchetype.Tank
                ? Mathf.RoundToInt(damage * 2.5f)
                : damage;
            _playerSkeleton.TakeDamage(finalDamage);
            Debug.Log($"⚔️ [{archetype}] нанёс {finalDamage} урона");
        }
    }

    bool IsPlayerFacingMe()
    {
        Vector3 dir = (transform.position - player.position).normalized;
        dir.y = 0;
        Vector3 forward = player.forward;
        forward.y = 0;
        return Vector3.Angle(forward, dir) <= blockAngle;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        // Патрульный маршрут
        if (archetype == EnemyArchetype.Patrol && patrolPoints != null)
        {
            Gizmos.color = Color.yellow;
            for (int i = 0; i < patrolPoints.Length; i++)
            {
                if (patrolPoints[i] == null) continue;
                Gizmos.DrawSphere(patrolPoints[i].position, 0.3f);
                int next = (i + 1) % patrolPoints.Length;
                if (patrolPoints[next] != null)
                    Gizmos.DrawLine(patrolPoints[i].position, patrolPoints[next].position);
            }
        }

        // Радиус окружения
        if (archetype == EnemyArchetype.Surrounding)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, surroundRadius);
        }
    }
}