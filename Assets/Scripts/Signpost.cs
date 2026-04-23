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
        // Скрываем UI по умолчанию
        if (promptPanel != null) promptPanel.SetActive(false);
        if (popupPanel != null) popupPanel.SetActive(false);
    }

    protected override void ShowPopupUI()
    {
        // Скрываем подсказку [Y], показываем текст
        if (promptPanel != null) promptPanel.SetActive(false);
        if (popupPanel != null) popupPanel.SetActive(true);
    }

    protected override void HidePopupUI()
    {
        // Скрываем текст
        if (popupPanel != null) popupPanel.SetActive(false);
        
        // Если всё ещё в радиусе - показываем подсказку [Y]
        if (isPlayerInRange && promptPanel != null)
        {
            promptPanel.SetActive(true);
        }

        if (destroyAfterReading)
        {
            Destroy(gameObject, 0.5f);
        }
    }

    protected override void OnPlayerEnterRange()
    {
        // Показываем подсказку [Y]
        if (promptPanel != null) promptPanel.SetActive(true);
    }

    protected override void OnPlayerExitRange()
    {
        // Скрываем подсказку [Y]
        if (promptPanel != null) promptPanel.SetActive(false);
    }
}
