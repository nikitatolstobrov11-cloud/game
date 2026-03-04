using UnityEngine;

public class skeleton : MonoBehaviour
{
    public float walkSpeed = 3f;
    public float runSpeed = 6f;
    public float jumpForce = 8f;
    public float rotationSpeed = 10f;

    private Animator animator;
    private Rigidbody rb;
    private bool isGrounded;
    private Transform cameraTransform;

    void Start()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();
        cameraTransform = Camera.main.transform;
    }

    void Update()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        Vector3 camF = cameraTransform.forward;
        Vector3 camR = cameraTransform.right;
        camF.y = 0; camR.y = 0;
        camF.Normalize(); camR.Normalize();

        Vector3 movement = (camF * v + camR * h).normalized;

        // Бег (зажат Shift + есть движение + на земле)
        bool isRunning = Input.GetKey(KeyCode.LeftShift) && movement.magnitude > 0.1f && isGrounded;

        // Выбираем скорость в зависимости от бега
        float currentSpeed = isRunning ? runSpeed : walkSpeed;
        
        // В воздухе скорость снижена
        if (!isGrounded) currentSpeed = walkSpeed * 0.7f;

        Vector3 newVel = rb.linearVelocity;
        newVel.x = movement.x * currentSpeed;
        newVel.z = movement.z * currentSpeed;
        rb.linearVelocity = newVel;

        // Поворачиваем персонажа только если есть движение ВПЕРЕД или В СТОРОНЫ, но не назад
        if (movement.magnitude > 0.1f && v >= 0)
        {
            Quaternion targetRot = Quaternion.LookRotation(movement);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotationSpeed * Time.deltaTime);
        }

        Vector3 localVel = transform.InverseTransformDirection(rb.linearVelocity);
        animator.SetFloat("SpeedZ", localVel.z);
        animator.SetFloat("SpeedX", localVel.x);

        // ⚡️ Передаём параметр isRunning в аниматор
        animator.SetBool("isRunning", isRunning);

        // Прыжок
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            animator.SetTrigger("Jump");
        }

        // ===== АТАКА =====
        if (Input.GetMouseButtonDown(0) && isGrounded) // 0 = левая кнопка мыши
        {
            Attack();
        }

        animator.SetBool("IsGrounded", isGrounded);
    }

    void Attack()
    {
        animator.SetTrigger("Attack");
        Debug.Log("Удар!");
    }

    void OnCollisionStay(Collision collision) => isGrounded = true;
    void OnCollisionExit(Collision collision) => isGrounded = false;
}