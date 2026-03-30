using UnityEngine;

/// <summary>
/// Тестовый скрипт для проверки Animation Events.
/// Висит на персонаже вместе с Weapon.
/// </summary>
public class AnimationEventTest : MonoBehaviour
{
    public void EnableHitbox()
    {
        Debug.Log("✓ EnableHitbox вызван!");
        
        // Ищем Weapon и вызываем его метод
        Weapon weapon = GetComponent<Weapon>();
        if (weapon != null)
        {
            weapon.EnableHitbox();
        }
        else
        {
            Debug.LogError("✗ Weapon не найден на этом объекте!");
        }
    }

    public void DisableHitbox()
    {
        Debug.Log("✓ DisableHitbox вызван!");
        
        Weapon weapon = GetComponent<Weapon>();
        if (weapon != null)
        {
            weapon.DisableHitbox();
        }
    }
}
