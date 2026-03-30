using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Отображение полоски здоровья в UI.
/// </summary>
public class HealthUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image hpFillImage;
    [SerializeField] private TextMeshProUGUI hpText;
    [SerializeField] private bool showText = true;

    [Header("Health Reference")]
    [SerializeField] private Health playerHealth; // Перетащите вручную!

    [Header("Colors")]
    [SerializeField] private Color fullHealthColor = Color.green;
    [SerializeField] private Color lowHealthColor = Color.red;
    [SerializeField] private float lowHealthThreshold = 0.3f; // 30% HP

    [Header("Animation")]
    [SerializeField] private float smoothSpeed = 5f;

    private Health health;
    private float currentFillAmount;
    private float targetFillAmount;

    void Awake()
    {
        // Если ссылка не назначена, пробуем найти
        if (playerHealth == null)
        {
            playerHealth = FindObjectOfType<Health>();
        }
        
        if (playerHealth == null)
        {
            Debug.LogError("HealthUI: Не найден компонент Health на сцене! Назначьте вручную в инспекторе.");
            enabled = false;
            return;
        }

        health = playerHealth;

        // Подписываемся на события
        health.OnHealthChanged += UpdateHealthUI;
        health.OnDeath += OnPlayerDeath;
    }

    void Start()
    {
        // Инициализируем UI актуальными значениями
        currentFillAmount = health.CurrentHealth / health.MaxHealth;
        targetFillAmount = currentFillAmount;
        UpdateHealthUI(health.CurrentHealth, health.MaxHealth);
    }

    void OnDestroy()
    {
        if (health != null)
        {
            health.OnHealthChanged -= UpdateHealthUI;
            health.OnDeath -= OnPlayerDeath;
        }
    }

    void Update()
    {
        // Плавная анимация полоски
        if (currentFillAmount != targetFillAmount)
        {
            currentFillAmount = Mathf.MoveTowards(
                currentFillAmount, 
                targetFillAmount, 
                Time.deltaTime * smoothSpeed
            );

            if (hpFillImage != null)
                hpFillImage.fillAmount = currentFillAmount;

            UpdateColor(currentFillAmount);
        }
    }

    private void UpdateHealthUI(float currentHP, float maxHP)
    {
        Debug.Log($"HealthUI: Обновление HP {currentHP} / {maxHP}");
        
        targetFillAmount = currentHP / maxHP;

        if (showText && hpText != null)
        {
            hpText.text = $"{Mathf.CeilToInt(currentHP)} / {Mathf.CeilToInt(maxHP)}";
        }

        UpdateColor(targetFillAmount);
    }

    private void UpdateColor(float fillAmount)
    {
        if (hpFillImage == null) return;

        // Интерполяция цвета от зелёного к красному
        Color color = Color.Lerp(lowHealthColor, fullHealthColor, fillAmount);
        
        // Если HP ниже порога - всегда красный
        if (fillAmount <= lowHealthThreshold)
            color = lowHealthColor;

        hpFillImage.color = color;
    }

    private void OnPlayerDeath()
    {
        if (hpFillImage != null)
        {
            // Анимация смерти - полоска становится серой
            hpFillImage.color = Color.gray;
        }
        
        if (hpText != null)
        {
            hpText.text = "МЕРТВ";
        }
    }

    /// <summary>
    /// Показать полоску.
    /// </summary>
    public void Show()
    {
        gameObject.SetActive(true);
    }

    /// <summary>
    /// Скрыть полоску.
    /// </summary>
    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
