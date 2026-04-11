using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Автоматически создаёт UI для системы взаимодействия при старте.
/// Просто повесь этот скрипт на пустой объект "InteractionUI".
/// </summary>
[RequireComponent(typeof(InteractionUI))]
public class InteractionUISetup : MonoBehaviour
{
    void Start()
    {
        InteractionUI ui = GetComponent<InteractionUI>();

        // Проверяем не назначен ли уже UI
        if (ui.promptPanel != null && ui.popupPanel != null)
        {
            // UI уже настроен вручную
            return;
        }

        // Создаём UI программно
        CreateInteractionUI(ui);
    }

    private void CreateInteractionUI(InteractionUI ui)
    {
        if (ui == null)
        {
            Debug.LogError("[InteractionUISetup] InteractionUI не найден!");
            return;
        }

        // Проверяем есть ли Canvas
        Canvas canvas = GetComponent<Canvas>();
        if (canvas == null)
        {
            canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 1000;
        }

        // Добавляем Graphic Raycaster
        if (GetComponent<GraphicRaycaster>() == null)
        {
            gameObject.AddComponent<GraphicRaycaster>();
        }

        // Добавляем Canvas Scaler
        CanvasScaler scaler = GetComponent<CanvasScaler>();
        if (scaler == null)
        {
            scaler = gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
        }

        // === СОЗДАЁМ ПОДСКАЗКУ [Y] внизу экрана ===
        GameObject promptObj = new GameObject("InteractionPrompt");
        promptObj.transform.SetParent(transform);
        promptObj.transform.localScale = Vector3.one;
        promptObj.SetActive(false);

        RectTransform promptRect = promptObj.AddComponent<RectTransform>();
        promptRect.anchorMin = new Vector2(0.5f, 0f);
        promptRect.anchorMax = new Vector2(0.5f, 0f);
        promptRect.pivot = new Vector2(0.5f, 0f);
        promptRect.anchoredPosition = new Vector2(0, 120f);
        promptRect.sizeDelta = new Vector2(250, 40);

        Image promptImage = promptObj.AddComponent<Image>();
        promptImage.color = new Color(0f, 0f, 0f, 0.7f);

        GameObject promptTextObj = new GameObject("Text");
        promptTextObj.transform.SetParent(promptObj.transform);
        promptTextObj.transform.localScale = Vector3.one;

        RectTransform promptTextRect = promptTextObj.AddComponent<RectTransform>();
        promptTextRect.anchorMin = Vector2.zero;
        promptTextRect.anchorMax = Vector2.one;
        promptTextRect.sizeDelta = Vector2.zero;

        TextMeshProUGUI promptText = promptTextObj.AddComponent<TextMeshProUGUI>();
        promptText.text = "[Y] Прочитать";
        promptText.alignment = TextAlignmentOptions.Center;
        promptText.fontSize = 20;
        promptText.color = new Color(0.9f, 0.9f, 0.9f, 1f);

        ui.promptPanel = promptObj;
        ui.promptText = promptText;

        // === СОЗДАЁМ ОКНО ТЕКСТА (внизу экрана) ===
        GameObject popupObj = new GameObject("TextPopup");
        popupObj.transform.SetParent(transform);
        popupObj.transform.localScale = Vector3.one;
        popupObj.SetActive(false);

        RectTransform popupRect = popupObj.AddComponent<RectTransform>();
        popupRect.anchorMin = new Vector2(0.5f, 0f);
        popupRect.anchorMax = new Vector2(0.5f, 0f);
        popupRect.pivot = new Vector2(0.5f, 0f);
        popupRect.anchoredPosition = new Vector2(0, 180f);
        popupRect.sizeDelta = new Vector2(600, 200);

        Image popupImage = popupObj.AddComponent<Image>();
        popupImage.color = new Color(0.05f, 0.05f, 0.05f, 0.85f);

        GameObject borderObj = new GameObject("Border");
        borderObj.transform.SetParent(popupObj.transform);
        borderObj.transform.localScale = Vector3.one;

        RectTransform borderRect = borderObj.AddComponent<RectTransform>();
        borderRect.anchorMin = Vector2.zero;
        borderRect.anchorMax = Vector2.one;
        borderRect.sizeDelta = new Vector2(-4, -4);

        Image borderImage = borderObj.AddComponent<Image>();
        borderImage.color = new Color(0.3f, 0.3f, 0.3f, 0.9f);

        GameObject titleObj = new GameObject("Title");
        titleObj.transform.SetParent(popupObj.transform);
        titleObj.transform.localScale = Vector3.one;

        RectTransform titleRect = titleObj.AddComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0f, 1f);
        titleRect.anchorMax = new Vector2(1f, 1f);
        titleRect.pivot = new Vector2(0.5f, 1f);
        titleRect.anchoredPosition = new Vector2(0, -20f);
        titleRect.sizeDelta = new Vector2(-30, 35);

        TextMeshProUGUI titleText = titleObj.AddComponent<TextMeshProUGUI>();
        titleText.text = "Заголовок";
        titleText.alignment = TextAlignmentOptions.Center;
        titleText.fontSize = 24;
        titleText.color = new Color(0.95f, 0.85f, 0.6f, 1f);
        titleText.fontStyle = FontStyles.Bold;

        GameObject contentObj = new GameObject("Content");
        contentObj.transform.SetParent(popupObj.transform);
        contentObj.transform.localScale = Vector3.one;

        RectTransform contentRect = contentObj.AddComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0f, 0f);
        contentRect.anchorMax = new Vector2(1f, 1f);
        contentRect.pivot = new Vector2(0.5f, 0.5f);
        contentRect.anchoredPosition = new Vector2(0, -10f);
        contentRect.sizeDelta = new Vector2(-40, -70);

        TextMeshProUGUI contentText = contentObj.AddComponent<TextMeshProUGUI>();
        contentText.text = "Текст подсказки...";
        contentText.alignment = TextAlignmentOptions.TopLeft;
        contentText.fontSize = 18;
        contentText.color = new Color(0.85f, 0.85f, 0.85f, 1f);
        contentText.textWrappingMode = TextWrappingModes.Normal;

        GameObject closeHintObj = new GameObject("CloseHint");
        closeHintObj.transform.SetParent(popupObj.transform);
        closeHintObj.transform.localScale = Vector3.one;

        RectTransform closeHintRect = closeHintObj.AddComponent<RectTransform>();
        closeHintRect.anchorMin = new Vector2(0.5f, 0f);
        closeHintRect.anchorMax = new Vector2(0.5f, 0f);
        closeHintRect.pivot = new Vector2(0.5f, 0f);
        closeHintRect.anchoredPosition = new Vector2(0, 15f);
        closeHintRect.sizeDelta = new Vector2(150, 25);

        TextMeshProUGUI closeHintText = closeHintObj.AddComponent<TextMeshProUGUI>();
        closeHintText.text = "[B] Закрыть";
        closeHintText.alignment = TextAlignmentOptions.Center;
        closeHintText.fontSize = 16;
        closeHintText.color = new Color(0.6f, 0.6f, 0.6f, 1f);

        ui.popupPanel = popupObj;
        ui.popupTitle = titleText;
        ui.popupContent = contentText;
        ui.popupCloseHint = closeHintText;

        Debug.Log("[InteractionUI] ✅ UI создан автоматически!");
    }
}
