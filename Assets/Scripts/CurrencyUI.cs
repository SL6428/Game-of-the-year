using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CurrencyUI : MonoBehaviour
{
    private static CurrencyUI _instance;

    private TextMeshProUGUI currencyText;
    private int targetAmount;
    private int displayedAmount = -1;
    private bool uiReady;

    public static void EnsureExists()
    {
        if (_instance != null) return;

        GameObject obj = new GameObject("CurrencyCanvas");
        DontDestroyOnLoad(obj);

        Canvas canvas = obj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 50;

        CanvasScaler scaler = obj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        _instance = obj.AddComponent<CurrencyUI>();
    }

    void Start()
    {
        BuildPanel();
        uiReady = true;
    }

    void Update()
    {
        if (!uiReady || PlayerStats.Instance == null) return;

        int actual = PlayerStats.Instance.Currency;
        if (actual != targetAmount)
            targetAmount = actual;

        if (displayedAmount != targetAmount)
        {
            if (displayedAmount < 0)
            {
                displayedAmount = targetAmount;
            }
            else
            {
                int diff = targetAmount - displayedAmount;
                int step = Mathf.Max(1, Mathf.CeilToInt(200f * Time.unscaledDeltaTime));
                if (Mathf.Abs(diff) <= step)
                    displayedAmount = targetAmount;
                else
                    displayedAmount += (int)Mathf.Sign(diff) * step;
            }

            currencyText.text = displayedAmount.ToString("N0");
        }
    }

    private void BuildPanel()
    {
        GameObject panelObj = new GameObject("CurrencyPanel");
        panelObj.transform.SetParent(transform, false);

        RectTransform panelRt = panelObj.AddComponent<RectTransform>();
        panelRt.anchorMin = new Vector2(1, 1);
        panelRt.anchorMax = new Vector2(1, 1);
        panelRt.pivot = new Vector2(1, 1);
        panelRt.anchoredPosition = new Vector2(-20, -20);
        panelRt.sizeDelta = new Vector2(250, 50);

        Image bg = panelObj.AddComponent<Image>();
        bg.color = new Color(0, 0, 0, 0.5f);
        bg.raycastTarget = false;

        GameObject textObj = new GameObject("CurrencyText");
        textObj.transform.SetParent(panelObj.transform, false);

        RectTransform textRt = textObj.AddComponent<RectTransform>();
        textRt.anchorMin = Vector2.zero;
        textRt.anchorMax = Vector2.one;
        textRt.offsetMin = new Vector2(10, 5);
        textRt.offsetMax = new Vector2(-10, -5);

        currencyText = textObj.AddComponent<TextMeshProUGUI>();
        currencyText.fontSize = 28;
        currencyText.color = new Color(1f, 0.85f, 0.3f);
        currencyText.alignment = TextAlignmentOptions.Right;
        currencyText.raycastTarget = false;
        currencyText.fontStyle = FontStyles.Bold;
        currencyText.text = "0";
    }
}