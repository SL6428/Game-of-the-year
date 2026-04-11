using UnityEngine;

/// <summary>
/// Простой инвентарь для хранения ключей.
/// Singleton - один на сцену.
/// </summary>
public class SimpleInventory : MonoBehaviour
{
    public static SimpleInventory Instance { get; private set; }

    // Ключи которые подобраны (название ключа)
    private System.Collections.Generic.HashSet<string> collectedKeys = new System.Collections.Generic.HashSet<string>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Подобрать ключ
    /// </summary>
    public void AddKey(string keyName)
    {
        if (collectedKeys.Add(keyName))
        {
            Debug.Log($"[Inventory] 🔑 Подобран ключ: {keyName}");
        }
    }

    /// <summary>
    /// Проверить есть ли ключ
    /// </summary>
    public bool HasKey(string keyName)
    {
        return collectedKeys.Contains(keyName);
    }

    /// <summary>
    /// Удалить ключ (если одноразовый)
    /// </summary>
    public void RemoveKey(string keyName)
    {
        collectedKeys.Remove(keyName);
    }
}
