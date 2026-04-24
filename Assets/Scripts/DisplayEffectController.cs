using UnityEngine;
using UnityEngine.UI;

public class DisplayEffectController : MonoBehaviour
{
    private Canvas overlayCanvas;
    private Image brightnessOverlay;
    private Image colorblindOverlay;

    private bool initialized;

    void Start()
    {
        Initialize();
        ApplyFromSettings();
    }

    private void Initialize()
    {
        if (initialized) return;

        overlayCanvas = CreateOverlayCanvas("DisplayEffectOverlay", 9999);

        brightnessOverlay = CreateOverlayImage(overlayCanvas.transform, "BrightnessOverlay");
        brightnessOverlay.color = new Color(0, 0, 0, 0);
        brightnessOverlay.raycastTarget = false;

        colorblindOverlay = CreateOverlayImage(overlayCanvas.transform, "ColorblindOverlay");
        colorblindOverlay.color = new Color(0, 0, 0, 0);
        colorblindOverlay.raycastTarget = false;

        initialized = true;
    }

    public void ApplyFromSettings()
    {
        if (!initialized) Initialize();
        if (SettingsManager.Instance == null) return;

        ApplyBrightness(SettingsManager.Instance.Brightness,
            SettingsManager.Instance.Gamma);
        ApplyColorblind(SettingsManager.Instance.ColorblindModeValue);
    }

    private void ApplyBrightness(float brightness, float gamma)
    {
        if (brightnessOverlay == null) return;

        float effectiveBrightness = brightness + gamma * 0.5f;
        effectiveBrightness = Mathf.Clamp(effectiveBrightness, -1f, 1f);

        if (effectiveBrightness <= 0f)
        {
            brightnessOverlay.color = new Color(0f, 0f, 0f, -effectiveBrightness * 0.85f);
        }
        else
        {
            float alpha = effectiveBrightness * 0.3f;
            brightnessOverlay.color = new Color(1f, 0.95f, 0.9f, alpha);
        }
    }

    private void ApplyColorblind(ColorblindMode mode)
    {
        if (colorblindOverlay == null) return;

        switch (mode)
        {
            case ColorblindMode.None:
                colorblindOverlay.color = new Color(0, 0, 0, 0);
                break;

            case ColorblindMode.Protanopia:
                colorblindOverlay.color = new Color(0.15f, 0.05f, 0.3f, 0.12f);
                break;

            case ColorblindMode.Deuteranopia:
                colorblindOverlay.color = new Color(0.1f, 0.05f, 0.35f, 0.12f);
                break;

            case ColorblindMode.Tritanopia:
                colorblindOverlay.color = new Color(0.3f, 0.15f, 0.05f, 0.12f);
                break;
        }
    }

    private Canvas CreateOverlayCanvas(string name, int sortOrder)
    {
        GameObject canvasObj = new GameObject(name);
        canvasObj.transform.SetParent(transform);

        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = sortOrder;
        canvas.additionalShaderChannels = AdditionalCanvasShaderChannels.None;

        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        canvasObj.AddComponent<GraphicRaycaster>();
        canvasObj.GetComponent<GraphicRaycaster>().enabled = false;

        return canvas;
    }

    private Image CreateOverlayImage(Transform parent, string name)
    {
        GameObject imageObj = new GameObject(name);
        imageObj.transform.SetParent(parent, false);

        RectTransform rect = imageObj.AddComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        Image image = imageObj.AddComponent<Image>();
        image.raycastTarget = false;

        return image;
    }
}
