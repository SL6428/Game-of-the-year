using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("UI References")]
    public GameObject pauseMenu;

    private bool isPaused = false;

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
        }
    }

    void Start()
    {
        // Начальное состояние - игра активна
        SetPause(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }
    }

    public void TogglePause()
    {
        isPaused = !isPaused;
        SetPause(isPaused);
    }

    public void SetPause(bool paused)
    {
        isPaused = paused;

        // Пауза в физике и времени
        Time.timeScale = paused ? 0f : 1f;

        // Управление курсором
        Cursor.lockState = paused ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = paused;

        // Включаем/выключаем меню паузы
        if (pauseMenu != null)
            pauseMenu.SetActive(paused);

        // Можно добавить другие эффекты при паузе (звук и т.д.)
        Debug.Log(paused ? "Game Paused" : "Game Resumed");
    }

    public bool IsGamePaused()
    {
        return isPaused;
    }
}