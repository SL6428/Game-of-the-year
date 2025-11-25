using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    public GameObject MainMenuPanel;
    public GameObject SettingsPanel;
    public GameObject CreditsPanel;

    public void StartNewGame()
    {
        PlayerPrefs.DeleteAll(); // Очистка сохранений
        SceneManager.LoadScene("Sinematic"); // Замените на имя вашей игровой сцены
    }

    public void OpenSettings()
    {
        // Пока просто заглушка
        Debug.Log("Открыть настройки");
        MainMenuPanel.SetActive(false);
        SettingsPanel.SetActive(true);
    }

    public CreditsScroller СreditsScroller;
    public void ShowCredits()
    {
        Debug.Log("Показать титры");
        MainMenuPanel.SetActive(false);
        CreditsPanel.SetActive(true);

        // Запускаем прокрутку титров
        if (creditsScroller != null)
            creditsScroller.StartScrolling();
    }

    public void ReturnToMainMenu()
    {
        // Останавливаем прокрутку при возврате
        if (creditsScroller != null)
            creditsScroller.StopScrolling();

        SettingsPanel.SetActive(false);
        CreditsPanel.SetActive(false);
        MainMenuPanel.SetActive(true);
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false; // Для редактора
#else
        Application.Quit();
#endif
    }
}