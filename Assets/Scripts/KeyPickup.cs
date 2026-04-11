using UnityEngine;

/// <summary>
/// Ключ на сцене. Подбирается по нажатию E вблизи.
/// </summary>
public class KeyPickup : MonoBehaviour
{
    [Header("Key Settings")]
    [SerializeField] private string keyName = "UpperHallKey"; // Название ключа
    [SerializeField] private float pickupRadius = 2f;
    [SerializeField] private Transform player;
    [SerializeField] private bool destroyAfterPickup = true;

    [Header("UI (опционально)")]
    [Tooltip("Твоя панель подсказки '[E] Подобрать'")]
    [SerializeField] private GameObject promptPanel;
    [Tooltip("Менеджер сообщений (перетащи объект с UIMessageManager)")]
    [SerializeField] private UIMessageManager messageManager;

    private bool isPickedUp = false;
    private bool isPlayerInRange = false;

    void Start()
    {
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
                player = playerObj.transform;
        }

        // Создаём инвентарь если нет
        if (SimpleInventory.Instance == null)
        {
            GameObject invObj = new GameObject("SimpleInventory");
            invObj.AddComponent<SimpleInventory>();
        }

        // Создаём менеджер сообщений если нет и не назначен
        if (messageManager == null && UIMessageManager.Instance == null)
        {
            GameObject msgObj = new GameObject("UIMessageManager");
            messageManager = msgObj.AddComponent<UIMessageManager>();
        }
        else if (messageManager == null)
        {
            messageManager = UIMessageManager.Instance;
        }

        // Скрываем подсказку по умолчанию
        if (promptPanel != null)
        {
            promptPanel.SetActive(false);
        }
    }

    void Update()
    {
        if (player == null || isPickedUp) return;

        float distance = Vector3.Distance(transform.position, player.position);
        bool wasInRange = isPlayerInRange;
        isPlayerInRange = distance <= pickupRadius;

        // Игрок вошёл в радиус - показываем подсказку
        if (isPlayerInRange && !wasInRange)
        {
            if (promptPanel != null) promptPanel.SetActive(true);
        }

        // Игрок вышел из радиуса - скрываем подсказку
        if (!isPlayerInRange && wasInRange)
        {
            if (promptPanel != null) promptPanel.SetActive(false);
        }

        // Подбор по E
        if (isPlayerInRange && Input.GetKeyDown(KeyCode.E))
        {
            PickupKey();
        }
    }

    void PickupKey()
    {
        isPickedUp = true;
        isPlayerInRange = false;

        // Скрываем подсказку "[E] Подобрать"
        if (promptPanel != null)
        {
            promptPanel.SetActive(false);
        }

        // Добавляем в инвентарь
        if (SimpleInventory.Instance != null)
        {
            SimpleInventory.Instance.AddKey(keyName);
        }

        // Показываем сообщение через менеджер
        if (messageManager != null)
        {
            messageManager.ShowMessage($"🔑 Подобран ключ: {keyName}");
        }

        // Уничтожаем объект
        if (destroyAfterPickup)
        {
            Destroy(gameObject, 0.5f);
        }
        else
        {
            // Или просто делаем невидимым
            foreach (Renderer rend in GetComponentsInChildren<Renderer>())
            {
                rend.enabled = false;
            }
            foreach (Collider col in GetComponents<Collider>())
            {
                col.enabled = false;
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, pickupRadius);
    }
}
