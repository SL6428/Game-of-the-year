using UnityEngine;

/// <summary>
/// Базовый класс для всех интерактивных объектов.
/// Отслеживает радиус игрока и нажатия кнопок.
/// </summary>
public abstract class InteractableObject : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] protected float interactionRadius = 2f;
    [SerializeField] protected Transform player;

    protected bool isPlayerInRange = false;
    protected bool isShowingPopup = false;

    protected virtual void Start()
    {
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
                player = playerObj.transform;
        }
    }

    protected virtual void Update()
    {
        if (player == null) return;

        // Проверяем дистанцию до игрока (ВСЕГДА)
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        bool wasInRange = isPlayerInRange;
        isPlayerInRange = distanceToPlayer <= interactionRadius;

        // Игрок вошёл в радиус
        if (isPlayerInRange && !wasInRange)
        {
            OnPlayerEnterRange();
        }
        
        // Игрок вышел из радиуса - закрываем popup если открыт
        if (!isPlayerInRange && wasInRange)
        {
            if (isShowingPopup)
            {
                CloseCurrentPopup();
            }
            OnPlayerExitRange();
        }

        // Нажатие B - закрываем popup (работает всегда)
        if (isShowingPopup && Input.GetKeyDown(KeyCode.B))
        {
            CloseCurrentPopup();
        }

        // Нажатие Y - открываем popup (только если в радиусе и popup закрыт)
        if (isPlayerInRange && !isShowingPopup && Input.GetKeyDown(KeyCode.Y))
        {
            OpenPopup();
        }
    }

    /// <summary>
    /// Открыть popup подсказки
    /// </summary>
    protected virtual void OpenPopup()
    {
        isShowingPopup = true;
        ShowPopupUI();
    }

    /// <summary>
    /// Закрыть текущий popup
    /// </summary>
    protected virtual void CloseCurrentPopup()
    {
        isShowingPopup = false;
        HidePopupUI();
    }

    /// <summary>
    /// Переопредели чтобы показать свой UI
    /// </summary>
    protected abstract void ShowPopupUI();

    /// <summary>
    /// Переопредели чтобы скрыть свой UI
    /// </summary>
    protected abstract void HidePopupUI();

    /// <summary>
    /// Игрок вошёл в радиус
    /// </summary>
    protected virtual void OnPlayerEnterRange() { }

    /// <summary>
    /// Игрок вышел из радиуса
    /// </summary>
    protected virtual void OnPlayerExitRange() { }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, interactionRadius);
    }
}
