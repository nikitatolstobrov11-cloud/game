using UnityEngine;
using UnityEngine.InputSystem;

public class ThirdPersonController : MonoBehaviour
{
    public float walkSpeed = 3f;
    public float runSpeed = 6f;
    public float jumpForce = 5f;
    public float rotationSpeed = 10f;
    
    private Animator animator;
    private Rigidbody rb;
    private float currentSpeed;
    private bool isGrounded;
    
    // Для новой системы ввода
    private Vector2 moveInput;
    private bool jumpInput;
    
    void Start()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();
    }
    
    // Этот метод вызывается автоматически новой системой ввода
    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }
    
    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.performed) // Нажали кнопку
        {
            jumpInput = true;
        }
    }
    
    void Update()
    {
        // Получаем ввод из переменных (заполняются через OnMove/OnJump)
        float horizontal = moveInput.x;
        float vertical = moveInput.y;
        
        // Дальше всё как в старом скрипте
        Vector3 movement = new Vector3(horizontal, 0, vertical);
        float speed = movement.magnitude;
        
        // Бег по Shift (тут сложнее, для простоты оставим проверку клавиши)
        if (Keyboard.current.leftShiftKey.isPressed && speed > 0.1f)
            currentSpeed = runSpeed;
        else
            currentSpeed = walkSpeed;
        
        // Для Blend Tree передаём отдельно Z и X
        animator.SetFloat("SpeedZ", vertical * currentSpeed);
        animator.SetFloat("SpeedX", horizontal * currentSpeed);
        
        // Поворот персонажа в сторону движения
        if (movement.magnitude > 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(movement);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
        
        // Прыжок
        if (jumpInput && isGrounded)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            animator.SetTrigger("Jump");
            jumpInput = false; // Сбрасываем
        }
        
        animator.SetBool("IsGrounded", isGrounded);
    }
    
    void OnCollisionStay(Collision collision)
    {
        isGrounded = true;
    }
    
    void OnCollisionExit(Collision collision)
    {
        isGrounded = false;
    }
}