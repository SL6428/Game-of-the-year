using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Триггерная зона для выхода в главное меню.
/// Когда игрок входит - показывается окно, по нажатию кнопки загружается меню.
/// </summary>
public class LevelExitTrigger : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("Имя сцены главного меню (точно как в Build Settings)")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";
    
    [Tooltip("Кнопка для перехода в меню")]
    [SerializeField] private KeyCode exitKey = KeyCode.Escape;

    [Header("UI")]
    [Tooltip("Твоё окно выхода (панель на весь экран)")]
    [SerializeField] private GameObject exitWindowPanel;

    private bool isPlayerInside = false;
    private bool isWindowOpen = false;

    void Start()
    {
        // Скрываем окно по умолчанию
        if (exitWindowPanel != null)
        {
            exitWindowPanel.SetActive(false);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        // Проверяем что это игрок
        if (other.CompareTag("Player"))
        {
            isPlayerInside = true;
            ShowExitWindow();
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInside = false;
            HideExitWindow();
        }
    }

    void Update()
    {
        // Если окно открыто и игрок внутри - проверяем нажатие кнопки
        if (isWindowOpen && isPlayerInside && Input.GetKeyDown(exitKey))
        {
            LoadMainMenu();
        }
    }

    void ShowExitWindow()
    {
        if (exitWindowPanel != null)
        {
            exitWindowPanel.SetActive(true);
            isWindowOpen = true;
            
            // Замедляем время для эффекта паузы
            Time.timeScale = 0.01f;
        }
    }

    void HideExitWindow()
    {
        if (exitWindowPanel != null)
        {
            exitWindowPanel.SetActive(false);
            isWindowOpen = false;
            
            // Возвращаем нормальную скорость
            Time.timeScale = 1f;
        }
    }

    void LoadMainMenu()
    {
        Debug.Log($"[LevelExitTrigger] Загружаю сцену: {mainMenuSceneName}");
        
        // Возвращаем нормальную скорость времени
        Time.timeScale = 1f;
        
        // Загружаем сцену меню
        SceneManager.LoadScene(mainMenuSceneName);
    }

    // Отрисовка триггера в редакторе
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        
        // Рисуем границы коллайдера
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            Gizmos.matrix = transform.localToWorldMatrix;
            
            if (col is BoxCollider box)
            {
                Gizmos.DrawWireCube(box.center, box.size);
            }
            else if (col is SphereCollider sphere)
            {
                Gizmos.DrawWireSphere(sphere.center, sphere.radius);
            }
        }
    }
}
