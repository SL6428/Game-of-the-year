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
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float currentHealth = -1f;
    [SerializeField] private bool isDead = false;

    public event Action<float, float> OnHealthChanged;
    public event Action OnDeath;
    public event Action OnDamageTaken;
    public event Func<float, float> OnModifyIncomingDamage;

    public float BaseMaxHealth => baseMaxHealth;
    public float MaxHealth => maxHealth;
    public float CurrentHealth => currentHealth;
    public bool IsDead => isDead;

    void Awake()
    {
        // Принудительная инициализация
        if (currentHealth < 0 || currentHealth > maxHealth)
        {
            currentHealth = maxHealth;
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

        // Выводим информацию о полученном уроне
        float healthPercent = (currentHealth / maxHealth) * 100f;
        Debug.Log($"[{gameObject.name}] DMG: {damage:F0}(raw) {modifiedDamage:F0}(final) | HP: {oldHealth:F0} -> {currentHealth:F0}");

        // Вызываем событие получения урона
        OnDamageTaken?.Invoke();

        // Вызываем событие изменения HP
        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        // Проверяем смерть
        if (currentHealth <= 0 && oldHealth > 0)
        {
            Debug.Log($"💀 [{gameObject.name}] Умер! HP: {oldHealth:F0} → 0 (0%)");
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
        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        
        float healthPercent = (currentHealth / maxHealth) * 100f;
        Debug.Log($"💚 [{gameObject.name}] Лечение +{amount:F0} HP | HP: {oldHealth:F0} → {currentHealth:F0} ({healthPercent:F0}%)");
        
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    /// <summary>
    /// Полное исцеление.
    /// </summary>
    public void FullHeal()
    {
        if (isDead) return;

        currentHealth = maxHealth;
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    /// <summary>
    /// Установить конкретное значение HP (для респавна).
    /// </summary>
    public void SetCurrentHealth(float value)
    {
        currentHealth = Mathf.Clamp(value, 0f, maxHealth);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    /// <summary>
    /// Воскресить объект (сброс isDead + установка HP).
    /// </summary>
    public void Revive(float hp)
    {
        isDead = false;
        currentHealth = Mathf.Clamp(hp, 1f, maxHealth);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
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
        currentHealth = maxHealth;
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    public void SetMaxHealth(float newMax)
    {
        float oldMax = maxHealth;
        maxHealth = Mathf.Max(1f, newMax);
        float diff = maxHealth - oldMax;
        if (diff > 0)
            currentHealth = Mathf.Min(maxHealth, currentHealth + diff);
        else
            currentHealth = Mathf.Min(currentHealth, maxHealth);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

#if UNITY_EDITOR
    // Для отладки в редакторе
    void OnValidate()
    {
        if (currentHealth > maxHealth)
            currentHealth = maxHealth;
        if (currentHealth < 0)
            currentHealth = 0;
    }
#endif
}
