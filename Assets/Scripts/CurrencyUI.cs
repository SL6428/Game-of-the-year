using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class CurrencyUI : MonoBehaviour
{
    private static CurrencyUI _instance;

    private Canvas uiCanvas;
    private TextMeshProUGUI currencyText;
    private int targetAmount;
    private int displayedAmount = -1;
    private bool uiReady;
    private bool isMenuScene = false;

    void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);
        BuildText();
        uiReady = true;
        
        SceneManager.sceneLoaded += OnSceneLoaded;
        UpdateSceneState(SceneManager.GetActiveScene());
    }

    void OnDestroy()
    {
        if (_instance == this)
            _instance = null;
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        UpdateSceneState(scene);
        
        // Если загружается MainMenu - сбрасываем отображаемую валюту
        if (scene.name == "MainMenu" || scene.name == "Sinematic")
        {
            displayedAmount = -1;
            targetAmount = 0;
        }
    }
    
    private void UpdateSceneState(Scene scene)
    {
        isMenuScene = scene.name == "MainMenu" || scene.name == "Sinematic";
    }

    void Update()
    {
        if (uiCanvas != null)
            uiCanvas.enabled = !isMenuScene;

        if (isMenuScene || !uiReady || PlayerStats.Instance == null)
        {
            if (uiReady && !isMenuScene) PlayerStats.EnsureExists();
            return;
        }

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
                int linear = Mathf.Max(1, Mathf.CeilToInt(200f * Time.unscaledDeltaTime));
                int accel = Mathf.Max(1, Mathf.CeilToInt(Mathf.Abs(diff) * 2f * Time.unscaledDeltaTime));
                int step = Mathf.Max(linear, accel);
                if (Mathf.Abs(diff) <= step)
                    displayedAmount = targetAmount;
                else
                    displayedAmount += (int)Mathf.Sign(diff) * step;
            }

            currencyText.text = $"Души: {displayedAmount:N0}";
        }
    }

    private void BuildText()
    {
        if (!TryGetComponent(out uiCanvas))
        {
            uiCanvas = gameObject.AddComponent<Canvas>();
            uiCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            uiCanvas.sortingOrder = -10;

            CanvasScaler scaler = gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
        }

        TMP_FontAsset font = ResolveFont();

        GameObject textObj = new GameObject("CurrencyText");
        textObj.transform.SetParent(transform, false);

        RectTransform rt = textObj.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(1, 1);
        rt.anchorMax = new Vector2(1, 1);
        rt.pivot = new Vector2(1, 1);
        rt.anchoredPosition = new Vector2(-20, -20);
        rt.sizeDelta = new Vector2(300, 40);

        currencyText = textObj.AddComponent<TextMeshProUGUI>();
        currencyText.font = font;
        currencyText.fontSize = 28;
        currencyText.color = new Color(1f, 0.85f, 0.3f);
        currencyText.alignment = TextAlignmentOptions.Right;
        currencyText.raycastTarget = false;
        currencyText.fontStyle = FontStyles.Bold;
        currencyText.outlineColor = Color.black;
        currencyText.outlineWidth = 0.3f;
        currencyText.text = "Души: 0";
    }

    private TMP_FontAsset ResolveFont()
    {
        if (TMP_Settings.defaultFontAsset != null)
            return TMP_Settings.defaultFontAsset;

        TMP_FontAsset[] fonts = Resources.FindObjectsOfTypeAll<TMP_FontAsset>();
        if (fonts.Length > 0) return fonts[0];

        Debug.LogError("CurrencyUI: TMP font not found!");
        return null;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void AutoCreate()
    {
        if (_instance != null) return;
        GameObject obj = new GameObject("CurrencyCanvas");
        DontDestroyOnLoad(obj);
        obj.AddComponent<CurrencyUI>();
    }
}
