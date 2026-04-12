using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Player")]
    [Tooltip("Ссылка на игрока (перетащи из сцены)")]
    [SerializeField] private Transform player;

    [Header("UI References")]
    public GameObject pauseMenu;

    private bool isPaused = false;

    // Публичное свойство для доступа к игроку
    public Transform Player => player;

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
        // Автоматический поиск игрока если не назначен
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
                Debug.Log("[GameManager] Найден игрок по тегу 'Player'");
            }
            else
            {
                Debug.LogWarning("[GameManager] Не найден игрок! Назначьте его в GameManager или добавьте тег 'Player'");
            }
        }

        // Сбрасываем паузу - игра запущена
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

        // Время и физика
        Time.timeScale = paused ? 0f : 1f;

        // Управление курсором
        Cursor.lockState = paused ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = paused;

        // Показываем/скрываем меню паузы
        if (pauseMenu != null)
            pauseMenu.SetActive(paused);

        Debug.Log(paused ? "Game Paused" : "Game Resumed");
    }

    public bool IsGamePaused()
    {
        return isPaused;
    }
}
