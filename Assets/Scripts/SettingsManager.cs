using UnityEngine;
using UnityEngine.SceneManagement;
using System;

public enum ColorblindMode
{
    None = 0,
    Protanopia = 1,
    Deuteranopia = 2,
    Tritanopia = 3
}

public enum FPSDisplayMode
{
    Off = 0,
    TopLeft = 1,
    TopRight = 2,
    BottomLeft = 3,
    BottomRight = 4
}

public class SettingsManager : MonoBehaviour
{
    public static SettingsManager Instance { get; private set; }

    public event Action OnSettingsApplied;

    public const float MIN_SENSITIVITY = 0.1f;
    public const float MAX_SENSITIVITY = 3f;
    public const float DEFAULT_SENSITIVITY = 1f;

    public const float MIN_BRIGHTNESS = -1f;
    public const float MAX_BRIGHTNESS = 1f;
    public const float DEFAULT_BRIGHTNESS = 0f;

    public const float MIN_GAMMA = -1f;
    public const float MAX_GAMMA = 1f;
    public const float DEFAULT_GAMMA = 0f;

    [Header("Defaults")]
    [SerializeField] private float defaultSensitivity = 1f;
    [SerializeField] private FullScreenMode defaultFullScreenMode = FullScreenMode.FullScreenWindow;

    private float cameraSensitivity;
    private Resolution[] availableResolutions;
    private int currentResolutionIndex;
    private FullScreenMode currentFullScreenMode;

    private int vSyncCount;
    private float brightness;
    private float gamma;
    private ColorblindMode colorblindMode;
    private FPSDisplayMode fpsDisplayMode;

    public float CameraSensitivity => cameraSensitivity;
    public Resolution[] AvailableResolutions => availableResolutions;
    public int CurrentResolutionIndex => currentResolutionIndex;
    public FullScreenMode CurrentFullScreenMode => currentFullScreenMode;
    public int VSyncCount => vSyncCount;
    public float Brightness => brightness;
    public float Gamma => gamma;
    public ColorblindMode ColorblindModeValue => colorblindMode;
    public FPSDisplayMode FPSDisplayModeValue => fpsDisplayMode;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        InitializeResolutions();
        LoadSettings();
    }

    void Start()
    {
        EnsureRuntimeComponents();

        ApplyScreenSettings();
        ApplyCameraSensitivity();
        ApplyDisplayEffects();
        ApplyFPSDisplay();
    }

    private void EnsureRuntimeComponents()
    {
        if (FindFirstObjectByType<DisplayEffectController>() == null)
        {
            GameObject ctrlObj = new GameObject("DisplayEffectController");
            ctrlObj.transform.SetParent(transform);
            ctrlObj.AddComponent<DisplayEffectController>();
        }

        if (FindFirstObjectByType<FPSDisplay>() == null)
        {
            GameObject fpsObj = new GameObject("FPSDisplay");
            fpsObj.transform.SetParent(transform);
            fpsObj.AddComponent<FPSDisplay>();
        }
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ApplyCameraSensitivity();
        ApplyDisplayEffects();
        ApplyFPSDisplay();
    }

    private void InitializeResolutions()
    {
        Resolution[] all = Screen.resolutions;
        System.Collections.Generic.List<Resolution> unique =
            new System.Collections.Generic.List<Resolution>();

        for (int i = 0; i < all.Length; i++)
        {
            bool exists = false;
            for (int j = 0; j < unique.Count; j++)
            {
                if (unique[j].width == all[i].width && unique[j].height == all[i].height)
                {
                    if ((int)all[i].refreshRateRatio.value > (int)unique[j].refreshRateRatio.value)
                        unique[j] = all[i];
                    exists = true;
                    break;
                }
            }
            if (!exists)
                unique.Add(all[i]);
        }

        availableResolutions = unique.ToArray();

        currentResolutionIndex = 0;
        for (int i = 0; i < availableResolutions.Length; i++)
        {
            if (availableResolutions[i].width == Screen.width &&
                availableResolutions[i].height == Screen.height)
            {
                currentResolutionIndex = i;
                break;
            }
        }
    }

    public void SetCameraSensitivity(float value)
    {
        cameraSensitivity = Mathf.Clamp(value, MIN_SENSITIVITY, MAX_SENSITIVITY);
    }

    public void SetResolution(int index)
    {
        if (index >= 0 && index < availableResolutions.Length)
            currentResolutionIndex = index;
    }

    public void SetFullScreenMode(FullScreenMode mode)
    {
        currentFullScreenMode = mode;
    }

    public void SetVSync(int value)
    {
        vSyncCount = Mathf.Clamp(value, 0, 2);
        QualitySettings.vSyncCount = vSyncCount;
    }

    public void SetBrightness(float value)
    {
        brightness = Mathf.Clamp(value, MIN_BRIGHTNESS, MAX_BRIGHTNESS);
    }

    public void SetGamma(float value)
    {
        gamma = Mathf.Clamp(value, MIN_GAMMA, MAX_GAMMA);
    }

    public void SetColorblindMode(ColorblindMode mode)
    {
        colorblindMode = mode;
    }

    public void SetFPSDisplayMode(FPSDisplayMode mode)
    {
        fpsDisplayMode = mode;
    }

    public void ApplyScreenSettings()
    {
        if (availableResolutions == null || currentResolutionIndex < 0 ||
            currentResolutionIndex >= availableResolutions.Length)
            return;

        Resolution res = availableResolutions[currentResolutionIndex];
        Screen.SetResolution(res.width, res.height, currentFullScreenMode,
            res.refreshRateRatio);
    }

    public void ApplyCameraSensitivity()
    {
        CameraController[] controllers =
            FindObjectsByType<CameraController>(FindObjectsSortMode.None);
        foreach (var c in controllers)
            c.sensitivityMultiplier = cameraSensitivity;

        CinemachineSensitivity[] appliers =
            FindObjectsByType<CinemachineSensitivity>(FindObjectsSortMode.None);
        foreach (var a in appliers)
            a.ApplySensitivity(cameraSensitivity);

        // Fallback: если на CinemachineCamera не повешен CinemachineSensitivity
        // (могло быть забыто в Editor), пробуем найти Input Axis Controller напрямую
        if (appliers == null || appliers.Length == 0)
        {
            var inputControllers = FindObjectsByType<Unity.Cinemachine.CinemachineInputAxisController>(
                FindObjectsSortMode.None);
            foreach (var ic in inputControllers)
            {
                // Добавляем CinemachineSensitivity автоматически и применяем
                var sens = ic.GetComponent<CinemachineSensitivity>();
                if (sens == null) sens = ic.gameObject.AddComponent<CinemachineSensitivity>();
                sens.ApplySensitivity(cameraSensitivity);
            }
        }
    }

    public void ApplyDisplayEffects()
    {
        DisplayEffectController[] controllers =
            FindObjectsByType<DisplayEffectController>(FindObjectsSortMode.None);
        foreach (var c in controllers)
            c.ApplyFromSettings();
    }

    public void ApplyFPSDisplay()
    {
        FPSDisplay[] displays =
            FindObjectsByType<FPSDisplay>(FindObjectsSortMode.None);
        foreach (var d in displays)
            d.ApplyFromSettings();
    }

    public void ApplyAllSettings()
    {
        ApplyScreenSettings();
        ApplyCameraSensitivity();
        ApplyDisplayEffects();
        ApplyFPSDisplay();
        SaveSettings();
        OnSettingsApplied?.Invoke();
    }

    public void SaveSettings()
    {
        PlayerPrefs.SetFloat("Settings_Sensitivity", cameraSensitivity);
        PlayerPrefs.SetInt("Settings_VSync", vSyncCount);
        PlayerPrefs.SetFloat("Settings_Brightness", brightness);
        PlayerPrefs.SetFloat("Settings_Gamma", gamma);
        PlayerPrefs.SetInt("Settings_ColorblindMode", (int)colorblindMode);
        PlayerPrefs.SetInt("Settings_FPSDisplayMode", (int)fpsDisplayMode);

        if (availableResolutions != null && currentResolutionIndex >= 0 &&
            currentResolutionIndex < availableResolutions.Length)
        {
            Resolution res = availableResolutions[currentResolutionIndex];
            PlayerPrefs.SetInt("Settings_ResWidth", res.width);
            PlayerPrefs.SetInt("Settings_ResHeight", res.height);
        }

        PlayerPrefs.SetInt("Settings_FullScreenMode", (int)currentFullScreenMode);
        PlayerPrefs.Save();
    }

    public void LoadSettings()
    {
        cameraSensitivity = PlayerPrefs.GetFloat("Settings_Sensitivity", defaultSensitivity);
        currentFullScreenMode = (FullScreenMode)PlayerPrefs.GetInt(
            "Settings_FullScreenMode", (int)defaultFullScreenMode);

        vSyncCount = PlayerPrefs.GetInt("Settings_VSync", 0);
        brightness = PlayerPrefs.GetFloat("Settings_Brightness", DEFAULT_BRIGHTNESS);
        gamma = PlayerPrefs.GetFloat("Settings_Gamma", DEFAULT_GAMMA);
        colorblindMode = (ColorblindMode)PlayerPrefs.GetInt("Settings_ColorblindMode", 0);
        fpsDisplayMode = (FPSDisplayMode)PlayerPrefs.GetInt("Settings_FPSDisplayMode", 0);

        QualitySettings.vSyncCount = vSyncCount;

        int savedWidth = PlayerPrefs.GetInt("Settings_ResWidth", -1);
        int savedHeight = PlayerPrefs.GetInt("Settings_ResHeight", -1);

        if (savedWidth > 0 && savedHeight > 0 && availableResolutions != null)
        {
            for (int i = 0; i < availableResolutions.Length; i++)
            {
                if (availableResolutions[i].width == savedWidth &&
                    availableResolutions[i].height == savedHeight)
                {
                    currentResolutionIndex = i;
                    break;
                }
            }
        }
    }

    public static int GetFullScreenModeIndex(FullScreenMode mode)
    {
        return mode switch
        {
            FullScreenMode.ExclusiveFullScreen => 0,
            FullScreenMode.FullScreenWindow => 1,
            FullScreenMode.Windowed => 2,
            _ => 1
        };
    }

    public static FullScreenMode GetFullScreenModeFromIndex(int index)
    {
        return index switch
        {
            0 => FullScreenMode.ExclusiveFullScreen,
            1 => FullScreenMode.FullScreenWindow,
            2 => FullScreenMode.Windowed,
            _ => FullScreenMode.FullScreenWindow
        };
    }
}
