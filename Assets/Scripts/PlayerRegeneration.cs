using UnityEngine;
using System;

/// <summary>
/// Система регенерации игрока с лимитами.
/// 5 использований, +1 каждые 15 секунд (последовательно, как в Dark Souls).
/// </summary>
public class PlayerRegeneration : MonoBehaviour
{
    [Header("Regeneration Settings")]
    [SerializeField] private int maxCharges = 5;
    [SerializeField] private float rechargeTime = 15f;
    [Tooltip("Процент от МАКСИМАЛЬНОГО HP, восстанавливаемый одним зарядом")]
    [SerializeField, Range(5f, 100f)] private float healPercent = 30f;

    [Header("References")]
    [SerializeField] private Health playerHealth;

    // Фляга подобрана?
    public bool HasFlask { get; private set; }

    // Текущие заряды
    private int currentCharges;
    private float[] queueTimers;  // Таймеры зарядов, ожидающих восстановления
    private int queueCount;       // Сколько зарядов сейчас в очереди

    private float baseRechargeTime;
    private bool deathPenaltyActive;

    // События для UI
    public event Action<int, int> OnChargesChanged;
    public event Action<float> OnChargeRecharged;

    public int CurrentCharges => currentCharges;
    public int MaxCharges => maxCharges;
    public float RechargeTime => rechargeTime;

    void Awake()
    {
        HasFlask = PlayerPrefs.GetInt("HasEstusFlask", 0) == 1;

        queueTimers = new float[maxCharges];
        queueCount = 0;
        currentCharges = HasFlask ? maxCharges : 0;
        baseRechargeTime = rechargeTime;
        deathPenaltyActive = false;

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
        if (!HasFlask) return;

        // Обновляем таймеры для перезарядки
        UpdateChargeTimers();
    }

    /// <summary>
    /// Попытка лечения.
    /// </summary>
    public void TryHeal()
    {
        if (!HasFlask) return;

        if (currentCharges <= 0)
        {
            return;
        }

        if (playerHealth.IsDead)
        {
            return;
        }

        if (playerHealth.CurrentHealth >= playerHealth.MaxHealth)
        {
            return;
        }

        // Рассчитываем лечение: фиксированный % от МАКСИМАЛЬНОГО HP
        float healAmount = playerHealth.MaxHealth * (healPercent / 100f);

        // Используем заряд
        UseCharge();

        // Лечим
        playerHealth.Heal(healAmount);
    }

    /// <summary>
    /// Использовать заряд и поставить его в очередь на восстановление.
    /// </summary>
    private void UseCharge()
    {
        if (currentCharges <= 0) return;

        currentCharges--;

        if (queueCount < maxCharges)
        {
            queueTimers[queueCount] = 0f;
            queueCount++;
        }

        OnChargesChanged?.Invoke(currentCharges, maxCharges);
    }

    /// <summary>
    /// Обновление таймеров перезарядки (только первый в очереди тикает).
    /// </summary>
    private void UpdateChargeTimers()
    {
        if (queueCount <= 0) return;

        queueTimers[0] += Time.deltaTime;

        if (queueTimers[0] >= rechargeTime)
        {
            queueTimers[0] = rechargeTime;
            currentCharges++;
            queueCount--;

            // Сдвигаем очередь
            for (int i = 0; i < queueCount; i++)
            {
                queueTimers[i] = queueTimers[i + 1];
            }
            if (queueCount < maxCharges)
            {
                queueTimers[queueCount] = 0f;
            }

            OnChargeRecharged?.Invoke(1f);
            OnChargesChanged?.Invoke(currentCharges, maxCharges);

            if (deathPenaltyActive && currentCharges >= maxCharges)
                ClearDeathPenalty();
        }
    }

    public void ApplyDeathPenalty()
    {
        if (deathPenaltyActive) return;
        deathPenaltyActive = true;
        rechargeTime = baseRechargeTime * 2f;
    }

    private void ClearDeathPenalty()
    {
        if (!deathPenaltyActive) return;
        deathPenaltyActive = false;
        rechargeTime = baseRechargeTime;
    }

    /// <summary>
    /// Получить прогресс перезарядки следующего заряда в очереди (0-1).
    /// </summary>
    public float GetNextChargeProgress()
    {
        if (queueCount > 0)
            return Mathf.Clamp01(queueTimers[0] / rechargeTime);
        return 1f;
    }

    /// <summary>
    /// Получить статус всех зарядов.
    /// </summary>
    public bool[] GetChargeStatus()
    {
        bool[] ready = new bool[maxCharges];
        for (int i = 0; i < currentCharges; i++)
            ready[i] = true;
        return ready;
    }

    /// <summary>
    /// Подобрать флягу (вызывается из EstusPickup).
    /// </summary>
    public void EnableFlask()
    {
        if (HasFlask) return;

        HasFlask = true;
        currentCharges = maxCharges;
        queueCount = 0;
        for (int i = 0; i < maxCharges; i++)
            queueTimers[i] = 0f;

        PlayerPrefs.SetInt("HasEstusFlask", 1);
        PlayerPrefs.Save();

        OnChargesChanged?.Invoke(currentCharges, maxCharges);
    }

    /// <summary>
    /// Установить конкретное количество готовых зарядов (без очереди).
    /// </summary>
    public void SetCharges(int count)
    {
        if (!HasFlask) return;
        currentCharges = Mathf.Clamp(count, 0, maxCharges);
        queueCount = 0;
        for (int i = 0; i < maxCharges; i++)
            queueTimers[i] = 0f;

        OnChargesChanged?.Invoke(currentCharges, maxCharges);
    }

    /// <summary>
    /// Установить готовые заряды и сразу поставить остальные в очередь восстановления.
    /// </summary>
    public void SetChargesWithQueue(int readyCount)
    {
        if (!HasFlask) return;
        currentCharges = Mathf.Clamp(readyCount, 0, maxCharges);
        queueCount = maxCharges - currentCharges;
        for (int i = 0; i < maxCharges; i++)
            queueTimers[i] = 0f;

        OnChargesChanged?.Invoke(currentCharges, maxCharges);
    }

    /// <summary>
    /// Сбросить все заряды (для респауна).
    /// </summary>
    public void ResetCharges()
    {
        currentCharges = HasFlask ? maxCharges : 0;
        queueCount = 0;
        for (int i = 0; i < maxCharges; i++)
            queueTimers[i] = 0f;

        ClearDeathPenalty();
        OnChargesChanged?.Invoke(currentCharges, maxCharges);
    }
}
