using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

public class GameMenu : MonoBehaviour
{
    [Header("Основные панели")]
    public GameObject GameMenuPanel;
    public GameObject SystemSubMenu;
    public GameObject QuitConfirmationPanel;

    [Header("Время игры")]
    public TextMeshProUGUI GameTimeText;

    private bool isMenuOpen = false;
    private float gameStartTime;
    private bool isGameStarted = false;
    private Coroutine timeCoroutine;
    private string mainMenuSceneName = "Sinematic";

    void Start()
    {
        CloseAllMenus();

        // Сбрасываем время при старте
        gameStartTime = Time.time;
        isGameStarted = true;

        // Запускаем обновление времени сразу при старте сцены
        if (timeCoroutine != null) StopCoroutine(timeCoroutine);
        timeCoroutine = StartCoroutine(UpdateGameTime());

        Debug.Log("InGameMenuManager инициализирован. Счетчик времени запущен.");
    }

    bool IsInMainMenu()
    {
        return SceneManager.GetActiveScene().name == mainMenuSceneName;
    }

    void InitializeInGameMenu()
    {
        CloseAllMenus();
        gameStartTime = Time.time;
        isGameStarted = true;
        StartCoroutine(UpdateGameTime());
    }

    void Update()
    {
        // Обрабатываем ESC только в игровой сцене
        if (SceneManager.GetActiveScene().name == "MainMenu") return;

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
                // Обновляем время даже если текст не активен
            if (GameTimeText != null)
            {
                float currentTime = Time.time - gameStartTime;
                string timeString = FormatTime(currentTime);
                GameTimeText.text = "Время игры: " + timeString;
                Debug.Log("Время обновлено: " + timeString);
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
        // Проверяем, что мы не в главном меню
        if (SceneManager.GetActiveScene().name == "Sinematic")
        {
            Debug.LogWarning("Попытка открыть игровое меню в главном меню!");
            return;
        }

        GameMenuPanel.SetActive(true);
        SystemSubMenu.SetActive(false);
        QuitConfirmationPanel.SetActive(false);
        isMenuOpen = true;
        Time.timeScale = 0f;
        Debug.Log("Игровое меню открыто");
    }

    public void CloseAllMenus()
    {
        GameMenuPanel.SetActive(false);
        SystemSubMenu.SetActive(false);
        QuitConfirmationPanel.SetActive(false);
        isMenuOpen = false;

        // Восстанавливаем время только в игровой сцене
        if (SceneManager.GetActiveScene().name != "Sinematic")
        {
            Time.timeScale = 1f;
        }
        Debug.Log("Все меню закрыты");
    }

    // КНОПКИ ГЛАВНОГО МЕНЮ
    public void OnResumeClicked()
    {
        CloseAllMenus();
    }

    public void OnInventoryClicked()
    {
        // Здесь будет логика инвентаря
        Debug.Log("Открыт инвентарь");
    }

    public void OnEquipmentClicked()
    {
        // Здесь будет логика снаряжения
        Debug.Log("Открыто снаряжение");
    }

    public void OnSkillsClicked()
    {
        // Здесь будет логика умений
        Debug.Log("Открыты умения");
    }

    public void OnSystemClicked()
    {
        SystemSubMenu.SetActive(true);
    }

    // КНОПКИ ПОДМЕНЮ СИСТЕМЫ
    public void OnSystemSettingsClicked()
    {
        // Можно переиспользовать вашу панель настроек из главного меню
        Debug.Log("Открыты настройки");
    }

    public void OnSystemBackClicked()
    {
        SystemSubMenu.SetActive(false);
    }

    public void OnSystemQuitClicked()
    {
        QuitConfirmationPanel.SetActive(true);
    }

    // КНОПКИ ПОДТВЕРЖДЕНИЯ ВЫХОДА
    public void OnQuitToMenuConfirmed()
    {
        Debug.Log("Загрузка главного меню...");
        Time.timeScale = 1f; // Важно: сбрасываем timescale перед сменой сцены

        // Сохраняем время игры перед выходом (опционально)
        PlayerPrefs.SetFloat("LastSessionTime", Time.time - gameStartTime);
        StartCoroutine(LoadMainMenuWithDelay());
        SceneManager.LoadScene("Sinematic");
    }

    IEnumerator LoadMainMenuWithDelay()
    {
        // Короткая задержка для обработки всех событий
        yield return new WaitForSecondsRealtime(0.1f);

        try
        {
            SceneManager.LoadScene("Sinematic");
            Debug.Log("Сцена главного меню загружается...");
        }
        catch (System.Exception e)
        {
            Debug.LogError("Ошибка загрузки главного меню: " + e.Message);

            // Альтернативный способ - по индексу сцены
            // Убедитесь, что главное меню имеет индекс 0 в Build Settings
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
