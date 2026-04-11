using UnityEngine;

/// <summary>
/// Подсказка/знак в стиле Dark Souls 3.
/// При приближении игрока показывает "[Y] Прочитать"
/// При нажатии — красивое окно с текстом.
/// </summary>
public class Signpost : InteractableObject
{
    [Header("Signpost Settings")]
    [TextArea(3, 10)]
    [SerializeField] private string signTitle = "Заброшенная записка";
    [TextArea(5, 20)]
    [SerializeField] private string signText = "Текст подсказки...";
    [SerializeField] private bool destroyAfterReading = false;

    public override void Interact()
    {
        isShowingPopup = true;

        // Показываем окно с текстом
        if (InteractionManager.Instance != null)
        {
            InteractionManager.Instance.ShowTextPopup(signTitle, signText);
            InteractionManager.Instance.HideInteractionPrompt();
        }
        else
        {
            Debug.LogWarning($"[{gameObject.name}] InteractionManager не найден!");
        }

        if (destroyAfterReading)
        {
            Destroy(gameObject, 0.5f);
        }
    }

    protected override void OnPlayerExitRange()
    {
        base.OnPlayerExitRange();

        // Если игрок ушёл — закрываем окно
        if (isShowingPopup)
        {
            ClosePopup();
        }
    }

    /// <summary>
    /// Закрыть окно текста
    /// </summary>
    public void ClosePopup()
    {
        isShowingPopup = false;
        if (InteractionManager.Instance != null)
        {
            InteractionManager.Instance.HideTextPopup();
        }
    }

    /// <summary>
    /// Вызывается при нажатии B для закрытия popup
    /// </summary>
    protected override void OnClosePopup()
    {
        ClosePopup();
    }
}
