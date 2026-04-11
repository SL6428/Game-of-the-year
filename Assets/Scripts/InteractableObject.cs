using UnityEngine;

/// <summary>
/// Базовый класс для всех интерактивных объектов (подсказки, знаки, предметы).
/// Наследуйте от этого класса для создания интерактивных объектов.
/// </summary>
public abstract class InteractableObject : MonoBehaviour
{
    [Header("Interaction Settings")]
    [SerializeField] protected string interactionPrompt = "[Y] Прочитать";
    [SerializeField] protected float interactionRadius = 2f;
    [SerializeField] protected Transform player;

    protected bool isPlayerInRange = false;
    protected bool isShowingPopup = false;

    protected virtual void Start()
    {
        // Ищем игрока
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

        // Проверяем закрытие popup по B (работает всегда)
        if (isShowingPopup && Input.GetKeyDown(KeyCode.B))
        {
            OnClosePopup();
            return;
        }

        // Проверяем дистанцию до игрока
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        
        // Обновляем состояние только если не показываем popup
        if (!isShowingPopup)
        {
            bool wasInRange = isPlayerInRange;
            isPlayerInRange = distanceToPlayer <= interactionRadius;

            // Игрок вошёл в радиус
            if (isPlayerInRange && !wasInRange)
            {
                OnPlayerEnterRange();
            }
            // Игрок вышел из радиуса
            if (!isPlayerInRange && wasInRange)
            {
                OnPlayerExitRange();
            }

            // Проверяем нажатие кнопки взаимодействия (ТОЛЬКО в радиусе!)
            if (isPlayerInRange && Input.GetKeyDown(KeyCode.Y))
            {
                Interact();
            }
        }
    }

    /// <summary>
    /// Вызывается при нажатии B для закрытия popup
    /// </summary>
    protected virtual void OnClosePopup()
    {
        // Переопределяется в Signpost
    }

    /// <summary>
    /// Вызывается когда игрок входит в радиус взаимодействия
    /// </summary>
    protected virtual void OnPlayerEnterRange()
    {
        if (InteractionManager.Instance != null)
        {
            InteractionManager.Instance.ShowInteractionPrompt(interactionPrompt);
        }
    }

    /// <summary>
    /// Вызывается когда игрок выходит из радиуса взаимодействия
    /// </summary>
    protected virtual void OnPlayerExitRange()
    {
        if (InteractionManager.Instance != null)
        {
            InteractionManager.Instance.HideInteractionPrompt();
        }
    }

    /// <summary>
    /// Вызывается при нажатии кнопки взаимодействия
    /// </summary>
    public abstract void Interact();

    /// <summary>
    /// Отрисовка радиуса взаимодействия в редакторе
    /// </summary>
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, interactionRadius);
    }
}
