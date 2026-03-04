using UnityEngine;

public class skeleton : MonoBehaviour
{
    public float walkSpeed = 3f;
    public float runSpeed = 6f;
    public float jumpForce = 8f;
    public float rotationSpeed = 10f;

    // Здоровье игрока
    public int maxHealth = 100;
    public int currentHealth;

    private Animator animator;
    private Rigidbody rb;
    private bool isGrounded;
    private Transform cameraTransform;
    private bool isPickingUp = false; // флаг для анимации подбора

    void Start()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();
        cameraTransform = Camera.main.transform;
        currentHealth = maxHealth; // инициализация здоровья
    }

    void Update()
    {
        if (isPickingUp) return;

        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        Vector3 camF = cameraTransform.forward;
        Vector3 camR = cameraTransform.right;
        camF.y = 0; camR.y = 0;
        camF.Normalize(); camR.Normalize();

        Vector3 movement = (camF * v + camR * h).normalized;

        bool isRunning = Input.GetKey(KeyCode.LeftShift) && movement.magnitude > 0.1f && isGrounded;

        float currentSpeed = isRunning ? runSpeed : walkSpeed;
        if (!isGrounded) currentSpeed = walkSpeed * 0.7f;

        Vector3 newVel = rb.linearVelocity;
        newVel.x = movement.x * currentSpeed;
        newVel.z = movement.z * currentSpeed;
        rb.linearVelocity = newVel;

        if (movement.magnitude > 0.1f && v >= 0)
        {
            Quaternion targetRot = Quaternion.LookRotation(movement);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotationSpeed * Time.deltaTime);
        }

        Vector3 localVel = transform.InverseTransformDirection(rb.linearVelocity);
        animator.SetFloat("SpeedZ", localVel.z);
        animator.SetFloat("SpeedX", localVel.x);

        animator.SetBool("isRunning", isRunning);

        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            animator.SetTrigger("Jump");
        }

        if (Input.GetMouseButtonDown(0) && isGrounded)
        {
            Attack();
        }

        // ТЕСТ: нажми H чтобы получить урон
        if (Input.GetKeyDown(KeyCode.H))
        {
            TakeDamage(10);
        }

        animator.SetBool("IsGrounded", isGrounded);
    }

    void Attack()
    {
        animator.SetTrigger("Attack");
        Debug.Log("Удар!");
    }

    // Вызывается из Interaction.cs при подборе меча или щита
    public void StartPickUp()
    {
        if (isPickingUp) return;
        isPickingUp = true;
        animator.SetTrigger("PickUp");
        Invoke(nameof(EndPickUp), 1f); // подбери время под свою анимацию
    }

    void EndPickUp()
    {
        isPickingUp = false;
    }

    // Метод получения урона
    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        Debug.Log($"Скелет получил {damage} урона. Осталось {currentHealth} HP");

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        Debug.Log("Скелет умер... Game Over");
        // Здесь можно добавить перезагрузку сцены или показ меню
        // Например: UnityEngine.SceneManagement.SceneManager.LoadScene(0);
    }

    void OnCollisionStay(Collision collision) => isGrounded = true;
    void OnCollisionExit(Collision collision) => isGrounded = false;
}