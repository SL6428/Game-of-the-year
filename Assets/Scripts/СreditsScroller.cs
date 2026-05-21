using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class CreditsScroller : MonoBehaviour
{
    [Header("Настройки титров")]
    public float scrollSpeed = 30f;
    public float startDelay = 2f;
    public float endDelay = 3f;

    [Header("Ссылки")]
    public RectTransform СreditsContent;     // ВНИМАНИЕ: кириллическая С — оставлено для совместимости со сценой
    public TextMeshProUGUI СreditsText;
    public RectTransform viewport;            // если null — берётся RectTransform у самого объекта

    private Vector2 startAnchoredPosition;
    private Coroutine scrollRoutine;

    void Awake()
    {
        if (viewport == null)
            viewport = GetComponent<RectTransform>();

        if (СreditsContent != null)
            startAnchoredPosition = СreditsContent.anchoredPosition;
    }

    void OnEnable()
    {
        if (СreditsContent == null || viewport == null) return;

        // Сброс позиции — на стартовую, сохранённую один раз в Awake
        СreditsContent.anchoredPosition = startAnchoredPosition;
        StartScrolling();
    }

    void OnDisable()
    {
        StopScrolling();
    }

    public void StartScrolling()
    {
        if (scrollRoutine != null) StopCoroutine(scrollRoutine);
        scrollRoutine = StartCoroutine(ScrollCredits());
    }

    public void StopScrolling()
    {
        if (scrollRoutine != null)
        {
            StopCoroutine(scrollRoutine);
            scrollRoutine = null;
        }
    }

    IEnumerator ScrollCredits()
    {
        yield return new WaitForSeconds(startDelay);

        // ВАЖНО: даём layout-системе досчитаться, иначе rect.height = 0 у только что включённого объекта
        Canvas.ForceUpdateCanvases();
        yield return null;

        float contentHeight = СreditsContent.rect.height;
        float viewportHeight = viewport.rect.height;

        // Прокручиваем содержимое снизу вверх, пока низ контента не уедет за верхнюю границу viewport
        float targetY = contentHeight + viewportHeight;

        while (СreditsContent.anchoredPosition.y < targetY)
        {
            Vector2 pos = СreditsContent.anchoredPosition;
            pos.y += scrollSpeed * Time.unscaledDeltaTime; // unscaled — чтобы пауза не ломала титры
            СreditsContent.anchoredPosition = pos;
            yield return null;
        }

        yield return new WaitForSeconds(endDelay);
        scrollRoutine = null;
    }
}
