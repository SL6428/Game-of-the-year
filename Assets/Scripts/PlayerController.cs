using UnityEngine;
using System.Collections;
using Unity.Cinemachine;

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
    private float runLockoutTimer;

    public LocomotionState(PlayerController controller) : base(controller) { }

    public override void Enter()
    {
        runLockoutTimer = 0f;
    }

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
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        isRunning = false;

        if (Input.GetKey(KeyCode.LeftShift))
        {
            if (runLockoutTimer > 0f)
            {
                runLockoutTimer -= Time.deltaTime;
            }
            else if (controller.stamina != null)
            {
                if (!controller.stamina.IsExhausted && controller.stamina.CurrentStamina > 1f)
                    isRunning = true;
            }
            else
            {
                isRunning = true;
            }
        }
        else
        {
            runLockoutTimer = 0f;
        }
    }

    private void HandleMovement()
    {
        float currentSpeed = isRunning ? controller.runSpeed : controller.walkSpeed;
        Vector3 cameraForward = controller.cameraPivot.GetForwardDirection();
        Vector3 cameraRight = controller.cameraPivot.GetRightDirection();

        Vector3 move = (cameraForward * verticalInput + cameraRight * horizontalInput);

        if (move.magnitude > 0.1f)
        {
            move.Normalize();
            controller.controller.Move(move * currentSpeed * Time.deltaTime);
        }

        if (isRunning && controller.stamina != null)
        {
            controller.stamina.DrainContinuous(controller.stamina.RunDrainRate);
            if (controller.stamina.IsExhausted)
            {
                isRunning = false;
                runLockoutTimer = 1f;
            }
        }
    }

    private void HandleRotation()
    {
        // В Lock-On режиме не поворачиваем
        if (controller.lockOnSystem != null && controller.lockOnSystem.IsLockedOn())
            return;

        if (new Vector3(horizontalInput, 0, verticalInput).magnitude > 0.1f)
        {
            Vector3 direction = (controller.cameraPivot.GetForwardDirection() * verticalInput + 
                                controller.cameraPivot.GetRightDirection() * horizontalInput).normalized;

            if (direction.magnitude > 0.1f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                controller.transform.rotation = Quaternion.Slerp(
                    controller.transform.rotation,
                    targetRotation,
                    controller.rotationSpeed * Time.deltaTime
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
            // Получаем направление от CameraPivot (только горизонтальное)
            Vector3 cameraForward = controller.cameraPivot.GetForwardDirection();
            Vector3 cameraRight = controller.cameraPivot.GetRightDirection();

            // Направление движения в мировом пространстве
            Vector3 worldMoveDirection = (cameraForward * verticalInput + cameraRight * horizontalInput);
            if (worldMoveDirection.magnitude > 1f)
                worldMoveDirection.Normalize();

            // Конвертируем в локальное пространство персонажа
            Vector3 localMove = controller.transform.InverseTransformDirection(worldMoveDirection);

            // Используем компоненты вектора как VelocityX и VelocityZ
            float velocityX = Mathf.Clamp(localMove.x, -1f, 1f);
            float velocityZ = Mathf.Clamp(localMove.z, -1f, 1f);

            // Передаем в Animator
            controller.animator.SetFloat("VelocityX", velocityX, 0.1f, Time.deltaTime);
            controller.animator.SetFloat("VelocityZ", velocityZ, 0.1f, Time.deltaTime);
        }
        else
        {
            // При остановке плавно сбрасываем значения в 0
            controller.animator.SetFloat("VelocityX", 0f, 0.1f, Time.deltaTime);
            controller.animator.SetFloat("VelocityZ", 0f, 0.1f, Time.deltaTime);
        }

        controller.animator.SetBool("IsGrounded", controller.isGrounded);
        controller.animator.SetBool("IsRunning", isRunning);
    }

    private void CheckTransitions()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (HasStamina(controller.stamina?.JumpCost ?? 0f))
                controller.ChangeState(new JumpState(controller));
            return;
        }
        if (Input.GetKeyDown(KeyCode.LeftAlt))
        {
            if (HasStamina(controller.stamina?.RollCost ?? 0f))
                controller.ChangeState(new RollState(controller));
            return;
        }
        if (Input.GetMouseButtonDown(0))
        {
            if (HasStamina(controller.stamina?.AttackCost ?? 0f))
                controller.ChangeState(new AttackState(controller));
            return;
        }
        if (Input.GetKeyDown(KeyCode.H))
        {
            if (controller.regeneration != null && controller.regeneration.HasFlask && controller.regeneration.CurrentCharges > 0)
            {
                controller.ChangeState(new HealState(controller));
            }
            return;
        }
    }

    private bool HasStamina(float cost)
    {
        if (controller.stamina == null) return true;
        return controller.stamina.CurrentStamina >= cost;
    }
}

// ==================== Состояние прыжка ====================
public class JumpState : PlayerState
{
    public JumpState(PlayerController controller) : base(controller) { }
    public override void Enter()
    {
        if (controller.stamina != null)
            controller.stamina.TryUseStamina(controller.stamina.JumpCost);
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
        if (controller.stamina != null)
            controller.stamina.TryUseStamina(controller.stamina.RollCost);
        controller.animator.SetTrigger("Roll");
        rollTimer = 0f;
    }

    public override void Update()
    {
        // Кувырок в направлении камеры (горизонтальное)
        Vector3 forward = controller.cameraPivot.GetForwardDirection();
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
        if (controller.stamina != null)
            controller.stamina.TryUseStamina(controller.stamina.AttackCost);
        controller.animator.SetTrigger("Attack");
        attackTimer = 0f;
        
        // Включаем хитбокс оружия (используем кешированное оружие)
        if (controller.weapon != null)
        {
            controller.weapon.EnableHitbox();
        }
    }

    public override void Update()
    {
        attackTimer += Time.deltaTime;
        if (attackTimer >= attackDuration)
        {
            // Выключаем хитбокс при выходе из атаки
            if (controller.weapon != null)
            {
                controller.weapon.DisableHitbox();
            }
            
            controller.ChangeState(new LocomotionState(controller));
        }
    }

    public override void Exit()
    {
        controller.animator.ResetTrigger("Attack");
        
        // Гарантированно выключаем хитбокс
        if (controller.weapon != null)
        {
            controller.weapon.DisableHitbox();
        }
    }
}

// ==================== Состояние смерти ====================
public class DeathState : PlayerState
{
    private float waitTimer;
    private bool triggered;
    private bool wasRootMotion;
    private CinemachineInputAxisController camInput;

    public DeathState(PlayerController controller) : base(controller) { }

    public override void Enter()
    {
        waitTimer = 3f;
        triggered = false;

        // Отключаем физику и root motion — иначе анимация смерти подбросит персонажа вверх
        wasRootMotion = controller.animator.applyRootMotion;
        controller.animator.applyRootMotion = false;
        controller.animator.SetTrigger("Death");

        if (controller.controller != null)
        {
            controller.controller.enabled = false;
            controller.playerVelocity = Vector3.zero;
        }

        // Блокируем ввод камеры (Cinemachine)
        camInput = Object.FindFirstObjectByType<CinemachineInputAxisController>();
        if (camInput != null)
            camInput.enabled = false;
    }

    public override void Update()
    {
        waitTimer -= Time.deltaTime;
        if (waitTimer <= 0f && !triggered)
        {
            triggered = true;
            DeathManager.Instance?.ShowDeathScreen(
                controller.RespawnPosition,
                controller.health, controller.regeneration, controller
            );
        }
    }

    public override void Exit()
    {
        controller.animator.ResetTrigger("Death");
        controller.animator.applyRootMotion = wasRootMotion;

        if (controller.controller != null)
            controller.controller.enabled = true;

        if (camInput != null)
            camInput.enabled = true;
    }
}

// ==================== Состояние лечения ====================
public class HealState : PlayerState
{
    private float healTimer;
    private float healDuration = 0.5f;

    public HealState(PlayerController controller) : base(controller) { }

    public override void Enter()
    {
        if (controller.regeneration == null || !controller.regeneration.HasFlask || controller.regeneration.CurrentCharges <= 0)
        {
            controller.ChangeState(new LocomotionState(controller));
            return;
        }

        controller.animator.SetTrigger("Heal");
        healTimer = 0f;
    }

    public override void Update()
    {
        healTimer += Time.deltaTime;
        if (healTimer >= healDuration)
        {
            // Применяем лечение при завершении анимации
            if (controller.regeneration != null && controller.regeneration.HasFlask)
            {
                controller.regeneration.TryHeal();
            }

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
    
    // CameraPivot (публичный для доступа из состояний)
    public CameraPivot cameraPivot { get; set; }

    // Для Lock-On системы
    [Header("Combat Settings")]
    public LockOnSystem lockOnSystem;

    // Система регенерации
    public PlayerRegeneration regeneration { get; private set; }

    // Стамина
    public Stamina stamina { get; private set; }
    
    // Оружие (кешируем для оптимизации)
    public Weapon weapon { get; private set; }
    
    // Health (кешируем для оптимизации)
    public Health health { get; private set; }

    // Текущая скорость
    public Vector3 playerVelocity;
    
    // Своя проверка земли (для Terrain)
    public bool isGrounded => controller.isGrounded || CheckGround();
    
    private bool CheckGround()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position + Vector3.up * 0.1f, Vector3.down, out hit, 0.2f))
            return true;
        return false;
    }

    // Текущее состояние
    private PlayerState currentState;
    public Vector3 RespawnPosition { get; private set; }

    [Header("Respawn")]
    [Tooltip("Если назначен — игрок респавнится здесь. Если пусто — на стартовой позиции.")]
    [SerializeField] private Transform respawnPoint;

    void Awake()
    {
        RespawnPosition = (respawnPoint != null) ? respawnPoint.position : transform.position;
    }

    void Start()
    {
        animator = GetComponent<Animator>();
        controller = GetComponent<CharacterController>();

        // Получаем CameraPivot
        cameraPivot = FindFirstObjectByType<CameraPivot>();
        if (cameraPivot != null)
        {
            cameraTransform = cameraPivot.CameraTransform;
        }
        else
        {
            cameraTransform = Camera.main.transform;
            Debug.LogWarning("CameraPivot не найден! Используем Camera.main");
        }

        if (controller == null)
            controller = gameObject.AddComponent<CharacterController>();

        // Настраиваем CharacterController
        controller.skinWidth = 0.01f;
        controller.center = new Vector3(0, 0.9f, 0);
        controller.height = 1.8f;
        controller.radius = 0.3f;

        // Получаем систему регенерации
        regeneration = GetComponent<PlayerRegeneration>();
        if (regeneration == null)
        {
            regeneration = gameObject.AddComponent<PlayerRegeneration>();
        }

        // Получаем стамину
        stamina = GetComponent<Stamina>();
        if (stamina == null)
        {
            stamina = gameObject.AddComponent<Stamina>();
        }
        
        // Кешируем оружие
        weapon = GetComponent<Weapon>();
        if (weapon == null)
        {
            weapon = GetComponentInChildren<Weapon>();
        }
        
        // Кешируем health
        health = GetComponent<Health>();

        // Подписка на смерть
        if (health != null)
            health.OnDeath += OnPlayerDeath;

        // Начальное состояние - движение
        currentState = new LocomotionState(this);
        currentState.Enter();
    }

    private void OnPlayerDeath()
    {
        if (currentState is DeathState) return;
        ChangeState(new DeathState(this));
    }

    void Update()
    {
#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.F9))
        {
            Debug.Log("[PlayerController] F9 pressed — resetting stats");
            if (PlayerStats.Instance != null)
                PlayerStats.Instance.ResetAllToDefault();
        }
#endif

        // Обновляем текущее состояние (всегда, даже при отключённом контроллере — иначе DeathState не тикает)
        currentState?.Update();

        // Guard: движение/гравитация только когда контроллер активен
        if (controller == null || !controller.enabled)
            return;

        // Применяем grounded
        if (isGrounded && playerVelocity.y < 0)
            playerVelocity.y = -2f;

        // Применяем гравитацию только когда в воздухе
        if (!isGrounded)
        {
            playerVelocity.y += gravity * Time.deltaTime;
            controller.Move(playerVelocity * Time.deltaTime);
        }
    }

    // Метод для смены состояния
    public void ChangeState(PlayerState newState)
    {
        currentState?.Exit();
        currentState = newState;
        currentState.Enter();
    }
}