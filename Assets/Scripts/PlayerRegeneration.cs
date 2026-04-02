using UnityEngine;
using System;

/// <summary>
/// Система регенерации игрока с лимитами.
/// 5 использований, +1 каждые 15 секунд.
/// </summary>
public class PlayerRegeneration : MonoBehaviour
{
    [Header("Regeneration Settings")]
    [SerializeField] private int maxCharges = 5;
    [SerializeField] private float rechargeTime = 15f;
    [SerializeField] private float healPercent = 30f; // 30% от текущего HP

    [Header("References")]
    [SerializeField] private Health playerHealth;

    // Текущие заряды
    private int currentCharges;
    private float[] chargeTimers; // Таймер для каждого заряда
    private bool[] chargeReady;   // Готов ли каждый заряд

    // События для UI
    public event Action<int, int> OnChargesChanged;
    public event Action<float> OnChargeRecharged;

    public int CurrentCharges => currentCharges;
    public int MaxCharges => maxCharges;
    public float RechargeTime => rechargeTime;

    void Awake()
    {
        // Инициализация массивов
        chargeTimers = new float[maxCharges];
        chargeReady = new bool[maxCharges];

        // Все заряды готовы при старте
        for (int i = 0; i < maxCharges; i++)
        {
            chargeReady[i] = true;
        }

        currentCharges = maxCharges;

        // Если здоровье не назначено, ищем на себе
        if (playerHealth == null)
        {
            playerHealth = GetComponent<Health>();
        }

        if (playerHealth == null)
        {
            Debug.LogError("PlayerRegeneration: Не найден Health на игроке!");
            enabled = false;
        }
    }

    void Update()
    {
        // Обновляем таймеры для перезарядки
        UpdateChargeTimers();

        // Проверка нажатия кнопки лечения (H)
        if (Input.GetKeyDown(KeyCode.H))
        {
            TryHeal();
        }
    }

    /// <summary>
    /// Попытка лечения.
    /// </summary>
    public void TryHeal()
    {
        if (currentCharges <= 0)
        {
            Debug.LogWarning("PlayerRegeneration: Нет доступных зарядов!");
            return;
        }

        if (playerHealth.IsDead)
        {
            Debug.LogWarning("PlayerRegeneration: Игрок мёртв!");
            return;
        }

        if (playerHealth.CurrentHealth >= playerHealth.MaxHealth)
        {
            Debug.Log("PlayerRegeneration: Здоровье уже полное!");
            return;
        }

        // Рассчитываем лечение: 30% от текущего HP
        float healAmount = playerHealth.CurrentHealth * (healPercent / 100f);
        
        // Если HP очень мало, лечим хотя бы на 10% от максимума
        if (healAmount < playerHealth.MaxHealth * 0.1f)
        {
            healAmount = playerHealth.MaxHealth * 0.1f;
        }

        // Используем заряд
        UseCharge();

        // Лечим
        playerHealth.Heal(healAmount);
        Debug.Log($"PlayerRegeneration: Лечение на {healAmount:F1} (+{healPercent}%). HP: {playerHealth.CurrentHealth:F1}/{playerHealth.MaxHealth:F1}");
    }

    /// <summary>
    /// Использовать заряд.
    /// </summary>
    private void UseCharge()
    {
        if (currentCharges <= 0) return;

        currentCharges--;

        // Помечаем последний готовый заряд как использованный
        for (int i = maxCharges - 1; i >= 0; i--)
        {
            if (chargeReady[i])
            {
                chargeReady[i] = false;
                chargeTimers[i] = 0f;
                break;
            }
        }

        OnChargesChanged?.Invoke(currentCharges, maxCharges);
    }

    /// <summary>
    /// Обновление таймеров перезарядки.
    /// </summary>
    private void UpdateChargeTimers()
    {
        bool chargesChanged = false;

        for (int i = 0; i < maxCharges; i++)
        {
            if (!chargeReady[i])
            {
                chargeTimers[i] += Time.deltaTime;

                if (chargeTimers[i] >= rechargeTime)
                {
                    chargeReady[i] = true;
                    chargeTimers[i] = rechargeTime;
                    currentCharges++;
                    chargesChanged = true;

                    OnChargeRecharged?.Invoke(chargeTimers[i] / rechargeTime);
                    Debug.Log($"PlayerRegeneration: Заряд #{i + 1} восстановлен! Всего: {currentCharges}/{maxCharges}");
                }
            }
        }

        if (chargesChanged)
        {
            OnChargesChanged?.Invoke(currentCharges, maxCharges);
        }
    }

    /// <summary>
    /// Получить прогресс перезарядки следующего заряда (0-1).
    /// </summary>
    public float GetNextChargeProgress()
    {
        for (int i = 0; i < maxCharges; i++)
        {
            if (!chargeReady[i])
            {
                return chargeTimers[i] / rechargeTime;
            }
        }
        return 1f; // Все заряды готовы
    }

    /// <summary>
    /// Получить статус всех зарядов.
    /// </summary>
    public bool[] GetChargeStatus()
    {
        return (bool[])chargeReady.Clone();
    }

    /// <summary>
    /// Сбросить все заряды (для респауна).
    /// </summary>
    public void ResetCharges()
    {
        currentCharges = maxCharges;

        for (int i = 0; i < maxCharges; i++)
        {
            chargeReady[i] = true;
            chargeTimers[i] = 0f;
        }

        OnChargesChanged?.Invoke(currentCharges, maxCharges);
    }
}
