using UnityEngine;
using TMPro;
using System.Collections;

public class CreditsScroller : MonoBehaviour
{
    [Header("Настройки титров")]
    public float scrollSpeed = 30f;
    public float startDelay = 2f;
    public float endDelay = 3f;

    [Header("Ссылки")]
    public RectTransform СreditsContent;
    public TextMeshProUGUI СreditsText;

    private Vector3 startPosition;
    private float creditsHeight;

    void Start()
    {
        // Сохраняем начальную позицию
        startPosition = СreditsContent.anchoredPosition;

        // Рассчитываем высоту контента
        creditsHeight = СreditsContent.rect.height;

        // Начинаем прокрутку при включении
        StartScrolling();
    }

    void OnEnable()
    {
        // Сбрасываем позицию при каждом открытии
        if (СreditsContent != null)
        {
            СreditsContent.anchoredPosition = startPosition;
            StartScrolling();
        }
    }

    public void StartScrolling()
    {
        StopAllCoroutines();
        StartCoroutine(ScrollCredits());
    }

    IEnumerator ScrollCredits()
    {
        // Задержка перед началом
        yield return new WaitForSeconds(startDelay);

        float panelHeight = GetComponent<RectTransform>().rect.height;
        float targetY = creditsHeight + panelHeight;

        // Прокрутка
        while (СreditsContent.anchoredPosition.y < targetY)
        {
            Vector3 pos = СreditsContent.anchoredPosition;
            pos.y += scrollSpeed * Time.deltaTime;
            СreditsContent.anchoredPosition = pos;
            yield return null;
        }

        // Задержка в конце
        yield return new WaitForSeconds(endDelay);

        // Автоматическое закрытие (опционально)
        // ReturnToMainMenu();
    }

    public void StopScrolling()
    {
        StopAllCoroutines();
    }
}