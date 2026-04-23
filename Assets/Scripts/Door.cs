using UnityEngine;

/// <summary>
/// Дверь с плавной анимацией открытия.
/// Работает под ЛЮБЫМ углом!
/// </summary>
public class Door : MonoBehaviour
{
    [Header("Door Settings")]
    [SerializeField] private float interactionRadius = 2f;
    [SerializeField] private Transform player;
    [SerializeField] private bool requiresKey = false;
    [SerializeField] private string requiredKeyName = "UpperHallKey";
    [SerializeField] private string keyMissingMessage = "Требуется ключ от верхнего зала";

    [Header("Animation")]
    [Tooltip("На какую ось вращать дверь (обычно Y)")]
    [SerializeField] private Vector3 rotationAxis = new Vector3(0, 1, 0);
    [Tooltip("На сколько градусов открывать дверь")]
    [SerializeField] private float openAngle = 90f;
    [Tooltip("Скорость открытия (секунды)")]
    [SerializeField] private float openDuration = 1.5f;

    [Header("UI (опционально)")]
    [Tooltip("Твоя панель подсказки '[E] Открыть'")]
    [SerializeField] private GameObject promptPanel;
    [Tooltip("Менеджер сообщений для 'Требуется ключ'")]
    [SerializeField] private UIMessageManager messageManager;

    [Header("Sound (опционально)")]
    [SerializeField] private AudioSource openSound;

    // Состояния
    private bool isOpen = false;
    private bool isAnimating = false;
    private bool isPlayerInRange = false;

    // Для анимации
    private Quaternion closedRotation;
    private Quaternion openRotation;
    private float animationTimer = 0f;

    void Start()
    {
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
                player = playerObj.transform;
        }

        // Сохраняем начальную позицию (закрытая дверь)
        closedRotation = transform.rotation;

        // Вычисляем позицию открытия (поворот на openAngle вокруг оси)
        Vector3 axis = rotationAxis.normalized;
        float angle = openAngle * Mathf.Deg2Rad;
        Quaternion deltaRotation = new Quaternion(
            axis.x * Mathf.Sin(angle / 2),
            axis.y * Mathf.Sin(angle / 2),
            axis.z * Mathf.Sin(angle / 2),
            Mathf.Cos(angle / 2)
        );
        openRotation = deltaRotation * closedRotation;

        // Скрываем UI по умолчанию
        if (promptPanel != null) promptPanel.SetActive(false);

        // Создаём инвентарь и менеджер сообщений если нет
        if (SimpleInventory.Instance == null && requiresKey)
        {
            GameObject invObj = new GameObject("SimpleInventory");
            invObj.AddComponent<SimpleInventory>();
        }

        if (messageManager == null && UIMessageManager.Instance == null && requiresKey)
        {
            GameObject msgObj = new GameObject("UIMessageManager");
            messageManager = msgObj.AddComponent<UIMessageManager>();
        }
        else if (messageManager == null)
        {
            messageManager = UIMessageManager.Instance;
        }
    }

    void Update()
    {
        // Анимация открытия
        if (isAnimating)
        {
            UpdateAnimation();
            return;
        }

        if (player == null || isOpen) return;

        float distance = Vector3.Distance(transform.position, player.position);
        bool wasInRange = isPlayerInRange;
        isPlayerInRange = distance <= interactionRadius;

        // Игрок вошёл в радиус
        if (isPlayerInRange && !wasInRange)
        {
            if (promptPanel != null) promptPanel.SetActive(true);
        }

        // Игрок вышел из радиуса
        if (!isPlayerInRange && wasInRange)
        {
            if (promptPanel != null) promptPanel.SetActive(false);
        }

        // Нажатие E - открываем дверь
        if (isPlayerInRange && Input.GetKeyDown(KeyCode.E))
        {
            TryOpenDoor();
        }
    }

    void TryOpenDoor()
    {
        // Проверяем ключ
        if (requiresKey)
        {
            if (SimpleInventory.Instance == null || !SimpleInventory.Instance.HasKey(requiredKeyName))
            {
                // Нет ключа - показываем сообщение
                ShowMessage(keyMissingMessage);
                return;
            }
        }

        // Открываем дверь
        OpenDoor();
    }

    void OpenDoor()
    {
        isOpen = true;
        isAnimating = true;
        animationTimer = 0f;

        // Скрываем подсказку
        if (promptPanel != null) promptPanel.SetActive(false);

        // Звук
        if (openSound != null)
        {
            openSound.Play();
        }

        Debug.Log($"[Door] 🚪 Дверь открывается...");
    }

    void UpdateAnimation()
    {
        animationTimer += Time.deltaTime;
        float t = Mathf.Clamp01(animationTimer / openDuration);

        // Плавное открытие (ease-out)
        t = 1f - Mathf.Pow(1f - t, 3f);

        // Интерполируем поворот
        transform.rotation = Quaternion.Slerp(closedRotation, openRotation, t);

        // Анимация завершена
        if (t >= 1f)
        {
            isAnimating = false;
            transform.rotation = openRotation;
            Debug.Log($"[Door] ✅ Дверь открыта!");
        }
    }

    void ShowMessage(string text)
    {
        if (messageManager != null)
        {
            messageManager.ShowMessage(text);
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, interactionRadius);

        // Показываем направление открытия
        if (!Application.isPlaying)
        {
            Vector3 axis = rotationAxis.normalized;
            Vector3 direction = new Vector3(axis.x, axis.y, axis.z);
            Gizmos.color = Color.red;
            Gizmos.DrawRay(transform.position, direction * 2f);
        }
    }
}
