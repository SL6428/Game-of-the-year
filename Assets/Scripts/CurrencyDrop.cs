using UnityEngine;

[RequireComponent(typeof(Health))]
public class CurrencyDrop : MonoBehaviour
{
    [Header("Currency")]
    [SerializeField] private int soulValue = 50;

    private Health health;

    void Start()
    {
        health = GetComponent<Health>();
        if (health != null)
            health.OnDeath += OnEnemyDeath;
    }

    void OnDestroy()
    {
        if (health != null)
            health.OnDeath -= OnEnemyDeath;
    }

    private void OnEnemyDeath()
    {
        if (PlayerStats.Instance != null)
            PlayerStats.Instance.AddCurrency(soulValue);
    }
}
