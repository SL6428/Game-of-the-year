using UnityEngine;

/// <summary>
/// Подсказка/знак. Перетащи свои UI панели в инспектор.
/// </summary>
public class Signpost : InteractableObject
{
    [Header("UI References")]
    [Tooltip("Панель подсказки '[Y] Прочитать' (появляется внизу экрана)")]
    [SerializeField] private GameObject promptPanel;

    [Tooltip("Панель окна с текстом подсказки")]
    [SerializeField] private GameObject popupPanel;

    [Header("Signpost Settings")]
    [SerializeField] private bool destroyAfterReading = false;

    void Awake()
    {
        ValidatePanels();

        promptPanel?.SetActive(false);
        popupPanel?.SetActive(false);
    }

    protected override void ShowPopupUI()
    {
        promptPanel?.SetActive(false);
        popupPanel?.SetActive(true);

        // Диагностика: если popupPanel не стал активен — родительский объект выключен
        if (popupPanel != null && !popupPanel.activeInHierarchy && popupPanel.activeSelf)
        {
            Debug.LogWarning(
                $"[Signpost '{gameObject.name}'] popupPanel '{popupPanel.name}' включён (activeSelf), " +
                "но не виден (activeInHierarchy = false). Проверь, что его родительский контейнер в Hierarchy тоже активен.",
                popupPanel);
        }
    }

    protected override void HidePopupUI()
    {
        popupPanel?.SetActive(false);

        if (isPlayerInRange)
            promptPanel?.SetActive(true);

        if (destroyAfterReading)
            Destroy(gameObject, 0.5f);
    }

    protected override void OnPlayerEnterRange()
    {
        promptPanel?.SetActive(true);
    }

    protected override void OnPlayerExitRange()
    {
        promptPanel?.SetActive(false);
    }

    private void ValidatePanels()
    {
        if (promptPanel == null)
            Debug.LogError($"[Signpost '{gameObject.name}'] promptPanel не назначен в Inspector!", this);

        if (popupPanel == null)
            Debug.LogError($"[Signpost '{gameObject.name}'] popupPanel не назначен в Inspector!", this);

        if (promptPanel == popupPanel && promptPanel != null)
            Debug.LogError($"[Signpost '{gameObject.name}'] promptPanel и popupPanel ссылаются на один и тот же объект!", this);

        if (popupPanel != null && promptPanel != null && popupPanel.transform.IsChildOf(promptPanel.transform))
            Debug.LogError($"[Signpost '{gameObject.name}'] popupPanel находится внутри promptPanel — это сломает переключение! Вынь popupPanel из promptPanel в Hierarchy.", this);
    }
}
