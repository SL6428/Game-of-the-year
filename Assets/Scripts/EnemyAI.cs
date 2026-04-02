using UnityEngine;
using UnityEngine.AI;

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
    [SerializeField] private float respawnTime = 5f; // Время до возрождения
    [SerializeField] private bool respawnEnabled = true; // Включить возрождение

    // Состояния
    private enum EnemyState { Patrol, Chase, Attack, Dead }
    private EnemyState currentState = EnemyState.Patrol;

    private int currentPatrolIndex = 0;
    private float patrolWaitTimer = 0f;
    private float attackTimer = 0f;
    private bool isDead = false;
    
    // Для респауна
    private Vector3 spawnPosition; // Позиция появления
    private Quaternion spawnRotation; // Поворот при появлении

    void Start()
    {
        // Сохраняем позицию появления для респауна
        spawnPosition = transform.position;
        spawnRotation = transform.rotation;
        Debug.Log($"[{gameObject.name}] 📍 Начальная позиция сохранена: {spawnPosition}");
        
        // Получаем компоненты
        if (health == null)
            health = GetComponent<Health>();

        if (agent == null)
            agent = GetComponent<NavMeshAgent>();

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

        // Проверяем что агент на NavMesh
        if (!agent.isOnNavMesh)
        {
            Debug.LogWarning($"{gameObject.name}: NavMeshAgent не на NavMesh! Проверьте что NavMesh запечён.");
        }

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
        // Проверяем смерть - но не вызываем Die() здесь,
        // потому что OnDeath уже вызывается из Health.OnDeath
        if (health == null || health.IsDead)
        {
            if (!isDead)
            {
                // Это нужно только если OnDeath не был вызван
                // В нормальном случае OnDeath вызывается из Health
                Debug.LogWarning($"[{gameObject.name}] Update: health.IsDead=true но isDead=false. Возможно OnDeath не сработал!");
            }
            return;
        }

        if (isDead) return;

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
        if (canSeePlayer)
        {
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
        agent.SetDestination(player.position);

        // Проверяем дистанцию атаки
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer <= attackRange)
        {
            currentState = EnemyState.Attack;
        }
    }

    private void UpdateAttack(bool canSeePlayer)
    {
        if (!canSeePlayer)
        {
            currentState = EnemyState.Patrol;
            return;
        }

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer > attackRange)
        {
            currentState = EnemyState.Chase;
            return;
        }

        // Поворачиваемся к игроку
        Vector3 direction = (player.position - transform.position).normalized;
        direction.y = 0;
        if (direction.magnitude > 0.01f)
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
    }

    /// <summary>
    /// Проверка: видит ли враг игрока.
    /// </summary>
    private bool CanSeePlayer()
    {
        if (player == null) return false;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        // Проверка дистанции
        if (distanceToPlayer > detectionRange)
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

        agent.SetDestination(patrolPoints[currentPatrolIndex].position);
        currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
    }

    /// <summary>
    /// Атака игрока.
    /// </summary>
    private void Attack()
    {
        Debug.Log($"{gameObject.name} атакует игрока!");
        
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
                Debug.Log($"EnemyAI: Наносим {damage} урона игроку. HP до: {playerHealth.CurrentHealth}");
                playerHealth.TakeDamage(damage);
                Debug.Log($"EnemyAI: HP игрока после: {playerHealth.CurrentHealth}");
            }
            else
            {
                if (playerHealth == null)
                    Debug.LogWarning($"EnemyAI: Health не найден у игрока!");
                else if (playerHealth.IsDead)
                    Debug.Log($"EnemyAI: Игрок уже мёртв!");
            }
        }
        else
        {
            Debug.LogWarning($"EnemyAI: Игрок не найден для атаки!");
        }
    }

    /// <summary>
    /// Смерть врага.
    /// </summary>
    private void OnDeath()
    {
        Debug.Log($"[{gameObject.name}] Получено событие смерти! Вызываем Die()");
        Die();
    }

    private void Die()
    {
        if (isDead)
        {
            Debug.LogWarning($"[{gameObject.name}] Die() вызван но уже мёртв!");
            return;
        }

        isDead = true;
        agent.enabled = false;

        Debug.Log($"[{gameObject.name}] 🪦 ВРАГ ПОБЕЖДЁН! Возрождение через {respawnTime} сек");

        // Проигрываем эффект смерти
        if (deathEffect != null)
        {
            Debug.Log($"[{gameObject.name}] Воспроизводим эффект смерти: {deathEffect.name}");
            Instantiate(deathEffect, transform.position, Quaternion.identity);
        }
        else
        {
            Debug.Log($"[{gameObject.name}] Нет эффекта смерти (deathEffect не назначен)");
        }

        // Отключаем коллайдеры
        Collider[] colliders = GetComponents<Collider>();
        Debug.Log($"[{gameObject.name}] Отключаем {colliders.Length} коллайдеров");
        foreach (var col in colliders)
        {
            col.enabled = false;
        }

        // Отключаем рендер (модель) - скрываем врага
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        Debug.Log($"[{gameObject.name}] Отключаем {renderers.Length} рендеров (скрываем)");
        foreach (var rend in renderers)
        {
            rend.enabled = false;
        }

        // Если включен респаун - запускаем таймер
        if (respawnEnabled)
        {
            Debug.Log($"[{gameObject.name}] ⏰ Запуск таймера респауна: {respawnTime} сек");
            Invoke(nameof(Respawn), respawnTime);
        }
        else
        {
            // Если респаун выключен - уничтожаем объект
            Debug.Log($"[{gameObject.name}] Респаун выключен - уничтожаем объект");
            Destroy(gameObject, disappearDelay);
        }
    }
    
    /// <summary>
    /// Возрождение врага.
    /// </summary>
    private void Respawn()
    {
        Debug.Log($"[{gameObject.name}] ⏰ Время респауна вышло! Возрождаем...");
        
        // Сбрасываем здоровье
        health.ResetHealth();
        Debug.Log($"[{gameObject.name}] HP восстановлено: {health.CurrentHealth}/{health.MaxHealth}");
        
        // Включаем коллайдеры
        Collider[] colliders = GetComponents<Collider>();
        foreach (var col in colliders)
        {
            col.enabled = true;
        }
        Debug.Log($"[{gameObject.name}] Включено {colliders.Length} коллайдеров");
        
        // Включаем рендер (показываем врага)
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        foreach (var rend in renderers)
        {
            rend.enabled = true;
        }
        Debug.Log($"[{gameObject.name}] Включено {renderers.Length} рендеров");
        
        // Возвращаем на позицию появления
        transform.position = spawnPosition;
        transform.rotation = spawnRotation;
        Debug.Log($"[{gameObject.name}] 📍 Возврат на позицию: {spawnPosition}");
        
        // Включаем NavMeshAgent
        agent.enabled = true;
        agent.Warp(spawnPosition);
        
        // Сбрасываем состояние
        isDead = false;
        currentState = EnemyState.Patrol;
        
        // Начинаем патрулирование заново
        currentPatrolIndex = 0;
        if (patrolPoints.Length > 0)
        {
            GoToNextPatrolPoint();
        }
        
        Debug.Log($"[{gameObject.name}] ✅ Враг возродился и готов к бою!");
    }

    // Отладка
    void OnDrawGizmosSelected()
    {
        if (!showDebugGizmos) return;

        // Радиус обнаружения
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

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
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position + Vector3.up * 0.5f, transform.forward * detectionRange);
    }
}
