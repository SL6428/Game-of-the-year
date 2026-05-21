using UnityEngine;
using UnityEngine.AI;
using System.Collections;

/// <summary>
/// Простой AI для врага: патрулирование + преследование игрока.
/// </summary>
[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Health))]
public class EnemyAI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private Health health;
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private EnemyAnimator enemyAnimator;

    [Header("Detection")]
    [SerializeField] private float detectionRange = 10f;
    [SerializeField] private float fieldOfView = 90f;
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private LayerMask obstacleLayer;

    [Header("Patrol")]
    [SerializeField] private Transform[] patrolPoints;
    [SerializeField] private float patrolWaitTime = 2f;
    [SerializeField] private float patrolSpeed = 2f;

    [Header("Chase")]
    [SerializeField] private float chaseSpeed = 5f;

    [Header("Combat")]
    [SerializeField] private float damage = 10f;
    [SerializeField] private float attackRange = 2f;
    [SerializeField] private float attackCooldown = 1.5f;

    [Header("Debug")]
    [SerializeField] private bool showDebugGizmos = true;

    [Header("Death")]
    [SerializeField] private GameObject deathEffect;
    [SerializeField] private float disappearDelay = 1f;
    [SerializeField] private float respawnTime = 20f; // Время до возрождения
    [SerializeField] private bool respawnEnabled = true; // Включить возрождение
    [SerializeField] private float deathAnimDuration = 2.5f; // Длительность анимации смерти
    [SerializeField] private float sinkSpeed = 2f; // Скорость погружения под пол (секунды)
    [SerializeField] private float sinkDistance = 3f; // Насколько глубоко уйти под пол
    [SerializeField] private float fadeInSpeed = 2f; // Скорость появления при респауне (секунды)

    // Состояния
    private enum EnemyState { Patrol, Chase, Attack, Dead }
    private EnemyState currentState = EnemyState.Patrol;

    private int currentPatrolIndex = 0;
    private float patrolWaitTimer = 0f;
    private float attackTimer = 0f;
    private bool isDead = false;
    private bool isAttacking = false; // Защита от повторного запуска анимации атаки
    
    // Для респауна
    private Vector3 spawnPosition; // Позиция появления
    private Quaternion spawnRotation; // Поворот при появлении
    
    // Кеш компонентов для оптимизации
    private Renderer[] cachedRenderers;
    private Collider[] cachedColliders;
    private float sqrDetectionRange; // Кешированный квадрат дистанции
    private float sqrAttackRange;

    void Start()
    {
        // Сохраняем позицию появления для респауна
        spawnPosition = transform.position;
        spawnRotation = transform.rotation;

        // Получаем компоненты
        if (health == null)
            health = GetComponent<Health>();

        if (agent == null)
            agent = GetComponent<NavMeshAgent>();

        // Кешируем компоненты для оптимизации
        cachedRenderers = GetComponentsInChildren<Renderer>();
        cachedColliders = GetComponents<Collider>();
        
        // Кешируем квадраты дистанций для оптимизации
        sqrDetectionRange = detectionRange * detectionRange;
        sqrAttackRange = attackRange * attackRange;

        // Проверяем что агент на NavMesh
        if (!agent.isOnNavMesh)
        {
            Debug.LogWarning($"{gameObject.name}: ⚠️ NavMeshAgent не на NavMesh! Враг будет отключён. Запеки NavMesh (Window → AI → Navigation → Bake).");
            enabled = false; // Отключаем скрипт чтобы не было ошибок
            return;
        }

        // Ищем игрока
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
                player = playerObj.transform;
        }

        // Настройки агента
        agent.stoppingDistance = attackRange;
        agent.angularSpeed = 120f;

        // Подписываемся на смерть
        health.OnDeath += OnDeath;

        // Начинаем патрулирование
        if (patrolPoints.Length > 0)
            GoToNextPatrolPoint();
    }

    void OnDestroy()
    {
        if (health != null)
            health.OnDeath -= OnDeath;
    }

    void Update()
    {
        // Проверяем смерть
        if (health == null || health.IsDead || isDead)
        {
            return;
        }

        // Если в состоянии смерти - не обновляем AI
        if (currentState == EnemyState.Dead) return;

        // Проверяем видимость игрока
        bool canSeePlayer = CanSeePlayer();

        // Машина состояний
        switch (currentState)
        {
            case EnemyState.Patrol:
                UpdatePatrol(canSeePlayer);
                break;

            case EnemyState.Chase:
                UpdateChase(canSeePlayer);
                break;

            case EnemyState.Attack:
                UpdateAttack(canSeePlayer);
                break;
        }

        // Таймер атаки
        if (attackTimer > 0)
            attackTimer -= Time.deltaTime;
    }

    private void UpdatePatrol(bool canSeePlayer)
    {
        // Обновляем анимацию движения — используем velocity с порогом для точности
        if (enemyAnimator != null)
        {
            bool isMoving = !agent.isStopped && agent.velocity.sqrMagnitude > 0.01f;
            enemyAnimator.SetSpeed(isMoving ? 1f : 0f);
        }

        if (canSeePlayer)
        {
            agent.isStopped = false;
            currentState = EnemyState.Chase;
            return;
        }

        // Если достигли точки патрулирования
        if (!agent.pathPending && agent.isOnNavMesh && agent.remainingDistance <= agent.stoppingDistance)
        {
            patrolWaitTimer += Time.deltaTime;

            if (patrolWaitTimer >= patrolWaitTime)
            {
                GoToNextPatrolPoint();
                patrolWaitTimer = 0f;
            }
        }

        agent.speed = patrolSpeed;
    }

    private void UpdateChase(bool canSeePlayer)
    {
        if (!canSeePlayer)
        {
            // Потеряли игрока - возвращаемся к патрулированию
            currentState = EnemyState.Patrol;
            return;
        }

        agent.speed = chaseSpeed;

        // Двигаемся к игроку
        if (agent.isOnNavMesh)
        {
            agent.SetDestination(player.position);
        }
        else
        {
            Debug.LogWarning($"{gameObject.name}: Агент не на NavMesh, не могу преследовать игрока.");
            currentState = EnemyState.Patrol;
            return;
        }

        // Проверяем дистанцию атаки (используем sqrMagnitude для оптимизации)
        float sqrDistanceToPlayer = (transform.position - player.position).sqrMagnitude;

        if (sqrDistanceToPlayer <= sqrAttackRange)
        {
            currentState = EnemyState.Attack;
        }

        // Обновляем анимацию
        if (enemyAnimator != null)
        {
            bool isMoving = !agent.isStopped && agent.velocity.sqrMagnitude > 0.01f;
            enemyAnimator.SetSpeed(isMoving ? 1f : 0f);
        }
    }

    private void UpdateAttack(bool canSeePlayer)
    {
        if (!canSeePlayer)
        {
            // Потеряли игрока — возвращаемся к патрулированию
            agent.isStopped = false;
            isAttacking = false; // Сбрасываем флаг атаки
            currentState = EnemyState.Patrol;
            return;
        }

        float sqrDistanceToPlayer = (transform.position - player.position).sqrMagnitude;

        if (sqrDistanceToPlayer > sqrAttackRange)
        {
            // Игрок вышел из радиуса атаки — преследуем
            agent.isStopped = false;
            isAttacking = false; // Сбрасываем флаг атаки
            currentState = EnemyState.Chase;
            return;
        }

        // Поворачиваемся к игроку
        Vector3 direction = (player.position - transform.position).normalized;
        direction.y = 0;
        if (direction.sqrMagnitude > 0.0001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
        }

        // Атака
        if (attackTimer <= 0)
        {
            Attack();
            attackTimer = attackCooldown;
        }
        else
        {
            // После атаки возобновляем движение
            if (agent.isStopped && attackTimer < attackCooldown - 0.3f)
            {
                agent.isStopped = false;
            }

            // Сбрасываем флаг атаки после окончания анимации (~0.8 сек)
            if (isAttacking && attackTimer < attackCooldown - 0.7f)
            {
                isAttacking = false;
            }
        }

        // Обновляем анимацию — во время атаки скорость = 0
        if (enemyAnimator != null)
        {
            if (agent.isStopped)
                enemyAnimator.SetSpeed(0f);
            else
                enemyAnimator.SetSpeed(0.1f); // Лёгкое движение на месте
        }
    }

    /// <summary>
    /// Проверка: видит ли враг игрока.
    /// </summary>
    private bool CanSeePlayer()
    {
        if (player == null) return false;

        // Проверка дистанции (используем sqrMagnitude для оптимизации)
        float sqrDistanceToPlayer = (transform.position - player.position).sqrMagnitude;
        
        if (sqrDistanceToPlayer > sqrDetectionRange)
            return false;

        // Проверка угла обзора
        Vector3 directionToPlayer = (player.position - transform.position).normalized;
        float angle = Vector3.Angle(transform.forward, directionToPlayer);

        if (angle > fieldOfView * 0.5f)
            return false;

        // Проверка препятствий (Raycast)
        RaycastHit hit;
        if (Physics.Linecast(
            transform.position + Vector3.up * 0.5f,
            player.position + Vector3.up * 0.5f,
            out hit,
            obstacleLayer))
        {
            if (hit.transform != player)
                return false;
        }

        return true;
    }

    /// <summary>
    /// Перейти к следующей точке патрулирования.
    /// </summary>
    private void GoToNextPatrolPoint()
    {
        if (patrolPoints.Length == 0) return;

        if (!agent.isOnNavMesh)
        {
            Debug.LogWarning($"{gameObject.name}: Не могу перейти к точке патрулирования — агент не на NavMesh! Запеки NavMesh (Window → AI → Navigation → Bake).");
            return;
        }

        try
        {
            agent.SetDestination(patrolPoints[currentPatrolIndex].position);
            currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"{gameObject.name}: Ошибка SetDestination: {e.Message}");
        }
    }

    /// <summary>
    /// Атака игрока.
    /// </summary>
    private void Attack()
    {
        // Защита от повторного запуска анимации
        if (isAttacking) return;
        isAttacking = true;

        // Проигрываем анимацию атаки
        if (enemyAnimator != null)
        {
            enemyAnimator.PlayAttack();
        }

        // Останавливаем движение на время атаки
        if (agent.isOnNavMesh)
        {
            agent.isStopped = true;
        }

        // Ищем игрока и наносим урон
        if (player != null)
        {
            Health playerHealth = player.GetComponent<Health>();

            // Если не нашли на этом объекте, ищем на родителях или дочерних
            if (playerHealth == null)
            {
                playerHealth = player.GetComponentInChildren<Health>();
            }

            if (playerHealth != null && !playerHealth.IsDead)
            {
                playerHealth.TakeDamage(damage);
            }
        }
    }

    /// <summary>
    /// Полная последовательность смерти: ожидание анимации -> погружение под пол -> респаун.
    /// </summary>
    private System.Collections.IEnumerator DeathSequence(float deathAnimDuration)
    {
        // Ждём окончания анимации смерти
        yield return new WaitForSeconds(deathAnimDuration);
        
        // Погружаемся под пол
        yield return StartCoroutine(SinkUnderGround());
        
        // После погружения отключаем рендеры (враг уже под полом)
        foreach (var rend in cachedRenderers)
        {
            if (rend != null)
                rend.enabled = false;
        }
        
        // Возвращаем на начальную позицию (под полом, невидимый)
        transform.position = spawnPosition;
        transform.rotation = spawnRotation;
        
        // Если включен респаун - запускаем таймер
        if (respawnEnabled)
        {
            yield return new WaitForSeconds(respawnTime);
            
            // Возрождаем
            Respawn();
        }
        else
        {
            // Если респаун выключен - уничтожаем объект
            Destroy(gameObject, disappearDelay);
        }
    }
    
    /// <summary>
    /// Плавное погружение врага под пол.
    /// </summary>
    private System.Collections.IEnumerator SinkUnderGround()
    {
        Vector3 startPosition = transform.position;
        Vector3 endPosition = startPosition + Vector3.down * sinkDistance;
        float elapsed = 0f;
        
        // Включаем рендеры чтобы было видно погружение
        foreach (var rend in cachedRenderers)
        {
            if (rend != null)
                rend.enabled = true;
        }
        
        while (elapsed < sinkSpeed)
        {
            float t = elapsed / sinkSpeed;
            transform.position = Vector3.Lerp(startPosition, endPosition, t);
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        // Убеждаемся что достигли конечной позиции
        transform.position = endPosition;
    }
    
    /// <summary>
    /// Смерть врага.
    /// </summary>
    private void OnDeath()
    {
        Die();
    }

    private void Die()
    {
        if (isDead)
        {
            return;
        }

        isDead = true;
        currentState = EnemyState.Dead;

        // Проигрываем анимацию смерти
        if (enemyAnimator != null)
        {
            enemyAnimator.PlayDeath();
        }

        // Останавливаем движение
        if (agent.isOnNavMesh)
        {
            agent.isStopped = true;
            agent.enabled = false;
        }

        // Проигрываем эффект смерти (если есть)
        if (deathEffect != null)
        {
            Instantiate(deathEffect, transform.position, Quaternion.identity);
        }

        // Отключаем коллайдеры
        foreach (var col in cachedColliders)
        {
            if (col != null)
                col.enabled = false;
        }

        // Запускаем coroutine: ждать анимацию -> погрузиться -> возродиться
        StartCoroutine(DeathSequence(deathAnimDuration));
    }
    
    /// <summary>
    /// Возрождение врага.
    /// </summary>
    private void Respawn()
    {
        // Сбрасываем здоровье
        health.ResetHealth();

        // Включаем коллайдеры
        foreach (var col in cachedColliders)
        {
            if (col != null)
                col.enabled = true;
        }

        // Возвращаем на позицию появления
        transform.position = spawnPosition;
        transform.rotation = spawnRotation;

        // Включаем NavMeshAgent
        agent.enabled = true;
        agent.Warp(spawnPosition);

        // Сбрасываем состояние
        isDead = false;
        currentState = EnemyState.Patrol;

        // Запускаем плавное появление из-под пола
        StartCoroutine(RespawnFadeIn());

        // Начинаем патрулирование заново
        currentPatrolIndex = 0;
        if (patrolPoints.Length > 0)
        {
            GoToNextPatrolPoint();
        }
    }
    
    /// <summary>
    /// Плавное появление врага из-под пола.
    /// </summary>
    private System.Collections.IEnumerator RespawnFadeIn()
    {
        // Начинаем немного ниже пола
        Vector3 startPos = transform.position + Vector3.down * sinkDistance * 0.5f;
        Vector3 endPos = transform.position;
        transform.position = startPos;
        
        // Включаем рендеры
        foreach (var rend in cachedRenderers)
        {
            if (rend != null)
                rend.enabled = true;
        }
        
        float elapsed = 0f;
        
        while (elapsed < fadeInSpeed)
        {
            float t = elapsed / fadeInSpeed;
            transform.position = Vector3.Lerp(startPos, endPos, t);
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        // Убеждаемся что достигли конечной позиции
        transform.position = endPos;
    }

    // Отладка
    void OnDrawGizmosSelected()
    {
        if (!showDebugGizmos) return;

        // Радиус обнаружения
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        // Радиус атаки (красный)
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        // Точки патрулирования
        if (patrolPoints != null && patrolPoints.Length > 0)
        {
            Gizmos.color = Color.blue;
            foreach (var point in patrolPoints)
            {
                if (point != null)
                    Gizmos.DrawSphere(point.position, 0.3f);
            }
        }

        // Направление взгляда
        Gizmos.color = Color.green;
        Gizmos.DrawRay(transform.position + Vector3.up * 0.5f, transform.forward * detectionRange);
    }
}
