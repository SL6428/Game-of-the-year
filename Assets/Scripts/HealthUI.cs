using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

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
            playerHealth = FindFirstObjectByType<Health>();
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
        
        SceneManager.sceneLoaded += OnSceneLoaded;
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
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Если загружается MainMenu - отписываемся от старого Health
        if (scene.name == "MainMenu" || scene.name == "Sinematic")
        {
            if (health != null)
            {
                health.OnHealthChanged -= UpdateHealthUI;
                health.OnDeath -= OnPlayerDeath;
            }
            health = null;
            playerHealth = null;
        }
        else
        {
            // В игровой сцене - переподключаемся к новому игроку
            if (playerHealth == null)
            {
                playerHealth = FindFirstObjectByType<Health>();
                if (playerHealth != null)
                {
                    health = playerHealth;
                    health.OnHealthChanged += UpdateHealthUI;
                    health.OnDeath += OnPlayerDeath;
                    
                    currentFillAmount = health.CurrentHealth / health.MaxHealth;
                    targetFillAmount = currentFillAmount;
                    UpdateHealthUI(health.CurrentHealth, health.MaxHealth);
                }
            }
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
