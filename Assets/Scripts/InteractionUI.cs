using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// UI для системы взаимодействия в стиле Dark Souls 3.
/// Управляет отображением подсказки "[Y] Прочитать" и окна с текстом.
/// </summary>
public class InteractionUI : MonoBehaviour
{
    [Header("Prompt UI")]
    public GameObject promptPanel;
    public TextMeshProUGUI promptText;

    [Header("Text Popup UI")]
    public GameObject popupPanel;
    public TextMeshProUGUI popupTitle;
    public TextMeshProUGUI popupContent;
    public TextMeshProUGUI popupCloseHint;

    [Header("Animation Settings")]
    [SerializeField] private float fadeInSpeed = 8f;
    [SerializeField] private float fadeOutSpeed = 10f;

    private CanvasGroup promptCanvasGroup;
    private CanvasGroup popupCanvasGroup;
    private bool isPromptVisible = false;
    private bool isPopupVisible = false;

    void Awake()
    {
        // Создаём CanvasGroup если нет
        if (promptPanel != null)
        {
            promptCanvasGroup = promptPanel.GetComponent<CanvasGroup>();
            if (promptCanvasGroup == null)
                promptCanvasGroup = promptPanel.AddComponent<CanvasGroup>();
            
            // Скрываем подсказку по умолчанию
            promptPanel.SetActive(false);
            promptCanvasGroup.alpha = 0f;
        }

        if (popupPanel != null)
        {
            popupCanvasGroup = popupPanel.GetComponent<CanvasGroup>();
            if (popupCanvasGroup == null)
                popupCanvasGroup = popupPanel.AddComponent<CanvasGroup>();
            
            // Скрываем попап по умолчанию
            popupPanel.SetActive(false);
            popupCanvasGroup.alpha = 0f;
        }
    }

    void Update()
    {
        // Плавное появление/исчезновение подсказки
        if (promptCanvasGroup != null)
        {
            float targetAlpha = isPromptVisible ? 1f : 0f;
            float speed = isPromptVisible ? fadeInSpeed : fadeOutSpeed;
            promptCanvasGroup.alpha = Mathf.MoveTowards(promptCanvasGroup.alpha, targetAlpha, Time.deltaTime * speed);
            
            // Деактивируем когда почти прозрачная
            if (!isPromptVisible && promptCanvasGroup.alpha < 0.01f && promptPanel.activeSelf)
            {
                promptCanvasGroup.alpha = 0f;
                promptPanel.SetActive(false);
            }
        }

        // Плавное появление/исчезновение попапа
        if (popupCanvasGroup != null)
        {
            float targetAlpha = isPopupVisible ? 1f : 0f;
            float speed = isPopupVisible ? fadeInSpeed : fadeOutSpeed;
            popupCanvasGroup.alpha = Mathf.MoveTowards(popupCanvasGroup.alpha, targetAlpha, Time.deltaTime * speed);
            
            // Деактивируем когда почти прозрачный
            if (!isPopupVisible && popupCanvasGroup.alpha < 0.01f && popupPanel.activeSelf)
            {
                popupCanvasGroup.alpha = 0f;
                popupPanel.SetActive(false);
            }
        }
    }

    /// <summary>
    /// Показать подсказку "[Y] Прочитать"
    /// </summary>
    public void ShowPrompt(string buttonText)
    {
        if (promptText != null)
        {
            promptText.text = buttonText;
        }

        if (promptPanel != null)
        {
            promptPanel.SetActive(true);
            if (promptCanvasGroup != null)
                promptCanvasGroup.alpha = 0f;
        }

        isPromptVisible = true;
    }

    /// <summary>
    /// Скрыть подсказку
    /// </summary>
    public void HidePrompt()
    {
        isPromptVisible = false;
    }

    /// <summary>
    /// Показать окно с текстом (стиль Dark Souls 3)
    /// </summary>
    public void ShowTextPopup(string title, string content)
    {
        if (popupTitle != null)
            popupTitle.text = title;

        if (popupContent != null)
            popupContent.text = content;

        if (popupCloseHint != null)
            popupCloseHint.text = "[B] Закрыть";

        if (popupPanel != null)
        {
            popupPanel.SetActive(true);
            if (popupCanvasGroup != null)
                popupCanvasGroup.alpha = 0f;
        }

        isPopupVisible = true;
    }

    /// <summary>
    /// Скрыть окно с текстом
    /// </summary>
    public void HideTextPopup()
    {
        isPopupVisible = false;
    }
}
