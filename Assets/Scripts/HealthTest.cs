using UnityEngine;

/// <summary>
/// Тестовый скрипт для проверки системы здоровья.
/// Нажмите T - получить урон, H - лечиться.
/// </summary>
public class HealthTest : MonoBehaviour
{
    [Header("Test Settings")]
    [SerializeField] private float testDamage = 10f;
    [SerializeField] private float testHeal = 10f;

    void Update()
    {
        Health health = GetComponent<Health>();
        if (health == null) return;

        // T - получить урон
        if (Input.GetKeyDown(KeyCode.T))
        {
            health.TakeDamage(testDamage);
            Debug.Log($"{gameObject.name} получил {testDamage} урона. HP: {health.CurrentHealth}/{health.MaxHealth}");
        }

        // H - лечиться
        if (Input.GetKeyDown(KeyCode.H))
        {
            health.Heal(testHeal);
            Debug.Log($"{gameObject.name} вылечился на {testHeal}. HP: {health.CurrentHealth}/{health.MaxHealth}");
        }

        // R - сброс
        if (Input.GetKeyDown(KeyCode.R))
        {
            health.ResetHealth();
            Debug.Log($"{gameObject.name} восстановил здоровье.");
        }
    }
}
