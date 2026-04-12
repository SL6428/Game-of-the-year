using UnityEngine;

/// <summary>
/// Тестовый скрипт для диагностики двери.
/// Добавьте его на дверь ВМЕСТЕ с Door.cs
/// Удалите после диагностики!
/// </summary>
public class DoorDebugHelper : MonoBehaviour
{
    void Start()
    {
        Debug.Log("=== DoorDebugHelper ===");
        Debug.Log($"[Debug] Дверь: {gameObject.name}");
        Debug.Log($"[Debug] Позиция: {transform.position}");
        Debug.Log($"[Debug] Поворот: {transform.eulerAngles}");
        
        // Проверяем Door компонент
        Door door = GetComponent<Door>();
        if (door != null)
        {
            Debug.Log($"[Debug] Door компонент: ✅ найден");
        }
        else
        {
            Debug.LogError($"[Debug] Door компонент: ❌ НЕ найден!");
        }
        
        // Проверяем GameManager
        if (GameManager.Instance != null)
        {
            Debug.Log($"[Debug] GameManager: ✅ найден");
            Debug.Log($"[Debug] GameManager.Player: {GameManager.Instance.Player}");
            Debug.Log($"[Debug] Time.timeScale: {Time.timeScale}");
            Debug.Log($"[Debug] GameManager.IsGamePaused: {GameManager.Instance.IsGamePaused()}");
        }
        else
        {
            Debug.LogWarning($"[Debug] GameManager: ❌ НЕ найден!");
        }
        
        // Ищем игрока
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            Debug.Log($"[Debug] Игрок по тегу 'Player': ✅ найден ({player.name})");
        }
        else
        {
            Debug.LogError($"[Debug] Игрок по тегу 'Player': ❌ НЕ найден!");
        }
        
        // Проверяем SimpleInventory
        if (SimpleInventory.Instance != null)
        {
            Debug.Log($"[Debug] SimpleInventory: ✅ найден");
        }
        else
        {
            Debug.Log($"[Debug] SimpleInventory: ещё не создан (будет создан автоматически)");
        }
        
        Debug.Log("=== Конец диагностики ===");
    }
    
    void Update()
    {
        // Показываем расстояние до игрока каждый кадр
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            float distance = Vector3.Distance(transform.position, player.transform.position);
            Debug.Log($"[Debug] Расстояние до игрока: {distance:F2}");
        }
    }
}
