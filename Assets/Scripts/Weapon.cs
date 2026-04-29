using UnityEngine;

/// <summary>
/// Оружие персонажа. Наносит урон при анимации атаки.
/// Вызывайте методы из Animation Events.
/// </summary>
public class Weapon : MonoBehaviour
{
    [Header("Damage Settings")]
    [SerializeField] private float damage = 20f;

    [Header("References")]
    [SerializeField] private Health playerHealth;
    [SerializeField] private Collider weaponCollider;

    private bool isAttacking = false;
    private bool canDamage = false;
    private bool hasHitThisAttack = false; // Защита от множественных попаданий за атаку

    void OnEnable()
    {
        // Сбрасываем все флаги при включении оружия
        canDamage = false;
        hasHitThisAttack = false;
        
        if (weaponCollider != null)
        {
            weaponCollider.enabled = false;
        }
    }

    void Awake()
    {
        // Если коллайдер не назначен, ищем на дочерних объектах
        if (weaponCollider == null)
        {
            weaponCollider = GetComponent<Collider>();
            
            // Если не нашли на этом объекте, ищем на дочерних
            if (weaponCollider == null)
            {
                weaponCollider = GetComponentInChildren<Collider>();
            }
        }
        
        if (weaponCollider != null)
        {
            weaponCollider.isTrigger = true;
        }
        else
        {
            Debug.LogError("Weapon: Не найден коллайдер для оружия!");
        }
        
        // Если здоровье игрока не назначено, ищем по тегу Player
        if (playerHealth == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerHealth = player.GetComponent<Health>();
            }
        }
    }

    /// <summary>
    /// Начать атаку (вызывать из анимации).
    /// </summary>
    public void StartAttack()
    {
        isAttacking = true;
    }

    /// <summary>
    /// Конец атаки (вызывать из анимации).
    /// </summary>
    public void EndAttack()
    {
        isAttacking = false;
        canDamage = false;
    }

    /// <summary>
    /// Активировать хитбокс (вызывать в кадре удара).
    /// </summary>
    public void EnableHitbox()
    {
        isAttacking = true;
        canDamage = true;
        hasHitThisAttack = false; // Сбрасываем флаг попадания для новой атаки

        if (weaponCollider != null)
        {
            weaponCollider.enabled = true;
        }
    }

    /// <summary>
    /// Деактивировать хитбокс.
    /// </summary>
    public void DisableHitbox()
    {
        canDamage = false;
        
        if (weaponCollider != null)
        {
            weaponCollider.enabled = false;
        }
    }

#if UNITY_EDITOR
    void Update()
    {
        // Отладка: проверяем что происходит
        if (Input.GetKeyDown(KeyCode.F1))
        {
            Debug.Log($"=== Weapon Debug ===");
            Debug.Log($"isAttacking: {isAttacking}");
            Debug.Log($"canDamage: {canDamage}");
            Debug.Log($"weaponCollider: {weaponCollider}");
            
            if (weaponCollider != null)
            {
                Debug.Log($"Is Trigger: {weaponCollider.isTrigger}");
                Debug.Log($"Enabled: {weaponCollider.enabled}");
            }
        }
    }
#endif

    void OnTriggerEnter(Collider other)
    {
        if (!canDamage || !isAttacking || hasHitThisAttack)
        {
            return;
        }

        // Проверяем что это враг
        if (other.CompareTag("Enemy"))
        {
            Health enemyHealth = other.GetComponent<Health>();

            // Если не нашли на этом объекте, ищем на родителе
            if (enemyHealth == null)
            {
                enemyHealth = other.GetComponentInParent<Health>();
            }

            if (enemyHealth != null && !enemyHealth.IsDead)
            {
                float totalDamage = damage;
                if (PlayerStats.Instance != null)
                    totalDamage += PlayerStats.Instance.DamageBonus;

                enemyHealth.TakeDamage(totalDamage);

                // Помечаем что было попадание и отключаем хитбокс
                hasHitThisAttack = true;
                DisableHitbox();
            }
        }
    }

    /// <summary>
    /// Установить урон оружия.
    /// </summary>
    public void SetDamage(float newDamage)
    {
        damage = newDamage;
    }

    /// <summary>
    /// Получить текущий урон.
    /// </summary>
    public float GetDamage()
    {
        return damage;
    }
}
