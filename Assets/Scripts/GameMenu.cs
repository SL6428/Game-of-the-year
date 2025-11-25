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

    void Start()
    {
        // Скрываем все меню при старте
        CloseAllMenus();

        // Запоминаем время начала игры
        gameStartTime = Time.time;
        isGameStarted = true;

        // Запускаем обновление времени
        StartCoroutine(UpdateGameTime());
    }

    void Update()
    {
        // Открываем/закрываем меню по ESC
        if (Input.GetKeyDown(KeyCode.Escape))
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
    }

    IEnumerator UpdateGameTime()
    {
        while (isGameStarted)
        {
            if (GameTimeText != null && GameTimeText.gameObject.activeInHierarchy)
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

    // ОСНОВНЫЕ МЕТОДЫ МЕНЮ
    public void OpenMainMenu()
    {
        GameMenuPanel.SetActive(true);
        SystemSubMenu.SetActive(false);
        QuitConfirmationPanel.SetActive(false);
        isMenuOpen = true;
        Time.timeScale = 0f; // Пауза игры
    }

    public void CloseAllMenus()
    {
        GameMenuPanel.SetActive(false);
        SystemSubMenu.SetActive(false);
        QuitConfirmationPanel.SetActive(false);
        isMenuOpen = false;
        Time.timeScale = 1f; // Возобновление игры
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
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu"); // Ваша сцена главного меню
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
