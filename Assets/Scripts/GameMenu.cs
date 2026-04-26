using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

public class GameMenu : MonoBehaviour
{
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
    private string mainMenuSceneName = "Sinematic";
    private SettingsUI settingsUI;

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
        if (LevelUpMenu.IsOpen || LevelUpMenu.JustClosed) return;

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ToggleMenu();
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
        Time.timeScale = 0f;
    }

    public void CloseAllMenus()
    {
        GameMenuPanel.SetActive(false);
        SystemSubMenu.SetActive(false);
        QuitConfirmationPanel.SetActive(false);
        if (SettingsPanel != null)
            SettingsPanel.SetActive(false);
        isMenuOpen = false;

        if (SceneManager.GetActiveScene().name != "Sinematic")
        {
            Time.timeScale = 1f;
        }
    }

    public void OnResumeClicked()
    {
        CloseAllMenus();

        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetPause(false);
        }
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
        Time.timeScale = 1f;
        PlayerPrefs.SetFloat("LastSessionTime", Time.time - gameStartTime);
        StartCoroutine(LoadMainMenuWithDelay());
        SceneManager.LoadScene("Sinematic");
    }

    IEnumerator LoadMainMenuWithDelay()
    {
        yield return new WaitForSecondsRealtime(0.1f);

        try
        {
            SceneManager.LoadScene("Sinematic");
        }
        catch (System.Exception e)
        {
            Debug.LogError("Ошибка загрузки главной сцены: " + e.Message);
            SceneManager.LoadScene(1);
        }
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