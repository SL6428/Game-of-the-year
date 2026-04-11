using UnityEngine;

/// <summary>
/// Компонент для игрока - позволяет взаимодействовать с объектами.
/// Повесить на объект игрока.
/// </summary>
public class PlayerInteraction : MonoBehaviour
{
    void Start()
    {
        // Проверяем что у игрока правильный тег
        if (gameObject.CompareTag("Player"))
        {
            Debug.Log("[PlayerInteraction] Игрок настроен правильно!");
        }
        else
        {
            Debug.LogWarning("[PlayerInteraction] У игрока не установлен тег 'Player'!");
        }
    }
}
