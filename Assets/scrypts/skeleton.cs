using UnityEngine;

public class skeleton : MonoBehaviour
{
    [Header("Movement Settings")]
    public float walkSpeed = 3f;
    public float runSpeed = 6f;
    public float jumpForce = 8f;
    public float rotationSpeed = 10f;

    [Header("Health")]
    public int maxHealth = 100;
    public int currentHealth;

    [Header("Stamina")]
    public float maxStamina = 100f;
    public float currentStamina;
    public float staminaRegenRate = 15f;
    public float runStaminaCost = 10f;
    public float attackStaminaCost = 15f;
    public float blockStaminaCost = 5f;

    [Header("Roll")]
    public float rollStaminaCost = 20f;
    public float rollDuration = 0.6f;
    public float rollCooldown = 0.3f;
    public float holdThreshold = 0.1f;
    public float rollForce = 5f;

    [Header("Lock-On")]
    public float lockOnRotationSpeed = 360f;
    private Transform lockOnTarget;
    private bool lockOnActive = false;

    [Header("Sounds")]
    public AudioClip[] battleCries;
    public AudioClip deathSound;
    public AudioClip blockSound;                  // звук удара по щиту

    [Header("Shield")]
    public bool hasShield = false;
    public GameObject shieldObject;                // объект щита (для позиции эффекта)
    public GameObject blockEffectPrefab;           // эффект при ударе по щиту (частицы)

    [Header("Game Over")]
    public GameObject gameOverCanvas;

    private Animator animator;
    private Rigidbody rb;
    private bool isGrounded;
    private Transform cameraTransform;
    private bool isPickingUp = false;
    private bool isBlocking = false;
    private AudioSource voiceSource;
    private AudioSource blockAudioSource;          // отдельный источник для звука блока
    private bool isDead = false;

    // Roll variables
    private bool isRolling = false;
    private bool isInvincible = false;
    private float rollEndTime = 0f;
    private float nextRollTime = 0f;
    private bool expectingRoll = false;
    private float runRollPressStart = 0f;

    private float rollDirX = 0f;
    private float rollDirZ = 1f;

    public bool IsDead => isDead;

    void Start()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();
        cameraTransform = Camera.main.transform;
        currentHealth = maxHealth;
        currentStamina = maxStamina;

        // Основной AudioSource (для криков, смерти)
        voiceSource = GetComponent<AudioSource>();
        if (voiceSource == null)
            voiceSource = gameObject.AddComponent<AudioSource>();
        voiceSource.spatialBlend = 1f;
        voiceSource.playOnAwake = false;

        // Отдельный AudioSource для звука блока (2D – всегда слышен)
        blockAudioSource = gameObject.AddComponent<AudioSource>();
        blockAudioSource.spatialBlend = 0f;   // 2D
        blockAudioSource.volume = 1f;
        blockAudioSource.playOnAwake = false;
    }

    void Update()
    {
        if (isDead) return;
        if (isPickingUp) return;

        if (currentStamina < maxStamina)
        {
            currentStamina += staminaRegenRate * Time.deltaTime;
            if (currentStamina > maxStamina) currentStamina = maxStamina;
        }

        HandleRunRollInput();

        if (isRolling)
        {
            if (Time.time >= rollEndTime) EndRoll();
            return;
        }

        if (lockOnActive && lockOnTarget != null)
        {
            Vector3 directionToTarget = lockOnTarget.position - transform.position;
            directionToTarget.y = 0f;
            if (directionToTarget != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, lockOnRotationSpeed * Time.deltaTime);
            }
        }

        bool blockPressed = Input.GetMouseButton(1);
        isBlocking = blockPressed && hasShield && !isRolling;
        if (isBlocking && currentStamina > 0)
        {
            currentStamina -= blockStaminaCost * Time.deltaTime;
            if (currentStamina < 0) currentStamina = 0;
        }
        animator.SetBool("isBlocking", isBlocking && currentStamina > 0);

        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        Vector3 camForward = cameraTransform.forward;
        Vector3 camRight = cameraTransform.right;
        camForward.y = 0; camRight.y = 0;
        camForward.Normalize(); camRight.Normalize();

        Vector3 movement = (camForward * v + camRight * h).normalized;
        bool isRunning = Input.GetKey(KeyCode.Space) && movement.magnitude > 0.1f && isGrounded && !isRolling;

        if (isRunning && currentStamina > 0)
        {
            currentStamina -= runStaminaCost * Time.deltaTime;
            if (currentStamina < 0) currentStamina = 0;
        }

        float currentSpeed = (isRunning && currentStamina > 0) ? runSpeed : walkSpeed;
        if (!isGrounded) currentSpeed = walkSpeed * 0.7f;

        Vector3 newVel = rb.linearVelocity;
        newVel.x = movement.x * currentSpeed;
        newVel.z = movement.z * currentSpeed;
        rb.linearVelocity = newVel;

        if (!lockOnActive && movement.magnitude > 0.1f && v >= 0)
        {
            Quaternion targetRot = Quaternion.LookRotation(movement);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotationSpeed * Time.deltaTime);
        }

        Vector3 localVel = transform.InverseTransformDirection(rb.linearVelocity);
        animator.SetFloat("SpeedZ", localVel.z);
        animator.SetFloat("SpeedX", localVel.x);
        animator.SetBool("isRunning", isRunning && currentStamina > 0);

        if (Input.GetKeyDown(KeyCode.LeftShift) && isGrounded && !isRolling)
        {
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            animator.SetTrigger("Jump");
        }

        if (Input.GetMouseButtonDown(0) && isGrounded && currentStamina >= attackStaminaCost && !isRolling)
        {
            currentStamina -= attackStaminaCost;
            Attack();
        }

        if (Input.GetKeyDown(KeyCode.H))
        {
            TakeDamage(10);
        }

        animator.SetBool("IsGrounded", isGrounded);
    }

    void HandleRunRollInput()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            runRollPressStart = Time.time;
            expectingRoll = true;
        }

        if (expectingRoll && Input.GetKey(KeyCode.Space) && Time.time - runRollPressStart >= holdThreshold)
        {
            expectingRoll = false;
        }

        if (Input.GetKeyUp(KeyCode.Space) && expectingRoll)
        {
            if (!isRolling && Time.time >= nextRollTime && isGrounded && currentStamina >= rollStaminaCost)
            {
                StartRoll();
            }
            expectingRoll = false;
        }
    }

    void StartRoll()
    {
        isRolling = true;
        isInvincible = true;
        currentStamina -= rollStaminaCost;
        if (currentStamina < 0) currentStamina = 0;

        rollEndTime = Time.time + rollDuration;
        nextRollTime = Time.time + rollDuration + rollCooldown;

        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        Vector3 camForward = cameraTransform.forward;
        Vector3 camRight = cameraTransform.right;
        camForward.y = 0; camRight.y = 0;
        camForward.Normalize(); camRight.Normalize();

        Vector3 moveDirection = (camForward * v + camRight * h).normalized;

        if (moveDirection.magnitude > 0.1f)
        {
            Vector3 localDir = transform.InverseTransformDirection(moveDirection);
            rollDirX = localDir.x;
            rollDirZ = localDir.z;
        }
        else
        {
            moveDirection = transform.forward;
            rollDirX = 0f;
            rollDirZ = 1f;
        }

        animator.SetFloat("RollX", rollDirX);
        animator.SetFloat("RollZ", rollDirZ);
        animator.SetTrigger("Roll");

        rb.linearVelocity = new Vector3(moveDirection.x * rollForce, rb.linearVelocity.y, moveDirection.z * rollForce);

        Debug.Log($"Ролл! Направление: ({rollDirX}, {rollDirZ})");
    }

    void EndRoll()
    {
        isRolling = false;
        isInvincible = false;
        Debug.Log("Ролл закончен");
    }

    void Attack()
    {
        animator.SetTrigger("Attack");
        PlayBattleCry();
        Debug.Log("Удар! Осталось стамины: " + currentStamina);
    }

    public void PlayBattleCry()
    {
        if (battleCries != null && battleCries.Length > 0 && voiceSource != null && !isDead)
        {
            int index = Random.Range(0, battleCries.Length);
            voiceSource.PlayOneShot(battleCries[index]);
        }
    }

    public void StartPickUp()
    {
        if (isPickingUp || isDead) return;
        isPickingUp = true;
        animator.SetTrigger("PickUp");
        Invoke(nameof(EndPickUp), 1f);
    }

    void EndPickUp()
    {
        isPickingUp = false;
    }

    // --- Публичные методы для вызова из EnemyAI ---

    /// <summary> Воспроизвести звук блока </summary>
    public void PlayBlockSound()
    {
        if (blockSound != null && blockAudioSource != null)
        {
            blockAudioSource.PlayOneShot(blockSound);
            Debug.Log("🔊 Звук блока");
        }
        else
        {
            if (blockSound == null) Debug.LogWarning("blockSound не назначен!");
            if (blockAudioSource == null) Debug.LogWarning("blockAudioSource = null!");
        }
    }

    /// <summary> Воспроизвести анимацию блока </summary>
    public void PlayBlockAnimation()
    {
        if (animator != null)
            animator.SetTrigger("Block");
    }

    /// <summary> Создать эффект частиц на щите </summary>
    public void PlayBlockEffect()
    {
        if (blockEffectPrefab == null) return;

        Vector3 spawnPos = shieldObject != null ? shieldObject.transform.position : transform.position;
        GameObject effect = Instantiate(blockEffectPrefab, spawnPos, Quaternion.identity);
        if (shieldObject != null)
            effect.transform.parent = shieldObject.transform; // эффект будет двигаться вместе со щитом
        Destroy(effect, 2f);
    }

    // --- Получение урона ---
    public void TakeDamage(int damage)
    {
        if (isDead) return;

        if (isInvincible)
        {
            Debug.Log("Уклонение! Урон проигнорирован.");
            return;
        }

        if (IsBlocking())
        {
            Debug.Log("Блок! Урон поглощён.");
            // Звук и эффект вызываются из EnemyAI, чтобы не дублировать
            return;
        }

        currentHealth -= damage;
        Debug.Log($"Скелет получил {damage} урона. Осталось {currentHealth} HP");

        if (currentHealth <= 0) Die();
    }

    void Die()
    {
        Debug.Log("Скелет умер...");

        isDead = true;
        isPickingUp = false;
        enabled = false;

        rb.isKinematic = true;
        rb.constraints = RigidbodyConstraints.FreezeAll;

        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false;

        animator.SetTrigger("Die");

        if (deathSound != null && voiceSource != null)
        {
            voiceSource.PlayOneShot(deathSound);
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        ShowGameOverUI();
    }

    void ShowGameOverUI()
    {
        if (gameOverCanvas != null)
        {
            gameOverCanvas.SetActive(true);
            Debug.Log("✅ Game Over Canvas активирован");
        }
        else
        {
            Debug.LogError("❌ GameOverCanvas не назначен в инспекторе!");
        }
    }

    public bool IsBlocking()
    {
        return isBlocking && currentStamina > 0 && !isDead;
    }

    public void SetLockOnTarget(Transform target)
    {
        lockOnTarget = target;
        lockOnActive = (target != null);
    }

    public void ClearLockOn()
    {
        lockOnTarget = null;
        lockOnActive = false;
    }

    void OnCollisionStay(Collision collision) => isGrounded = true;
    void OnCollisionExit(Collision collision) => isGrounded = false;
}