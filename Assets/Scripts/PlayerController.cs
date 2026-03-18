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

// ==================== Состояние движения (ходьба/бег) ====================
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
        horizontalInput = Input.GetAxis("Horizontal");
        verticalInput = Input.GetAxis("Vertical");
        isRunning = Input.GetKey(KeyCode.LeftShift);
    }

    private void HandleMovement()
    {
        float currentSpeed = isRunning ? controller.runSpeed : controller.walkSpeed;
        Vector3 move = Vector3.zero;

        // Проверка: захват активен?
        if (controller.lockOnSystem != null && controller.lockOnSystem.isLocked && controller.lockOnSystem.currentTarget != null)
        {
            move = GetLockOnMovement();
        }
        else
        {
            move = GetFreeMovement();
        }

        controller.controller.Move(move * currentSpeed * Time.deltaTime);
    }

    private Vector3 GetFreeMovement()
    {
        Vector3 cameraForward = controller.cameraTransform.forward;
        Vector3 cameraRight = controller.cameraTransform.right;
        cameraForward.y = 0; cameraRight.y = 0;
        cameraForward.Normalize(); cameraRight.Normalize();

        return (cameraForward * verticalInput + cameraRight * horizontalInput).normalized;
    }

    private Vector3 GetLockOnMovement()
    {
        Transform target = controller.lockOnSystem.currentTarget;
        Vector3 directionToTarget = (target.position - controller.transform.position).normalized;
        directionToTarget.y = 0;
        Vector3 rightVector = Vector3.Cross(directionToTarget, Vector3.up).normalized;

        return (directionToTarget * verticalInput + rightVector * horizontalInput).normalized;
    }

    private void HandleRotation()
    {
        if (controller.lockOnSystem != null && controller.lockOnSystem.isLocked && controller.lockOnSystem.currentTarget != null)
        {
            HandleLockOnRotation();
            return;
        }

        if (new Vector3(horizontalInput, 0, verticalInput).magnitude > 0.1f)
        {
            Vector3 direction = new Vector3(horizontalInput, 0, verticalInput).normalized;
            direction = controller.cameraTransform.TransformDirection(direction);
            direction.y = 0;

            Quaternion targetRotation = Quaternion.LookRotation(direction);
            controller.transform.rotation = Quaternion.Slerp(controller.transform.rotation, targetRotation, controller.rotationSpeed * Time.deltaTime);
        }
    }

    private void HandleLockOnRotation()
    {
        Transform target = controller.lockOnSystem.currentTarget;
        if (target != null)
        {
            Vector3 dirToTarget = (target.position - controller.transform.position).normalized;
            dirToTarget.y = 0;
            Quaternion targetRotation = Quaternion.LookRotation(dirToTarget);
            controller.transform.rotation = Quaternion.Slerp(controller.transform.rotation, targetRotation, controller.rotationSpeed * Time.deltaTime);
        }
    }

    private void UpdateAnimations()
    {
        // Определяем вектор движения в зависимости от режима
        Vector3 moveVector;
        if (controller.lockOnSystem != null && controller.lockOnSystem.isLocked && controller.lockOnSystem.currentTarget != null)
        {
            Transform target = controller.lockOnSystem.currentTarget;
            Vector3 forward = (target.position - controller.transform.position).normalized;
            forward.y = 0;
            Vector3 right = Vector3.Cross(forward, Vector3.up).normalized;
            moveVector = (forward * verticalInput) + (right * horizontalInput);
        }
        else
        {
            Vector3 cameraForward = controller.cameraTransform.forward;
            cameraForward.y = 0;
            Vector3 cameraRight = controller.cameraTransform.right;
            cameraRight.y = 0;
            moveVector = (cameraForward * verticalInput) + (cameraRight * horizontalInput);
        }

        // Конвертируем в локальные координаты персонажа
        Vector3 localMove = controller.transform.InverseTransformDirection(moveVector);

        // Передаём ДВА параметра вместо одного Speed
        controller.animator.SetFloat("VelocityX", localMove.x, 0.1f, Time.deltaTime);
        controller.animator.SetFloat("VelocityZ", localMove.z, 0.1f, Time.deltaTime);
        controller.animator.SetBool("IsGrounded", controller.isGrounded);
        controller.animator.SetBool("IsRunning", isRunning);
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
    private bool hasJumped;

    public JumpState(PlayerController controller) : base(controller) { }

    public override void Enter()
    {
        // Задаём вертикальную скорость
        controller.playerVelocity.y = Mathf.Sqrt(controller.jumpHeight * -2f * controller.gravity);
        controller.animator.SetTrigger("Jump");
        hasJumped = true;
    }

    public override void Update()
    {
        // Во время прыжка можно разрешить небольшое управление в воздухе
        // Но для простоты оставим без движения. Можно добавить, если нужно.

        // Проверяем, не приземлился ли персонаж
        if (controller.isGrounded && controller.playerVelocity.y <= 0)
        {
            controller.ChangeState(new LocomotionState(controller));
        }
    }

    public override void Exit()
    {
        // Сбрасываем триггер прыжка, чтобы не зациклить
        controller.animator.ResetTrigger("Jump");
    }
}

// ==================== Состояние кувырка ====================
public class RollState : PlayerState
{
    private float rollTimer;
    private float rollDuration = 0.8f; // должно совпадать с длиной анимации
    private float rollSpeed = 6f;

    public RollState(PlayerController controller) : base(controller) { }

    public override void Enter()
    {
        controller.animator.SetTrigger("Roll");
        rollTimer = 0f;
    }

    public override void Update()
    {
        // Двигаем вперёд
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
    private float attackDuration = 0.8f; // настраивается под анимацию

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
            // Здесь можно добавить эффект лечения (например, восстановить HP)
            controller.ChangeState(new LocomotionState(controller));
        }
    }

    public override void Exit()
    {
        controller.animator.ResetTrigger("Heal");
    }
}

// ==================== Основной контроллер (контекст) ====================
public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float walkSpeed = 3f;
    public float runSpeed = 6f;
    public float rotationSpeed = 10f;

    [Header("Action Settings")]
    public float gravity = -9.81f;
    public float jumpHeight = 1.2f;

    [Header("Combat Settings")]
    public LockOnSystem lockOnSystem;

    // Компоненты
    public Animator animator { get; private set; }
    public CharacterController controller { get; private set; }
    public Transform cameraTransform { get; private set; }

    // Общие данные
    public Vector3 playerVelocity;
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
        // 1. Применяем гравитацию только если мы не в состоянии, которое управляет Y (например, прыжок)
        // Но для простоты оставим здесь, но уберем дублирование Move из состояний
        if (isGrounded && playerVelocity.y < 0)
            playerVelocity.y = -2f;

        // 2. Обновляем состояние
        currentState?.Update();

        // 3. Применяем вертикальную скорость (Гравитация)
        // Важно: CharacterController.Move должен вызываться ОДИН раз за кадр ideally, 
        // но в твоей архитектуре состояния вызывают Move. 
        // Давай оставим как есть для начала, но в LocomotionState уберем вертикальное движение.
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