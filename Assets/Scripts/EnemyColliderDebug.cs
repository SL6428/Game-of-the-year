using UnityEngine;

/// <summary>
/// Отладка коллайдеров врага.
/// Повесь этот скрипт на врага чтобы проверить коллайдеры.
/// </summary>
public class EnemyColliderDebug : MonoBehaviour
{
    void Start()
    {
        Debug.Log($"=== [DEBUG] Враг: {gameObject.name} ===");
        
        // Проверяем теги
        Debug.Log($"[DEBUG] Тег врага: {gameObject.tag}");
        Debug.Log($"[DEBUG] Сравнение с 'Enemy': {gameObject.CompareTag("Enemy")}");
        
        // Проверяем коллайдеры
        Collider[] cols = GetComponents<Collider>();
        Debug.Log($"[DEBUG] Коллайдеров на враге: {cols.Length}");
        
        foreach (var col in cols)
        {
            Debug.Log($"  - {col.GetType().Name}: isTrigger={col.isTrigger}, enabled={col.enabled}");
        }
        
        // Проверяем дочерние коллайдеры
        Collider[] childCols = GetComponentsInChildren<Collider>();
        Debug.Log($"[DEBUG] Коллайдеров на дочерних: {childCols.Length}");
        
        foreach (var col in childCols)
        {
            Debug.Log($"  - {col.gameObject.name}: {col.GetType().Name}, isTrigger={col.isTrigger}, enabled={col.enabled}");
        }
        
        // Проверяем Health
        Health health = GetComponent<Health>();
        if (health != null)
        {
            Debug.Log($"[DEBUG] Health найден: HP={health.CurrentHealth}/{health.MaxHealth}");
        }
        else
        {
            Debug.LogError("[DEBUG] Health НЕ НАЙДЕН на враге!");
        }
    }
    
    void OnDrawGizmos()
    {
        // Рисуем границы коллайдеров для визуальной проверки
        Collider[] cols = GetComponentsInChildren<Collider>();
        foreach (var col in cols)
        {
            if (col is BoxCollider box)
            {
                Gizmos.color = Color.red;
                Gizmos.matrix = Matrix4x4.TRS(col.transform.position, col.transform.rotation, Vector3.one);
                Gizmos.DrawWireCube(box.center, box.size);
            }
            else if (col is SphereCollider sphere)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawWireSphere(transform.TransformPoint(sphere.center), sphere.radius);
            }
        }
    }
}
