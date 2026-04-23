using UnityEngine;

/// <summary>
/// Менеджер UI сообщений. Один на сцену.
/// Показывает сообщения о подборе ключей и т.д.
/// </summary>
public class UIMessageManager : MonoBehaviour
{
    public static UIMessageManager Instance { get; private set; }

    [Header("References")]
    [SerializeField] private GameObject messagePanel;
    [SerializeField] private UnityEngine.UI.Text messageText;
    [Header("Settings")]
    [Tooltip("Сколько секунд показывать сообщение")]
    [SerializeField] private float displayDuration = 2f;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // Скрываем сообщение по умолчанию
        if (messagePanel != null)
        {
            messagePanel.SetActive(false);
        }
    }

    /// <summary>
    /// Показать сообщение
    /// </summary>
    public void ShowMessage(string text)
    {
        if (messagePanel == null)
        {
            Debug.LogWarning($"[UIMessageManager] messagePanel не назначен!");
            return;
        }

        // Отменяем предыдущее скрытие
        CancelInvoke(nameof(HideMessage));

        if (messageText != null)
            messageText.text = text;

        messagePanel.SetActive(true);

        // Скрываем через displayDuration
        Invoke(nameof(HideMessage), displayDuration);
    }

    void HideMessage()
    {
        if (messagePanel != null)
        {
            messagePanel.SetActive(false);
        }
    }
}
