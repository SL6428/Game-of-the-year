using UnityEngine;
using System.Collections;

// ==================== Базовый класс состояния ====================
public abstract class PlayerState
{
    protected PlayerController controller;
    public PlayerState(PlayerController controller)
    {
        this.controller = controller;
    }
    public virtual void Enter() { }
    public virtual void Update() { }
    public virtual void Exit() { }
}

// ==================== Состояние движения (Ходьба/Бег) ====================
public class LocomotionState : PlayerState
{
    private float horizontalInput;
    private float verticalInput;
    private bool isRunning;

    public LocomotionState(PlayerController controller) : base(controller) { }

    public override void Enter() { }

    public override void Update()
    {
        GetInput();
        HandleMovement();
        HandleRotation();
        UpdateAnimations();
        CheckTransitions();
    }

    private void GetInput()
    {
        // ИСПРАВЛЕНО: Убраны пробелы в названиях осей ввода
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");
        isRunning = Input.GetKey(KeyCode.LeftShift);
    }

    private void HandleMovement()
    {
        float currentSpeed = isRunning ? controller.runSpeed : controller.walkSpeed;

        Vector3 cameraForward = controller.cameraTransform.forward;
        Vector3 cameraRight = controller.cameraTransform.right;
        cameraForward.y = 0;
        cameraRight.y = 0;
        cameraForward.Normalize();
        cameraRight.Normalize();

        Vector3 move = (cameraForward * verticalInput + cameraRight * horizontalInput).normalized;
        controller.controller.Move(move * currentSpeed * Time.deltaTime);
    }

    private void HandleRotation()
    {
        if (new Vector3(horizontalInput, 0, verticalInput).magnitude > 0.1f)
        {
            Vector3 direction = new Vector3(horizontalInput, 0, verticalInput).normalized;
            direction = controller.cameraTransform.TransformDirection(direction);
            direction.y = 0;

            if (direction.magnitude > 0.1f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                // Увеличь коэффициент для более плавного поворота
                controller.transform.rotation = Quaternion.Slerp(
                    controller.transform.rotation,
                    targetRotation,
                    controller.rotationSpeed * 0.2f * Time.deltaTime
                );
            }
        }
    }

    private void UpdateAnimations()
    {
        // 1. Считаем магнитуду ввода для перехода Idle ↔ Walk
        float inputMagnitude = new Vector2(horizontalInput, verticalInput).magnitude;
        float speedValue = (inputMagnitude > 0.15f) ? 1f : 0f;
        controller.animator.SetFloat("Speed", speedValue, 0.1f, Time.deltaTime);

        // 2. РАССЧИТЫВАЕМ VelocityX и VelocityZ ОТНОСИТЕЛЬНО ПЕРСОНАЖА
        if (inputMagnitude > 0.15f)
        {
            // Получаем направление камеры (без вертикальной составляющей)
            Vector3 cameraForward = controller.cameraTransform.forward;
            Vector3 cameraRight = controller.cameraTransform.right;
            cameraForward.y = 0;
            cameraRight.y = 0;
            cameraForward.Normalize();
            cameraRight.Normalize();

            // Направление движения в мировом пространстве
            Vector3 worldMoveDirection = (cameraForward * verticalInput + cameraRight * horizontalInput);
            if (worldMoveDirection.magnitude > 1f)
                worldMoveDirection.Normalize();

            // === КЛЮЧЕВОЕ ИСПРАВЛЕНИЕ ===
            // Конвертируем мировое направление в локальное пространство персонажа
            Vector3 localMove = controller.transform.InverseTransformDirection(worldMoveDirection);

            // Используем компоненты вектора как VelocityX и VelocityZ
            float velocityX = localMove.x;
            float velocityZ = localMove.z;

            // Ограничиваем значения в диапазоне [-1, 1]
            velocityX = Mathf.Clamp(velocityX, -1f, 1f);
            velocityZ = Mathf.Clamp(velocityZ, -1f, 1f);

            // Передаем в Animator
            controller.animator.SetFloat("VelocityX", velocityX, 0.1f, Time.deltaTime);
            controller.animator.SetFloat("VelocityZ", velocityZ, 0.1f, Time.deltaTime);

            // Остальные параметры
            controller.animator.SetBool("IsGrounded", controller.isGrounded);
            controller.animator.SetBool("IsRunning", isRunning);
        }
        else
        {
            // При остановке плавно сбрасываем значения в 0        controller.animator.SetFloat("VelocityX", 0f, 0.1f, Time.deltaTime);
            controller.animator.SetFloat("VelocityZ", 0f, 0.1f, Time.deltaTime);
        }
    }

    private void CheckTransitions()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            controller.ChangeState(new JumpState(controller));
            return;
        }
        if (Input.GetKeyDown(KeyCode.LeftAlt))
        {
            controller.ChangeState(new RollState(controller));
            return;
        }
        if (Input.GetMouseButtonDown(0))
        {
            controller.ChangeState(new AttackState(controller));
            return;
        }
        if (Input.GetKeyDown(KeyCode.H))
        {
            controller.ChangeState(new HealState(controller));
            return;
        }
    }
}

// ==================== Состояние прыжка ====================
public class JumpState : PlayerState
{
    public JumpState(PlayerController controller) : base(controller) { }
    public override void Enter()
    {
        controller.playerVelocity.y = Mathf.Sqrt(controller.jumpHeight * -2f * controller.gravity);
        controller.animator.SetTrigger("Jump");
    }

    public override void Update()
    {
        if (controller.isGrounded && controller.playerVelocity.y <= 0)
        {
            controller.ChangeState(new LocomotionState(controller));
        }
    }

    public override void Exit()
    {
        controller.animator.ResetTrigger("Jump");
    }
}

// ==================== Состояние кувырка ====================
public class RollState : PlayerState
{
    private float rollTimer;
    private float rollDuration = 0.8f;
    private float rollSpeed = 6f;

    public RollState(PlayerController controller) : base(controller) { }

    public override void Enter()
    {
        controller.animator.SetTrigger("Roll");
        rollTimer = 0f;
    }

    public override void Update()
    {
        Vector3 forward = controller.cameraTransform.forward;
        forward.y = 0;
        controller.controller.Move(forward * rollSpeed * Time.deltaTime);

        rollTimer += Time.deltaTime;
        if (rollTimer >= rollDuration)
        {
            controller.ChangeState(new LocomotionState(controller));
        }
    }

    public override void Exit()
    {
        controller.animator.ResetTrigger("Roll");
    }
}

// ==================== Состояние атаки ====================
public class AttackState : PlayerState
{
    private float attackTimer;
    private float attackDuration = 0.8f;

    public AttackState(PlayerController controller) : base(controller) { }

    public override void Enter()
    {
        controller.animator.SetTrigger("Attack");
        attackTimer = 0f;
    }

    public override void Update()
    {
        attackTimer += Time.deltaTime;
        if (attackTimer >= attackDuration)
        {
            controller.ChangeState(new LocomotionState(controller));
        }
    }

    public override void Exit()
    {
        controller.animator.ResetTrigger("Attack");
    }
}

// ==================== Состояние лечения ====================
public class HealState : PlayerState
{
    private float healTimer;
    private float healDuration = 1.5f;

    public HealState(PlayerController controller) : base(controller) { }

    public override void Enter()
    {
        controller.animator.SetTrigger("Heal");
        healTimer = 0f;
    }

    public override void Update()
    {
        healTimer += Time.deltaTime;
        if (healTimer >= healDuration)
        {
            controller.ChangeState(new LocomotionState(controller));
        }
    }

    public override void Exit()
    {
        controller.animator.ResetTrigger("Heal");
    }
}

// ==================== Главный контроллер ====================
public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float walkSpeed = 3f;
    public float runSpeed = 6f;
    public float rotationSpeed = 10f;

    [Header("Action Settings")]
    public float gravity = -9.81f;
    public float jumpHeight = 1.2f;

    // Ссылки
    public Animator animator { get; private set; }
    public CharacterController controller { get; private set; }
    public Transform cameraTransform { get; private set; }

    // Для Lock-On системы
    [Header("Combat Settings")]
    public LockOnSystem lockOnSystem;

    // Текущая скорость
    public Vector3 playerVelocity;
    // ИСПРАВЛЕНО: Синтаксис свойства
    public bool isGrounded => controller.isGrounded;

    // Текущее состояние
    private PlayerState currentState;

    void Start()
    {
        animator = GetComponent<Animator>();
        controller = GetComponent<CharacterController>();
        cameraTransform = Camera.main.transform;

        if (controller == null)
            controller = gameObject.AddComponent<CharacterController>();

        // Начальное состояние - движение
        currentState = new LocomotionState(this);
        currentState.Enter();
    }

    void Update()
    {
        // Применяем grounded
        if (isGrounded && playerVelocity.y < 0)
            playerVelocity.y = -2f;

        // Обновляем текущее состояние
        currentState?.Update();

        // Применяем гравитацию (вертикальное движение)
        playerVelocity.y += gravity * Time.deltaTime;
        controller.Move(playerVelocity * Time.deltaTime);
    }

    // Метод для смены состояния
    public void ChangeState(PlayerState newState)
    {
        currentState?.Exit();
        currentState = newState;
        currentState.Enter();
    }
}