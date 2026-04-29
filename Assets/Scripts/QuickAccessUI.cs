using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class QuickAccessUI : MonoBehaviour
{
    private static QuickAccessUI _instance;
    private PlayerRegeneration regen;

    [Header("Sprites")]
    [SerializeField] private Sprite colorSprite;
    [SerializeField] private Sprite graySprite;

    [Header("Size")]
    [SerializeField] private float iconSize = 192f;

    private Image colorIcon;
    private Image grayOverlay;
    private TextMeshProUGUI badgeText;
    private Canvas uiCanvas;

    private readonly Color grayTint = new Color(0.35f, 0.35f, 0.35f, 1f);

    private int lastFrameCharges = -1;
    private float pulseTimer = 0f;
    private const float PULSE_DURATION = 0.3f;
    private float reconnectTimer = 0f;
    private const float RECONNECT_INTERVAL = 0.5f;

    void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);
        BuildUI();
        uiCanvas = GetComponent<Canvas>();
        if (uiCanvas != null) uiCanvas.enabled = false;
    }

    void OnDestroy()
    {
        if (_instance == this)
            _instance = null;
    }

    void Update()
    {
        if (regen == null)
        {
            reconnectTimer -= Time.unscaledDeltaTime;
            if (reconnectTimer <= 0f)
            {
                regen = FindFirstObjectByType<PlayerRegeneration>();
                reconnectTimer = RECONNECT_INTERVAL;
            }
            if (regen == null) return;
        }

        if (uiCanvas != null)
        {
            if (!regen.HasFlask)
            {
                uiCanvas.enabled = false;
                return;
            }
            if (!uiCanvas.enabled)
                uiCanvas.enabled = true;
        }

        int current = regen.CurrentCharges;
        int max = regen.MaxCharges;
        float progress = regen.GetNextChargeProgress();

        badgeText.text = current.ToString();

        // Pulse-анимация при восстановлении заряда
        if (current > lastFrameCharges && lastFrameCharges >= 0)
        {
            pulseTimer = PULSE_DURATION;
        }
        lastFrameCharges = current;

        if (pulseTimer > 0f)
        {
            pulseTimer -= Time.unscaledDeltaTime;
            float t = 1f - Mathf.Clamp01(pulseTimer / PULSE_DURATION); // 0..1
            float scale = 1f + Mathf.Sin(t * Mathf.PI) * 0.5f;        // пик 1.5 в середине
            badgeText.rectTransform.localScale = Vector3.one * scale;
        }
        else
        {
            badgeText.rectTransform.localScale = Vector3.one;
        }

        if (current >= max)
        {
            grayOverlay.fillAmount = 0f;
        }
        else
        {
            grayOverlay.fillAmount = 1f - progress;
        }
    }

    private void BuildUI()
    {
        Canvas canvas = gameObject.GetComponent<Canvas>();
        if (canvas == null)
        {
            canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = -10; // ниже всех окон (меню перекрывают)

            CanvasScaler scaler = gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
        }

        float containerSize = iconSize + 48f; // запас для badge

        GameObject container = new GameObject("QuickAccessContainer");
        container.transform.SetParent(transform, false);
        RectTransform cRt = container.AddComponent<RectTransform>();
        cRt.anchorMin = new Vector2(0, 0);
        cRt.anchorMax = new Vector2(0, 0);
        cRt.pivot = new Vector2(0, 0);
        cRt.anchoredPosition = new Vector2(40, 40);
        cRt.sizeDelta = new Vector2(containerSize, containerSize);

        // Цветная иконка (нижний слой)
        GameObject colorObj = new GameObject("ColorIcon");
        colorObj.transform.SetParent(container.transform, false);
        RectTransform colRt = colorObj.AddComponent<RectTransform>();
        colRt.anchorMin = Vector2.zero;
        colRt.anchorMax = Vector2.zero;
        colRt.pivot = new Vector2(0, 0);
        colRt.anchoredPosition = Vector2.zero;
        colRt.sizeDelta = new Vector2(iconSize, iconSize);

        colorIcon = colorObj.AddComponent<Image>();
        colorIcon.sprite = colorSprite != null ? colorSprite : CreateCircleSprite((int)iconSize, true);
        colorIcon.color = Color.white;

        // Серый оверлей (верхний слой, радиальное исчезновение против часовой)
        GameObject grayObj = new GameObject("GrayOverlay");
        grayObj.transform.SetParent(container.transform, false);
        RectTransform grayRt = grayObj.AddComponent<RectTransform>();
        grayRt.anchorMin = Vector2.zero;
        grayRt.anchorMax = Vector2.zero;
        grayRt.pivot = new Vector2(0, 0);
        grayRt.anchoredPosition = Vector2.zero;
        grayRt.sizeDelta = new Vector2(iconSize, iconSize);

        grayOverlay = grayObj.AddComponent<Image>();
        grayOverlay.sprite = graySprite != null ? graySprite : CreateCircleSprite((int)iconSize, true);
        grayOverlay.color = grayTint;
        grayOverlay.type = Image.Type.Filled;
        grayOverlay.fillMethod = Image.FillMethod.Radial360;
        grayOverlay.fillOrigin = (int)Image.Origin360.Top;
        grayOverlay.fillClockwise = false;
        grayOverlay.fillAmount = 0f;

        // Badge с количеством готовых зарядов
        GameObject badgeObj = new GameObject("Badge");
        badgeObj.transform.SetParent(container.transform, false);
        RectTransform bRt = badgeObj.AddComponent<RectTransform>();
        bRt.anchorMin = Vector2.zero;
        bRt.anchorMax = Vector2.zero;
        bRt.pivot = new Vector2(1, 0); // правый нижний угол текста
        bRt.anchoredPosition = new Vector2(iconSize + 4f, 8f);
        bRt.sizeDelta = new Vector2(60, 50);

        badgeText = badgeObj.AddComponent<TextMeshProUGUI>();
        badgeText.fontSize = 36;
        badgeText.color = Color.white;
        badgeText.alignment = TextAlignmentOptions.Center;
        badgeText.raycastTarget = false;
        badgeText.fontStyle = FontStyles.Bold;
        badgeText.outlineColor = Color.black;
        badgeText.outlineWidth = 0.3f;
        badgeText.text = "0";
    }

    private Sprite CreateCircleSprite(int size, bool solid)
    {
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        Color clear = new Color(0, 0, 0, 0);
        Color white = Color.white;
        float r = size * 0.5f;
        Vector2 center = new Vector2(r, r);

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float d = Vector2.Distance(new Vector2(x, y), center);
                bool inside = solid ? (d <= r) : (d <= r && d >= r * 0.65f);
                tex.SetPixel(x, y, inside ? white : clear);
            }
        }
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect);
    }
}
