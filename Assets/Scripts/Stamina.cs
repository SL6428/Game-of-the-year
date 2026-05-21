using UnityEngine;
using System;

public class Stamina : MonoBehaviour
{
    [Header("Stamina Settings")]
    [SerializeField] private float maxStamina = 100f;
    [SerializeField] private float regenRate = 30f;
    [SerializeField] private float regenDelay = 0.5f;

    [Header("Action Costs")]
    [SerializeField] private float runDrainRate = 15f;
    [SerializeField] private float attackCost = 20f;

    public float MaxStamina => maxStamina;
    public float CurrentStamina => currentStamina;
    public bool IsExhausted => currentStamina <= 0f;
    public float AttackCost => attackCost;
    public float RunDrainRate => runDrainRate;
    public float RegenDelay => regenDelay;

    public event Action<float, float> OnStaminaChanged;

    private float currentStamina;
    private float timeSinceLastUse;

    void Awake()
    {
        currentStamina = maxStamina;
        timeSinceLastUse = regenDelay;
    }

    void Update()
    {
        if (currentStamina < maxStamina && timeSinceLastUse >= regenDelay)
        {
            currentStamina = Mathf.Min(maxStamina, currentStamina + regenRate * Time.deltaTime);
            OnStaminaChanged?.Invoke(currentStamina, maxStamina);
        }

        timeSinceLastUse += Time.deltaTime;
    }

    public bool TryUseStamina(float amount)
    {
        if (currentStamina < amount) return false;
        currentStamina -= amount;
        timeSinceLastUse = 0f;
        OnStaminaChanged?.Invoke(currentStamina, maxStamina);
        return true;
    }

    public void DrainContinuous(float rate)
    {
        currentStamina = Mathf.Max(0f, currentStamina - rate * Time.deltaTime);
        timeSinceLastUse = 0f;
        OnStaminaChanged?.Invoke(currentStamina, maxStamina);
    }

    public void SetMaxStamina(float newMax)
    {
        float oldMax = maxStamina;
        maxStamina = Mathf.Max(1f, newMax);
        float diff = maxStamina - oldMax;
        if (diff > 0)
            currentStamina = Mathf.Min(maxStamina, currentStamina + diff);
        else
            currentStamina = Mathf.Min(currentStamina, maxStamina);
        OnStaminaChanged?.Invoke(currentStamina, maxStamina);
    }
}
