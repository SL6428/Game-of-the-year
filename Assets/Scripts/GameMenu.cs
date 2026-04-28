using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

public class GameMenu : MonoBehaviour
{
    public static GameMenu Instance { get; private set; }

    [Header("Панели меню")]
    public GameObject GameMenuPanel;
    public GameObject SystemSubMenu;
    public GameObject QuitConfirmationPanel;
    public GameObject SettingsPanel;

    [Header("Текст таймера")]
    public TextMeshProUGUI GameTimeText;

    [Header("Кнопки")]
    public Button resumeButton;

    private bool isMenuOpen = false;
    private float gameStartTime;
    private bool isGameStarted = false;
    private Coroutine timeCoroutine;
    private string mainMenuSceneName = "MainMenu";
    private SettingsUI settingsUI;
    private int pauseDepth = 0;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        CloseAllMenus();

        if (resumeButton != null)
        {
            resumeButton.onClick.AddListener(OnResumeClicked);
        }
        else
        {
            Debug.LogWarning("Resume button not assigned in inspector!");
        }

        if (SettingsPanel != null)
        {
            settingsUI = SettingsPanel.GetComponent<SettingsUI>();
            if (settingsUI != null)
                settingsUI.OnBackPressed += OnSettingsBack;
        }

        gameStartTime = Time.time;
        isGameStarted = true;

        if (timeCoroutine != null) StopCoroutine(timeCoroutine);
        timeCoroutine = StartCoroutine(UpdateGameTime());
    }

    void OnDestroy()
    {
        if (settingsUI != null)
            settingsUI.OnBackPressed -= OnSettingsBack;
    }

    bool IsInMainMenu()
    {
        return SceneManager.GetActiveScene().name == mainMenuSceneName;
    }

    void Update()
    {
        if (SceneManager.GetActiveScene().name == "MainMenu") return;

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (LevelUpMenu.IsOpen)
            {
                LevelUpMenu.Instance?.Hide();
            }
            else
            {
                ToggleMenu();
            }
        }
    }

    void ToggleMenu()
    {
        if (!isMenuOpen)
        {
            OpenMainMenu();
        }
        else
        {
            CloseAllMenus();
        }
    }

    IEnumerator UpdateGameTime()
    {
        while (isGameStarted)
        {
            if (GameTimeText != null)
            {
                float currentTime = Time.time - gameStartTime;
                string timeString = FormatTime(currentTime);
                GameTimeText.text = "Время игры: " + timeString;
            }
            yield return new WaitForSeconds(1f);
        }
    }

    string FormatTime(float timeInSeconds)
    {
        int hours = (int)(timeInSeconds / 3600);
        int minutes = (int)((timeInSeconds % 3600) / 60);
        int seconds = (int)(timeInSeconds % 60);
        return string.Format("{0:00}:{1:00}:{2:00}", hours, minutes, seconds);
    }

    public void OpenMainMenu()
    {
        if (SceneManager.GetActiveScene().name == "Sinematic")
        {
            Debug.LogWarning("Нельзя открыть меню в главной сцене!");
            return;
        }

        GameMenuPanel.SetActive(true);
        SystemSubMenu.SetActive(false);
        QuitConfirmationPanel.SetActive(false);
        if (SettingsPanel != null)
            SettingsPanel.SetActive(false);
        isMenuOpen = true;

        PushMenuPause();
    }

    public void CloseAllMenus()
    {
        GameMenuPanel.SetActive(false);
        SystemSubMenu.SetActive(false);
        QuitConfirmationPanel.SetActive(false);
        if (SettingsPanel != null)
            SettingsPanel.SetActive(false);
        isMenuOpen = false;

        PopMenuPause();
    }

    public void PushMenuPause()
    {
        if (pauseDepth == 0)
        {
            Time.timeScale = 0f;
            var pc = FindFirstObjectByType<PlayerController>();
            if (pc != null) pc.enabled = false;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        pauseDepth++;
    }

    public void PopMenuPause()
    {
        pauseDepth = Mathf.Max(0, pauseDepth - 1);
        if (pauseDepth == 0)
        {
            Time.timeScale = 1f;
            var pc = FindFirstObjectByType<PlayerController>();
            if (pc != null) pc.enabled = true;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    public void OnResumeClicked()
    {
        CloseAllMenus();
    }

    public void OnInventoryClicked()
    {
        Debug.Log("Инвентарь заглушка");
    }

    public void OnEquipmentClicked()
    {
        Debug.Log("Экипировка заглушка");
    }

    public void OnSkillsClicked()
    {
        Debug.Log("Навыки заглушка");
    }

    public void OnSystemClicked()
    {
        SystemSubMenu.SetActive(true);
    }

    public void OnSystemSettingsClicked()
    {
        if (SettingsPanel != null)
        {
            SystemSubMenu.SetActive(false);
            SettingsPanel.SetActive(true);
        }
    }

    private void OnSettingsBack()
    {
        if (SettingsPanel != null)
            SettingsPanel.SetActive(false);
        SystemSubMenu.SetActive(true);
    }

    public void OnSystemBackClicked()
    {
        SystemSubMenu.SetActive(false);
    }

    public void OnSystemQuitClicked()
    {
        QuitConfirmationPanel.SetActive(true);
    }

    public void OnQuitToMenuConfirmed()
    {
        pauseDepth = 0;
        Time.timeScale = 1f;
        PlayerPrefs.SetFloat("LastSessionTime", Time.time - gameStartTime);
        SceneManager.LoadScene("MainMenu");
    }

    public void OnQuitToDesktopConfirmed()
    {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public void OnQuitCancelClicked()
    {
        QuitConfirmationPanel.SetActive(false);
    }
}