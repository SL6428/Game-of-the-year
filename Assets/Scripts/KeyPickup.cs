using UnityEngine;

/// <summary>
/// Ключ на сцене. Подбирается по нажатию E вблизи.
/// </summary>
public class KeyPickup : MonoBehaviour
{
    [Header("Настройки ключа")]
    [Tooltip("Уникальное имя ключа (должно совпадать с Door.requiredKeyName)")]
    [SerializeField] private string keyName = "UpperHallKey";
    [Tooltip("Расстояние, с которого можно подобрать")]
    [SerializeField] private float pickupRadius = 2f;
    [Tooltip("Уничтожать объект после подбора")]
    [SerializeField] private bool destroyAfterPickup = true;

    [Header("UI - Подсказки и сообщения")]
    [Tooltip("Панель с подсказкой '[E] Подобрать' (показывается при приближении)")]
    [SerializeField] private GameObject promptPanel;
    [Tooltip("Панель с сообщением 'Ключ подобран' (отдельная от подсказки!)")]
    [SerializeField] private GameObject pickupMessagePanel;
    [Tooltip("Текст внутри pickupMessagePanel")]
    [SerializeField] private UnityEngine.UI.Text pickupMessageText;
    [Tooltip("Сколько секунд показывать сообщение")]
    [SerializeField] private float messageDuration = 2f;

    // Состояния
    private bool isPickedUp = false;
    private bool isPlayerInRange = false;

    // Для скрытия сообщения
    private float messageHideTimer = 0f;
    private bool isMessageShowing = false;

    // Кэш игрока
    private Transform playerTransform;

    void Start()
    {
        // Скрываем UI
        if (promptPanel != null)
            promptPanel.SetActive(false);
        
        if (pickupMessagePanel != null)
            pickupMessagePanel.SetActive(false);

        // Создаём инвентарь если его ещё нет
        if (SimpleInventory.Instance == null)
        {
            GameObject invObj = new GameObject("SimpleInventory");
            invObj.AddComponent<SimpleInventory>();
            Debug.Log("[KeyPickup] Создан SimpleInventory");
        }

        // Получаем ссылку на игрока
        UpdatePlayerReference();
    }

    void Update()
    {
        // Если ключ уже подобран - ничего не делаем
        if (isPickedUp) return;

        // Обновляем ссылку на игрока если нужно
        UpdatePlayerReference();

        // Обновляем таймер сообщения (используем unscaledDeltaTime)
        if (isMessageShowing)
        {
            messageHideTimer -= Time.unscaledDeltaTime;
            if (messageHideTimer <= 0f)
            {
                isMessageShowing = false;
                if (pickupMessagePanel != null)
                    pickupMessagePanel.SetActive(false);
            }
            return;
        }

        // Проверяем расстояние до игрока
        if (playerTransform == null) return;
        
        float distance = Vector3.Distance(transform.position, playerTransform.position);
        bool wasInRange = isPlayerInRange;
        isPlayerInRange = distance <= pickupRadius;

        // Игрок вошёл в радиус - показываем подсказку
        if (isPlayerInRange && !wasInRange)
        {
            if (promptPanel != null) 
                promptPanel.SetActive(true);
        }

        // Игрок вышел из радиуса - скрываем подсказку
        if (!isPlayerInRange && wasInRange)
        {
            if (promptPanel != null) 
                promptPanel.SetActive(false);
        }

        // Подбор по E
        if (isPlayerInRange && Input.GetKeyDown(KeyCode.E))
        {
            PickupKey();
        }
    }

    /// <summary>
    /// Обновляет ссылку на игрока
    /// </summary>
    void UpdatePlayerReference()
    {
        if (playerTransform == null)
        {
            // Сначала пробуем через GameManager
            if (GameManager.Instance != null && GameManager.Instance.Player != null)
            {
                playerTransform = GameManager.Instance.Player;
            }
            // Если нет - ищем по тегу
            else
            {
                GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
                if (playerObj != null)
                {
                    playerTransform = playerObj.transform;
                }
            }
        }
    }

    void PickupKey()
    {
        isPickedUp = true;
        isPlayerInRange = false;

        // Скрываем подсказку "[E] Подобрать"
        if (promptPanel != null)
            promptPanel.SetActive(false);

        // Добавляем в инвентарь
        if (SimpleInventory.Instance != null)
            SimpleInventory.Instance.AddKey(keyName);

        // Показываем сообщение "Ключ подобран"
        ShowPickupMessage();

        // Уничтожаем объект
        if (destroyAfterPickup)
        {
            Destroy(gameObject, 0.5f);
        }
        else
        {
            // Или просто делаем невидимым/неактивным
            foreach (Renderer rend in GetComponentsInChildren<Renderer>())
                rend.enabled = false;
            
            foreach (Collider col in GetComponents<Collider>())
                col.enabled = false;
        }

        Debug.Log($"[KeyPickup] Ключ '{keyName}' подобран");
    }

    void ShowPickupMessage()
    {
        if (pickupMessagePanel == null)
        {
            Debug.LogWarning($"[KeyPickup] Не назначен pickupMessagePanel для ключа '{keyName}'");
            return;
        }

        // Устанавливаем текст
        if (pickupMessageText != null)
        {
            pickupMessageText.text = $"🔑 Подобран ключ: {keyName}";
        }

        // Показываем панель
        pickupMessagePanel.SetActive(true);
        isMessageShowing = true;
        messageHideTimer = messageDuration;

        Debug.Log($"[KeyPickup] Показано сообщение 'Ключ подобран'");
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, pickupRadius);
    }
}
