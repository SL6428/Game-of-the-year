using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections.Generic;

public class SettingsUI : MonoBehaviour
{
    [Header("Resolution")]
    [SerializeField] private TMP_Dropdown resolutionDropdown;

    [Header("Display Mode")]
    [SerializeField] private TMP_Dropdown fullscreenDropdown;

    [Header("Camera Sensitivity")]
    [SerializeField] private Slider sensitivitySlider;
    [SerializeField] private TextMeshProUGUI sensitivityValueText;

    [Header("V-Sync")]
    [SerializeField] private TMP_Dropdown vsyncDropdown;

    [Header("Brightness")]
    [SerializeField] private Slider brightnessSlider;
    [SerializeField] private TextMeshProUGUI brightnessValueText;

    [Header("Gamma")]
    [SerializeField] private Slider gammaSlider;
    [SerializeField] private TextMeshProUGUI gammaValueText;

    [Header("Colorblind Mode")]
    [SerializeField] private TMP_Dropdown colorblindDropdown;

    [Header("FPS Display")]
    [SerializeField] private TMP_Dropdown fpsDisplayDropdown;

    [Header("Buttons")]
    [SerializeField] private Button applyButton;
    [SerializeField] private Button backButton;

    public event Action OnBackPressed;

    private SettingsManager settings;

    private float savedSensitivity;
    private int savedResolutionIndex;
    private FullScreenMode savedFullScreenMode;
    private int savedVSync;
    private float savedBrightness;
    private float savedGamma;
    private ColorblindMode savedColorblindMode;
    private FPSDisplayMode savedFPSDisplayMode;

    void Awake()
    {
        settings = SettingsManager.Instance;
        if (settings == null)
        {
            GameObject go = new GameObject("SettingsManager");
            settings = go.AddComponent<SettingsManager>();
        }
    }

    void OnEnable()
    {
        PopulateResolutionDropdown();
        PopulateFullscreenDropdown();
        PopulateVSyncDropdown();
        PopulateColorblindDropdown();
        PopulateFPSDisplayDropdown();

        InitializeSensitivitySlider();
        InitializeBrightnessSlider();
        InitializeGammaSlider();

        SaveCurrentState();

        if (resolutionDropdown != null)
            resolutionDropdown.onValueChanged.AddListener(OnResolutionChanged);
        if (fullscreenDropdown != null)
            fullscreenDropdown.onValueChanged.AddListener(OnFullscreenChanged);
        if (sensitivitySlider != null)
            sensitivitySlider.onValueChanged.AddListener(OnSensitivityChanged);
        if (vsyncDropdown != null)
            vsyncDropdown.onValueChanged.AddListener(OnVSyncChanged);
        if (brightnessSlider != null)
            brightnessSlider.onValueChanged.AddListener(OnBrightnessChanged);
        if (gammaSlider != null)
            gammaSlider.onValueChanged.AddListener(OnGammaChanged);
        if (colorblindDropdown != null)
            colorblindDropdown.onValueChanged.AddListener(OnColorblindChanged);
        if (fpsDisplayDropdown != null)
            fpsDisplayDropdown.onValueChanged.AddListener(OnFPSDisplayChanged);
        if (applyButton != null)
            applyButton.onClick.AddListener(OnApplyClicked);
        if (backButton != null)
            backButton.onClick.AddListener(OnBackClicked);
    }

    void OnDisable()
    {
        if (resolutionDropdown != null)
            resolutionDropdown.onValueChanged.RemoveListener(OnResolutionChanged);
        if (fullscreenDropdown != null)
            fullscreenDropdown.onValueChanged.RemoveListener(OnFullscreenChanged);
        if (sensitivitySlider != null)
            sensitivitySlider.onValueChanged.RemoveListener(OnSensitivityChanged);
        if (vsyncDropdown != null)
            vsyncDropdown.onValueChanged.RemoveListener(OnVSyncChanged);
        if (brightnessSlider != null)
            brightnessSlider.onValueChanged.RemoveListener(OnBrightnessChanged);
        if (gammaSlider != null)
            gammaSlider.onValueChanged.RemoveListener(OnGammaChanged);
        if (colorblindDropdown != null)
            colorblindDropdown.onValueChanged.RemoveListener(OnColorblindChanged);
        if (fpsDisplayDropdown != null)
            fpsDisplayDropdown.onValueChanged.RemoveListener(OnFPSDisplayChanged);
        if (applyButton != null)
            applyButton.onClick.RemoveListener(OnApplyClicked);
        if (backButton != null)
            backButton.onClick.RemoveListener(OnBackClicked);
    }

    private void SaveCurrentState()
    {
        savedSensitivity = settings.CameraSensitivity;
        savedResolutionIndex = settings.CurrentResolutionIndex;
        savedFullScreenMode = settings.CurrentFullScreenMode;
        savedVSync = settings.VSyncCount;
        savedBrightness = settings.Brightness;
        savedGamma = settings.Gamma;
        savedColorblindMode = settings.ColorblindModeValue;
        savedFPSDisplayMode = settings.FPSDisplayModeValue;
    }

    private void PopulateResolutionDropdown()
    {
        if (resolutionDropdown == null || settings == null) return;

        resolutionDropdown.ClearOptions();
        List<string> options = new List<string>();
        Resolution[] resolutions = settings.AvailableResolutions;

        for (int i = 0; i < resolutions.Length; i++)
        {
            int hz = (int)resolutions[i].refreshRateRatio.value;
            options.Add($"{resolutions[i].width} x {resolutions[i].height} ({hz}Hz)");
        }

        resolutionDropdown.AddOptions(options);
        resolutionDropdown.value = settings.CurrentResolutionIndex;
        resolutionDropdown.RefreshShownValue();
    }

    private void PopulateFullscreenDropdown()
    {
        if (fullscreenDropdown == null) return;

        fullscreenDropdown.ClearOptions();
        fullscreenDropdown.AddOptions(new List<string>
        {
            "Полноэкранный",
            "Полноэкранный окно",
            "Оконный"
        });

        fullscreenDropdown.value = SettingsManager.GetFullScreenModeIndex(
            settings.CurrentFullScreenMode);
        fullscreenDropdown.RefreshShownValue();
    }

    private void PopulateVSyncDropdown()
    {
        if (vsyncDropdown == null) return;

        vsyncDropdown.ClearOptions();
        vsyncDropdown.AddOptions(new List<string>
        {
            "Выключен",
            "Каждый кадр",
            "Каждый 2-й кадр"
        });

        vsyncDropdown.value = settings.VSyncCount;
        vsyncDropdown.RefreshShownValue();
    }

    private void PopulateColorblindDropdown()
    {
        if (colorblindDropdown == null) return;

        colorblindDropdown.ClearOptions();
        colorblindDropdown.AddOptions(new List<string>
        {
            "Выключен",
            "Протанопия (красно-зелёная)",
            "Дейтеранопия (красно-зелёная)",
            "Тританопия (сине-жёлтая)"
        });

        colorblindDropdown.value = (int)settings.ColorblindModeValue;
        colorblindDropdown.RefreshShownValue();
    }

    private void PopulateFPSDisplayDropdown()
    {
        if (fpsDisplayDropdown == null) return;

        fpsDisplayDropdown.ClearOptions();
        fpsDisplayDropdown.AddOptions(new List<string>
        {
            "Выключен",
            "Верхний левый",
            "Верхний правый",
            "Нижний левый",
            "Нижний правый"
        });

        fpsDisplayDropdown.value = (int)settings.FPSDisplayModeValue;
        fpsDisplayDropdown.RefreshShownValue();
    }

    private void InitializeSensitivitySlider()
    {
        if (sensitivitySlider == null) return;

        sensitivitySlider.minValue = SettingsManager.MIN_SENSITIVITY;
        sensitivitySlider.maxValue = SettingsManager.MAX_SENSITIVITY;
        sensitivitySlider.value = settings.CameraSensitivity;
        UpdateSensitivityText(settings.CameraSensitivity);
    }

    private void InitializeBrightnessSlider()
    {
        if (brightnessSlider == null) return;

        brightnessSlider.minValue = SettingsManager.MIN_BRIGHTNESS;
        brightnessSlider.maxValue = SettingsManager.MAX_BRIGHTNESS;
        brightnessSlider.value = settings.Brightness;
        UpdateBrightnessText(settings.Brightness);
    }

    private void InitializeGammaSlider()
    {
        if (gammaSlider == null) return;

        gammaSlider.minValue = SettingsManager.MIN_GAMMA;
        gammaSlider.maxValue = SettingsManager.MAX_GAMMA;
        gammaSlider.value = settings.Gamma;
        UpdateGammaText(settings.Gamma);
    }

    private void OnSensitivityChanged(float value)
    {
        settings.SetCameraSensitivity(value);
        settings.ApplyCameraSensitivity();
        UpdateSensitivityText(value);
    }

    private void OnResolutionChanged(int index)
    {
        settings.SetResolution(index);
        settings.ApplyScreenSettings();
    }

    private void OnFullscreenChanged(int index)
    {
        settings.SetFullScreenMode(SettingsManager.GetFullScreenModeFromIndex(index));
        settings.ApplyScreenSettings();
    }

    private void OnVSyncChanged(int index)
    {
        settings.SetVSync(index);
    }

    private void OnBrightnessChanged(float value)
    {
        settings.SetBrightness(value);
        settings.ApplyDisplayEffects();
        UpdateBrightnessText(value);
    }

    private void OnGammaChanged(float value)
    {
        settings.SetGamma(value);
        settings.ApplyDisplayEffects();
        UpdateGammaText(value);
    }

    private void OnColorblindChanged(int index)
    {
        settings.SetColorblindMode((ColorblindMode)index);
        settings.ApplyDisplayEffects();
    }

    private void OnFPSDisplayChanged(int index)
    {
        settings.SetFPSDisplayMode((FPSDisplayMode)index);
        settings.ApplyFPSDisplay();
    }

    private void OnApplyClicked()
    {
        SaveCurrentState();
        settings.ApplyAllSettings();
    }

    private void OnBackClicked()
    {
        settings.SetCameraSensitivity(savedSensitivity);
        settings.SetResolution(savedResolutionIndex);
        settings.SetFullScreenMode(savedFullScreenMode);
        settings.SetVSync(savedVSync);
        settings.SetBrightness(savedBrightness);
        settings.SetGamma(savedGamma);
        settings.SetColorblindMode(savedColorblindMode);
        settings.SetFPSDisplayMode(savedFPSDisplayMode);

        settings.ApplyScreenSettings();
        settings.ApplyCameraSensitivity();
        settings.ApplyDisplayEffects();
        settings.ApplyFPSDisplay();
        OnBackPressed?.Invoke();
    }

    private void UpdateSensitivityText(float value)
    {
        if (sensitivityValueText != null)
            sensitivityValueText.text = value.ToString("0.00");
    }

    private void UpdateBrightnessText(float value)
    {
        if (brightnessValueText != null)
            brightnessValueText.text = value.ToString("0.00");
    }

    private void UpdateGammaText(float value)
    {
        if (gammaValueText != null)
            gammaValueText.text = value.ToString("0.00");
    }
}
