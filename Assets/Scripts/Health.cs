using UnityEngine;
using System;

/// <summary>
/// Компонент здоровья для игрока и врагов.
/// Обрабатывает получение урона, лечение и смерть.
/// </summary>
public class Health : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private float baseMaxHealth = 100f;
    [SerializeField] private float currentHealth = -1f;
    [SerializeField] private bool isDead = false;

    public event Action<float, float> OnHealthChanged;
    public event Action OnDeath;
    public event Action OnDamageTaken;
    public event Func<float, float> OnModifyIncomingDamage;

    public float BaseMaxHealth => baseMaxHealth;
    public float MaxHealth => baseMaxHealth; // Используем baseMaxHealth напрямую
    public float CurrentHealth => currentHealth;
    public bool IsDead => isDead;

    void Awake()
    {
        // Принудительная инициализация
        if (currentHealth < 0 || currentHealth > baseMaxHealth)
        {
            currentHealth = baseMaxHealth;
        }
    }

    /// <summary>
    /// Получить урон.
    /// </summary>
    public void TakeDamage(float damage)
    {
        if (isDead) return;

        float modifiedDamage = OnModifyIncomingDamage?.Invoke(damage) ?? damage;
        modifiedDamage = Mathf.Max(1f, modifiedDamage);

        float oldHealth = currentHealth;
        currentHealth = Mathf.Max(0, currentHealth - modifiedDamage);

        // Вызываем событие получения урона
        OnDamageTaken?.Invoke();

        // Вызываем событие изменения HP
        OnHealthChanged?.Invoke(currentHealth, baseMaxHealth);

        // Проверяем смерть
        if (currentHealth <= 0 && oldHealth > 0)
        {
            Die();
        }
    }

    /// <summary>
    /// Восстановить здоровье.
    /// </summary>
    public void Heal(float amount)
    {
        if (isDead) return;

        float oldHealth = currentHealth;
        currentHealth = Mathf.Min(baseMaxHealth, currentHealth + amount);
        
        OnHealthChanged?.Invoke(currentHealth, baseMaxHealth);
    }

    /// <summary>
    /// Полное исцеление.
    /// </summary>
    public void FullHeal()
    {
        if (isDead) return;

        currentHealth = baseMaxHealth;
        OnHealthChanged?.Invoke(currentHealth, baseMaxHealth);
    }

    /// <summary>
    /// Установить конкретное значение HP (для респавна).
    /// </summary>
    public void SetCurrentHealth(float value)
    {
        currentHealth = Mathf.Clamp(value, 0f, baseMaxHealth);
        OnHealthChanged?.Invoke(currentHealth, baseMaxHealth);
    }

    /// <summary>
    /// Воскресить объект (сброс isDead + установка HP).
    /// </summary>
    public void Revive(float hp)
    {
        isDead = false;
        currentHealth = Mathf.Clamp(hp, 1f, baseMaxHealth);
        OnHealthChanged?.Invoke(currentHealth, baseMaxHealth);
    }

    /// <summary>
    /// Смерть объекта.
    /// </summary>
    private void Die()
    {
        isDead = true;
        OnDeath?.Invoke();
    }

    /// <summary>
    /// Восстановить здоровье (для респауна/теста).
    /// </summary>
    public void ResetHealth()
    {
        isDead = false;
        currentHealth = baseMaxHealth;
        OnHealthChanged?.Invoke(currentHealth, baseMaxHealth);
    }

    public void SetMaxHealth(float newMax)
    {
        float oldMax = baseMaxHealth;
        baseMaxHealth = Mathf.Max(1f, newMax);
        float diff = baseMaxHealth - oldMax;
        if (diff > 0)
            currentHealth = Mathf.Min(baseMaxHealth, currentHealth + diff);
        else
            currentHealth = Mathf.Min(currentHealth, baseMaxHealth);
        OnHealthChanged?.Invoke(currentHealth, baseMaxHealth);
    }

#if UNITY_EDITOR
    // Для отладки в редакторе
    void OnValidate()
    {
        if (currentHealth > baseMaxHealth)
            currentHealth = baseMaxHealth;
        if (currentHealth < 0)
            currentHealth = 0;
    }
#endif
}
