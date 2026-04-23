using UnityEngine;

/// <summary>
/// Арена босса - ПРОСТАЯ ВЕРСИЯ.
/// Стены выключены по умолчанию.
/// Включаются когда игрок входит в триггер-зону.
/// Выключаются когда босс умирает.
/// </summary>
public class BossArena : MonoBehaviour
{
    [Header("Босс")]
    [Tooltip("Перетащи босса сюда")]
    [SerializeField] private GameObject boss;
    
    [Tooltip("Компонент Health у босса (найдёт сам если пусто)")]
    [SerializeField] private Health bossHealth;

    [Header("Отладка")]
    [Tooltip("Показывать отладку в консоли")]
    [SerializeField] private bool debugMode = true;

    // 4 стены
    [SerializeField] private GameObject wallNorth;
    [SerializeField] private GameObject wallSouth;
    [SerializeField] private GameObject wallEast;
    [SerializeField] private GameObject wallWest;

    private bool isArenaActive = false;
    private bool bossWasDefeated = false;

    void Start()
    {
        Log("=== BossArena Start ===");
        Log($"Позиция арены: {transform.position}");
        
        // Проверяем что стены выключены
        CheckWallsState();
        
        // Находим Health
        FindBossHealth();
        
        Log("=== BossArena готов ===");
    }

    void Update()
    {
        // Проверяем смерть босса
        if (isArenaActive && !bossWasDefeated)
        {
            if (IsBossDead())
            {
                bossWasDefeated = true;
                DeactivateArena();
                Log("🏆 Босс побеждён! Арена открыта.");
            }
        }
    }

    /// <summary>
    /// Проверяет состояние стен
    /// </summary>
    void CheckWallsState()
    {
        Log("Проверка стен:");
        
        GameObject[] allWalls = { wallNorth, wallSouth, wallEast, wallWest };
        
        for (int i = 0; i < allWalls.Length; i++)
        {
            if (allWalls[i] != null)
            {
                bool isActive = allWalls[i].activeSelf;
                Log($"  Стена {i}: {(isActive ? "АКТИВНА ❌" : "выключена ✅")}");
                
                if (isActive)
                {
                    Log($"  ⚠️ ВНИМАНИЕ: Стена {i} активна! Это проблема!");
                }
            }
            else
            {
                Log($"  Стена {i}: НЕ НАЗНАЧЕНА ⚠️");
            }
        }
    }

    /// <summary>
    /// Ищет Health у босса
    /// </summary>
    void FindBossHealth()
    {
        if (boss == null)
        {
            Log("⚠️ Босс не назначен! Найди босса и перетащи в поле Boss.");
            return;
        }
        
        if (bossHealth == null)
        {
            bossHealth = boss.GetComponent<Health>();
            
            if (bossHealth != null)
            {
                Log($"✅ Найден Health у босса '{boss.name}'");
            }
            else
            {
                Log($"❌ Не найден Health у босса '{boss.name}'!");
                Log($"   Добавь компонент Health к боссу.");
            }
        }
        else
        {
            Log($"✅ Health назначен вручную");
        }
    }

    /// <summary>
    /// Активирует арену - включает ВСЕ стены
    /// </summary>
    public void ActivateArena()
    {
        if (isArenaActive)
        {
            Log("Арена уже активна");
            return;
        }

        Log("🔒 ВКЛЮЧАЮ АРЕНУ...");
        
        GameObject[] allWalls = { wallNorth, wallSouth, wallEast, wallWest };
        int enabledCount = 0;
        
        foreach (var wall in allWalls)
        {
            if (wall != null)
            {
                wall.SetActive(true);
                enabledCount++;
                Log($"  ✅ Включена: {wall.name}");
            }
        }
        
        isArenaActive = true;
        Log($"🔒 Арена активирована! Включено стен: {enabledCount}");
    }

    /// <summary>
    /// Деактивирует арену - выключает ВСЕ стены
    /// </summary>
    public void DeactivateArena()
    {
        if (!isArenaActive)
        {
            Log("Арена уже неактивна");
            return;
        }

        Log("🔓 ВЫКЛЮЧАЮ АРЕНУ...");
        
        GameObject[] allWalls = { wallNorth, wallSouth, wallEast, wallWest };
        int disabledCount = 0;
        
        foreach (var wall in allWalls)
        {
            if (wall != null)
            {
                wall.SetActive(false);
                disabledCount++;
                Log($"  ✅ Выключена: {wall.name}");
            }
        }
        
        isArenaActive = false;
        Log($"🔓 Арена деактивирована! Выключено стен: {disabledCount}");
    }

    /// <summary>
    /// Триггер - игрок вошёл
    /// </summary>
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Log($" Игрок вошёл в триггер!");
            Log($"  Игрок: {other.name}");
            Log($"  Позиция: {other.transform.position}");
            ActivateArena();
        }
    }

    /// <summary>
    /// Триггер - игрок вышел (опционально)
    /// </summary>
    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Log($"🚶 Игрок вышел из триггера");
            // Можно добавить логику если нужно
        }
    }

    /// <summary>
    /// Проверяет мёртв ли босс
    /// </summary>
    bool IsBossDead()
    {
        if (boss == null)
        {
            Log("⚠️ Босс = null (уничтожен)");
            return true;
        }
        
        if (!boss.activeInHierarchy)
        {
            Log("⚠️ Босс неактивен");
            return true;
        }
        
        if (bossHealth != null)
        {
            bool isDead = bossHealth.IsDead;
            if (isDead)
            {
                Log("✅ Босс мёртв (Health.IsDead = true)");
            }
            return isDead;
        }
        
        return false;
    }

    /// <summary>
    /// Логирование
    /// </summary>
    void Log(string message)
    {
        if (debugMode)
        {
            Debug.Log($"[BossArena] {message}");
        }
    }

    /// <summary>
    /// Визуальная отладка в Scene View
    /// </summary>
    void OnDrawGizmos()
    {
        // Красная рамка = границы арены
        Gizmos.color = Color.red;
        
        // Рисуем примерные границы (если стены назначены)
        if (wallNorth != null && wallSouth != null && wallEast != null && wallWest != null)
        {
            // Север-юг
            Gizmos.DrawLine(wallNorth.transform.position, wallEast.transform.position);
            Gizmos.DrawLine(wallEast.transform.position, wallSouth.transform.position);
            Gizmos.DrawLine(wallSouth.transform.position, wallWest.transform.position);
            Gizmos.DrawLine(wallWest.transform.position, wallNorth.transform.position);
        }
    }
}
