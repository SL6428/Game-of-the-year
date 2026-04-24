using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FPSDisplay : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI fpsLabel;

    private float deltaTime;
    private float updateInterval = 0.5f;
    private float timeSinceUpdate;
    private int frameCount;
    private float fps;

    void Start()
    {
        CreateFPSLabel();
        ApplyFromSettings();
    }

    private void CreateFPSLabel()
    {
        if (fpsLabel != null) return;

        Canvas targetCanvas = FindFirstObjectByType<Canvas>();

        if (targetCanvas == null || targetCanvas.renderMode != RenderMode.ScreenSpaceOverlay)
        {
            GameObject canvasObj = new GameObject("FPS_Canvas");
            canvasObj.transform.SetParent(transform);
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 10000;
            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            targetCanvas = canvas;
        }

        GameObject labelObj = new GameObject("FPS_Label");
        labelObj.transform.SetParent(targetCanvas.transform, false);

        RectTransform rect = labelObj.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(200, 40);
        rect.pivot = new Vector2(0, 1);

        fpsLabel = labelObj.AddComponent<TextMeshProUGUI>();
        fpsLabel.fontSize = 24;
        fpsLabel.color = new Color(0.3f, 1f, 0.3f, 0.9f);
        fpsLabel.alignment = TextAlignmentOptions.Left;
        fpsLabel.raycastTarget = false;
        fpsLabel.text = "";
        fpsLabel.fontStyle = FontStyles.Bold;
    }

    void Update()
    {
        if (fpsLabel == null || !fpsLabel.gameObject.activeSelf) return;

        deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
        frameCount++;
        timeSinceUpdate += Time.unscaledDeltaTime;

        if (timeSinceUpdate >= updateInterval)
        {
            fps = frameCount / timeSinceUpdate;
            float ms = deltaTime * 1000f;

            if (fps >= 55)
                fpsLabel.color = new Color(0.3f, 1f, 0.3f, 0.9f);
            else if (fps >= 30)
                fpsLabel.color = new Color(1f, 0.9f, 0.2f, 0.9f);
            else
                fpsLabel.color = new Color(1f, 0.3f, 0.3f, 0.9f);

            fpsLabel.text = $"FPS: {Mathf.RoundToInt(fps)} ({ms:F1}ms)";
            frameCount = 0;
            timeSinceUpdate = 0f;
        }
    }

    public void ApplyFromSettings()
    {
        if (fpsLabel == null) CreateFPSLabel();
        if (SettingsManager.Instance == null) return;

        FPSDisplayMode mode = SettingsManager.Instance.FPSDisplayModeValue;

        if (mode == FPSDisplayMode.Off)
        {
            fpsLabel.gameObject.SetActive(false);
            return;
        }

        fpsLabel.gameObject.SetActive(true);

        RectTransform rect = fpsLabel.GetComponent<RectTransform>();

        switch (mode)
        {
            case FPSDisplayMode.TopLeft:
                rect.anchorMin = new Vector2(0, 1);
                rect.anchorMax = new Vector2(0, 1);
                rect.pivot = new Vector2(0, 1);
                rect.anchoredPosition = new Vector2(10, -10);
                fpsLabel.alignment = TextAlignmentOptions.Left;
                break;

            case FPSDisplayMode.TopRight:
                rect.anchorMin = new Vector2(1, 1);
                rect.anchorMax = new Vector2(1, 1);
                rect.pivot = new Vector2(1, 1);
                rect.anchoredPosition = new Vector2(-10, -10);
                fpsLabel.alignment = TextAlignmentOptions.Right;
                break;

            case FPSDisplayMode.BottomLeft:
                rect.anchorMin = new Vector2(0, 0);
                rect.anchorMax = new Vector2(0, 0);
                rect.pivot = new Vector2(0, 0);
                rect.anchoredPosition = new Vector2(10, 10);
                fpsLabel.alignment = TextAlignmentOptions.Left;
                break;

            case FPSDisplayMode.BottomRight:
                rect.anchorMin = new Vector2(1, 0);
                rect.anchorMax = new Vector2(1, 0);
                rect.pivot = new Vector2(1, 0);
                rect.anchoredPosition = new Vector2(-10, 10);
                fpsLabel.alignment = TextAlignmentOptions.Right;
                break;
        }
    }
}
