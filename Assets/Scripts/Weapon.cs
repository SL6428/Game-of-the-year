using UnityEngine;
using System.Collections;

/// <summary>
/// Оружие персонажа. Наносит урон при анимации атаки.
/// Вызывайте методы из Animation Events.
/// </summary>
public class Weapon : MonoBehaviour
{
    [Header("Damage Settings")]
    [SerializeField] private float damage = 20f;
    [SerializeField] private LayerMask enemyLayers;

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
                Debug.Log($"Weapon: Найден коллайдер на дочернем объекте: {weaponCollider?.gameObject.name}");
            }
        }
        
        if (weaponCollider != null)
        {
            weaponCollider.isTrigger = true;
            Debug.Log($"Weapon: Коллайдер настроен - {weaponCollider.gameObject.name}, IsTrigger: {weaponCollider.isTrigger}");
        }
        else
        {
            Debug.LogError("Weapon: Не найден коллайдер для оружия!");
        }
    }

    void Start()
    {
        Debug.Log($"=== Weapon Start ===");
        Debug.Log($"weaponCollider после Awake: {weaponCollider}");
        
        // Если всё ещё null, ищем все коллайдеры на дочерних
        if (weaponCollider == null)
        {
            Collider[] allColliders = GetComponentsInChildren<Collider>();
            Debug.Log($"Найдено коллайдеров на дочерних: {allColliders.Length}");
            
            foreach (var col in allColliders)
            {
                Debug.Log($"  - {col.gameObject.name}: IsTrigger={col.isTrigger}");
            }
            
            if (allColliders.Length > 0)
            {
                weaponCollider = allColliders[0];
                weaponCollider.isTrigger = true;
            }
        }
        
        // Если здоровье игрока не назначено, ищем
        if (playerHealth == null)
        {
            playerHealth = FindObjectOfType<Health>();
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
        isAttacking = true;       // ✅ Устанавливаем флаг атаки
        canDamage = true;
        hasHitThisAttack = false; // Сбрасываем флаг попадания для новой атаки

        if (weaponCollider != null)
        {
            weaponCollider.enabled = true;
        }

        Debug.Log($"Weapon: Hitbox включён! isAttacking={isAttacking}, canDamage={canDamage}");
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
        
        Debug.Log("Weapon: Hitbox выключен!");
    }

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

    void OnTriggerEnter(Collider other)
    {
        Debug.Log($"[TRIGGER] === ВХОД В ТРИГГЕР ===");
        Debug.Log($"[TRIGGER] Объект: {other.gameObject.name}, Тег: {other.tag}");
        Debug.Log($"[TRIGGER] canDamage={canDamage}, isAttacking={isAttacking}, hasHitThisAttack={hasHitThisAttack}");
        Debug.Log($"[TRIGGER] weaponCollider.enabled={weaponCollider?.enabled}");

        if (!canDamage)
        {
            Debug.Log("[TRIGGER] ❌ Отмена: canDamage = false");
            return;
        }

        if (!isAttacking)
        {
            Debug.Log("[TRIGGER] ❌ Отмена: isAttacking = false");
            return;
        }

        // Защита от множественных попаданий за одну атаку
        if (hasHitThisAttack)
        {
            Debug.Log("[TRIGGER] ❌ Отмена: уже было попадание в этой атаке");
            return;
        }

        Debug.Log($"[TRIGGER] ✅ Попали в {other.gameObject.name}, тег: {other.tag}");

        // Проверяем что это враг
        if (other.CompareTag("Enemy"))
        {
            Health enemyHealth = other.GetComponent<Health>();

            // Если не нашли на этом объекте, ищем на родителе
            if (enemyHealth == null)
            {
                enemyHealth = other.GetComponentInParent<Health>();
                Debug.Log($"[HEALTH] Найден на родителе: {enemyHealth?.gameObject.name}");
            }

            if (enemyHealth != null && !enemyHealth.IsDead)
            {
                Debug.Log($"[DAMAGE] ⚔️ Наносим {damage} урона врагу {other.gameObject.name}");
                Debug.Log($"[DAMAGE] HP до: {enemyHealth.CurrentHealth}");
                enemyHealth.TakeDamage(damage);
                Debug.Log($"[DAMAGE] HP после: {enemyHealth.CurrentHealth}, IsDead: {enemyHealth.IsDead}");

                // Помечаем что было попадание и отключаем хитбокс
                hasHitThisAttack = true;
                DisableHitbox();
            }
            else
            {
                if (enemyHealth == null)
                    Debug.LogWarning($"[HEALTH] ❌ Health не найден на {other.gameObject.name}");
                else if (enemyHealth.IsDead)
                    Debug.Log($"[HEALTH] ℹ️ Враг уже мёртв!");
            }
        }
        else
        {
            Debug.Log($"[TRIGGER] ❌ Объект не с тегом Enemy: {other.tag}");
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
