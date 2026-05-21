using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Полоска здоровья над врагом.
/// Отображается только когда враг получает урон.
/// </summary>
[RequireComponent(typeof(Health))]
public class HealthUIEnemy : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private Image hpFillImage;
    [SerializeField] private bool hideWhenFull = true;

    [Header("Settings")]
    [SerializeField] private float fadeDelay = 2f;
    [SerializeField] private float fadeSpeed = 5f;

    private Health health;
    private float fadeTimer = 0f;
    private bool isDamaged = false;
    private bool shouldBeVisible = false;

    void Start()
    {
        // Получаем Health
        health = GetComponent<Health>();
        
        if (health == null)
        {
            Debug.LogError("HealthUIEnemy: Не найден Health на этом объекте!");
            enabled = false;
            return;
        }

        // Подписываемся на события
        health.OnHealthChanged += UpdateHealthUI;
        health.OnDamageTaken += OnDamageTaken;
        health.OnDeath += OnDeath;

        // Создаём UI если не назначен
        if (canvasGroup == null)
            CreateUI();

        // Инициализация
        UpdateHealthUI(health.CurrentHealth, health.MaxHealth);
    }

    void OnDestroy()
    {
        if (health != null)
        {
            health.OnHealthChanged -= UpdateHealthUI;
            health.OnDamageTaken -= OnDamageTaken;
            health.OnDeath -= OnDeath;
        }
    }

    void Update()
    {
        if (canvasGroup == null) return;

        // Логика исчезновения полоски
        if (hideWhenFull && !isDamaged)
        {
            fadeTimer += Time.deltaTime;
            
            if (fadeTimer >= fadeDelay)
            {
                shouldBeVisible = false;
            }
        }

        // Плавное появление/исчезновение
        float targetAlpha = shouldBeVisible ? 1f : 0f;
        canvasGroup.alpha = Mathf.MoveTowards(canvasGroup.alpha, targetAlpha, Time.deltaTime * fadeSpeed);

        // Если полностью прозрачный - отключаем
        canvasGroup.interactable = canvasGroup.alpha > 0.1f;
        canvasGroup.blocksRaycasts = canvasGroup.alpha > 0.1f;
    }

    private void UpdateHealthUI(float currentHP, float maxHP)
    {
        if (hpFillImage == null) return;

        float fillAmount = Mathf.Clamp01(currentHP / maxHP);
        hpFillImage.fillAmount = fillAmount;

        // Плавный градиент: красный (0) → оранжевый (0.5) → жёлтый (0.75) → зелёный (1.0)
        Color low    = new Color(0.78f, 0.10f, 0.10f); // тёмно-красный
        Color mid    = new Color(0.95f, 0.55f, 0.15f); // оранжевый
        Color high   = new Color(0.30f, 0.78f, 0.20f); // зелёный

        Color result;
        if (fillAmount < 0.5f)
            result = Color.Lerp(low, mid, fillAmount * 2f);
        else
            result = Color.Lerp(mid, high, (fillAmount - 0.5f) * 2f);

        hpFillImage.color = result;
    }

    private void OnDamageTaken()
    {
        isDamaged = true;
        fadeTimer = 0f;
        shouldBeVisible = true;

        // Показываем полоску на 2 секунды после урона
        Invoke(nameof(HideAfterDelay), fadeDelay);
    }

    private void HideAfterDelay()
    {
        if (hideWhenFull && health.CurrentHealth >= health.MaxHealth)
        {
            shouldBeVisible = false;
        }
    }

    private void OnDeath()
    {
        shouldBeVisible = false;
    }

    /// <summary>
    /// Создать UI программно.
    /// </summary>
    private void CreateUI()
    {
        // Проверяем не создан ли уже UI
        if (canvasGroup != null)
        {
            Debug.LogWarning("HealthUIEnemy: UI уже создан!");
            return;
        }

        // Проверяем нет ли уже дочернего Canvas
        Canvas existingCanvas = GetComponentInChildren<Canvas>();
        if (existingCanvas != null)
        {
            Debug.LogWarning("HealthUIEnemy: Canvas уже существует на дочернем объекте!");
            canvasGroup = existingCanvas.GetComponent<CanvasGroup>();
            hpFillImage = existingCanvas.GetComponentInChildren<Image>();
            return;
        }

        // Создаём Canvas (мировой)
        GameObject canvasObj = new GameObject("EnemyHP_Canvas");
        canvasObj.transform.SetParent(transform);
        canvasObj.transform.localPosition = new Vector3(0, 2f, 0); // Над головой
        canvasObj.transform.localRotation = Quaternion.identity;

        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.worldCamera = Camera.main;

        // Canvas Group для прозрачности
        canvasGroup = canvasObj.AddComponent<CanvasGroup>();

        // Rect
        RectTransform rect = canvasObj.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(3f, 0.45f); // было (2f, 0.3f) — увеличено для читаемости

        // Фон полоски
        GameObject bgObj = new GameObject("Background");
        bgObj.transform.SetParent(canvasObj.transform);
        Image bgImage = bgObj.AddComponent<Image>();
        bgImage.color = new Color(0.05f, 0.05f, 0.05f, 0.85f); // было (0,0,0,0.5)

        RectTransform bgRect = bgObj.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.sizeDelta = Vector2.zero;

        // Полоска HP
        GameObject hpObj = new GameObject("HP_Fill");
        hpObj.transform.SetParent(bgObj.transform);
        hpFillImage = hpObj.AddComponent<Image>();
        hpFillImage.color = Color.green;
        hpFillImage.type = Image.Type.Filled;
        hpFillImage.fillMethod = Image.FillMethod.Horizontal;

        RectTransform hpRect = hpObj.GetComponent<RectTransform>();
        hpRect.anchorMin = Vector2.zero;
        hpRect.anchorMax = Vector2.one;
        hpRect.sizeDelta = Vector2.zero;
    }

    /// <summary>
    /// Показать полоску.
    /// </summary>
    public void Show()
    {
        shouldBeVisible = true;
    }

    /// <summary>
    /// Скрыть полоску.
    /// </summary>
    public void Hide()
    {
        shouldBeVisible = false;
    }
}
