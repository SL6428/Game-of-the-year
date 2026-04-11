using UnityEngine;

/// <summary>
/// Менеджер системы взаимодействия - один на сцену.
/// Управляет отображением подсказок и окон текста.
/// Разместить на пустом объекте "InteractionManager" на сцене.
/// </summary>
public class InteractionManager : MonoBehaviour
{
    public static InteractionManager Instance { get; private set; }

    [Header("References")]
    [SerializeField] private InteractionUI interactionUI;

    void Awake()
    {
        // Singleton паттерн
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        // Если UI не назначен - ищем или создаём (в Start когда всё готово)
        if (interactionUI == null)
        {
            SetupInteractionUI();
        }
    }

    /// <summary>
    /// Найти или создать UI для взаимодействия
    /// </summary>
    private void SetupInteractionUI()
    {
        // Ищем существующий UI
        InteractionUI[] existingUI = FindObjectsByType<InteractionUI>(FindObjectsSortMode.None);
        
        if (existingUI.Length > 0)
        {
            interactionUI = existingUI[0];
            Debug.Log("[InteractionManager] Найден существующий InteractionUI");
        }
        else
        {
            // Создаём новый объект с UI
            GameObject uiObj = new GameObject("InteractionUI");
            uiObj.transform.SetParent(transform);
            
            interactionUI = uiObj.AddComponent<InteractionUI>();
            
            // Добавляем автоматическую настройку
            uiObj.AddComponent<InteractionUISetup>();
            
            Debug.Log("[InteractionManager] Создан новый InteractionUI");
        }
    }

    /// <summary>
    /// Показать подсказку "Нажмите [кнопку]"
    /// </summary>
    public void ShowInteractionPrompt(string buttonText)
    {
        if (interactionUI != null)
        {
            interactionUI.ShowPrompt(buttonText);
        }
    }

    /// <summary>
    /// Скрыть подсказку "Нажмите [кнопку]"
    /// </summary>
    public void HideInteractionPrompt()
    {
        if (interactionUI != null)
        {
            interactionUI.HidePrompt();
        }
    }

    /// <summary>
    /// Показать окно с текстом подсказки
    /// </summary>
    public void ShowTextPopup(string title, string content)
    {
        if (interactionUI != null)
        {
            interactionUI.ShowTextPopup(title, content);
        }
    }

    /// <summary>
    /// Скрыть окно с текстом подсказки
    /// </summary>
    public void HideTextPopup()
    {
        if (interactionUI != null)
        {
            interactionUI.HideTextPopup();
        }
    }

    /// <summary>
    /// Назначить UI из инспектора или создать автоматически
    /// </summary>
    public void SetInteractionUI(InteractionUI ui)
    {
        interactionUI = ui;
    }
}
