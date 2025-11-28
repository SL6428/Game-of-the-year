using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float walkSpeed = 3f;
    public float runSpeed = 6f;
    public float rotationSpeed = 10f;

    private Animator animator;
    private CharacterController controller;
    private Transform cameraTransform;
    private float currentSpeed;
    private bool isRunning;

    // Input values
    private float horizontalInput;
    private float verticalInput;

    void Start()
    {
        animator = GetComponent<Animator>();
        controller = GetComponent<CharacterController>();
        cameraTransform = Camera.main.transform;

        // Если нет Character Controller - добавляем автоматически
        if (controller == null)
            controller = gameObject.AddComponent<CharacterController>();
    }

    void Update()
    {
        GetInput();
        HandleMovement();
        HandleRotation();
        UpdateAnimations();
    }

    void GetInput()
    {
        horizontalInput = Input.GetAxis("Horizontal");
        verticalInput = Input.GetAxis("Vertical");
        isRunning = Input.GetKey(KeyCode.LeftShift);
    }

    void HandleMovement()
    {
        // Определяем скорость в зависимости от бега
        currentSpeed = isRunning ? runSpeed : walkSpeed;

        // Получаем направление движения относительно камеры
        Vector3 cameraForward = cameraTransform.forward;
        Vector3 cameraRight = cameraTransform.right;

        cameraForward.y = 0;
        cameraRight.y = 0;
        cameraForward.Normalize();
        cameraRight.Normalize();

        // Создаем вектор движения
        Vector3 moveDirection = (cameraForward * verticalInput + cameraRight * horizontalInput).normalized;

        // Двигаем персонажа
        controller.SimpleMove(moveDirection * currentSpeed);
    }

    void HandleRotation()
    {
        // Поворачиваем персонажа только если есть движение
        if (new Vector3(horizontalInput, 0, verticalInput).magnitude > 0.1f)
        {
            Vector3 direction = new Vector3(horizontalInput, 0, verticalInput).normalized;

            // Учитываем направление камеры
            direction = cameraTransform.TransformDirection(direction);
            direction.y = 0;

            if (direction.magnitude > 0.1f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            }
        }
    }

    void UpdateAnimations()
    {
        // Вычисляем скорость для анимаций (0-1)
        float speedPercent = (new Vector3(horizontalInput, 0, verticalInput).magnitude > 0.1f) ?
                            (isRunning ? 1f : 0.5f) : 0f;

        animator.SetFloat("Speed", speedPercent, 0.1f, Time.deltaTime);
    }
}