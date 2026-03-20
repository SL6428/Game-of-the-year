using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class LockOnSystem : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform playerTransform;
    [SerializeField] private CameraPivot cameraPivot;
    [SerializeField] private CinemachineVirtualCamera defaultCamera;
    [SerializeField] private CinemachineVirtualCamera lockedOnCamera;

    [Header("Lock-On Settings")]
    [SerializeField] private float lockOnRadius = 5f;
    [SerializeField] private float fieldOfViewAngle = 90f;
    [SerializeField] private KeyCode lockOnKey = KeyCode.U;
    [SerializeField] private KeyCode switchTargetKey = KeyCode.Tab;

    [Header("Debug")]
    [SerializeField] private bool showDebugGizmos = true;

    private List<Transform> availableTargets = new List<Transform>();
    private Transform currentTarget = null;
    private bool isLockedOn = false;
    private int currentTargetIndex = 0;
    private float lastSearchTime = 0f;
    private float searchInterval = 0.5f;

    // Priorities для камер
    private const int DEFAULT_CAM_PRIORITY = 10;
    private const int LOCKED_CAM_PRIORITY = 15;

    void Start()
    {
        if (playerTransform == null)
            playerTransform = transform;

        // Инициализация приоритетов
        if (defaultCamera != null)
            defaultCamera.Priority = DEFAULT_CAM_PRIORITY;
        if (lockedOnCamera != null)
            lockedOnCamera.Priority = DEFAULT_CAM_PRIORITY;

        // Настройка камер
        if (defaultCamera != null)
        {
            defaultCamera.Follow = cameraPivot.transform;
            defaultCamera.LookAt = playerTransform;
        }
        if (lockedOnCamera != null)
        {
            lockedOnCamera.Follow = cameraPivot.transform;
            lockedOnCamera.LookAt = null;
        }
    }

    void Update()
    {
        // ПОЛНАЯ ПРОВЕРКА НА NULL ПЕРЕД ВЫПОЛНЕНИЕМ
        if (defaultCamera == null || lockedOnCamera == null || cameraPivot == null)
        {
            Debug.LogError("Важные компоненты не назначены в LockOnSystem!");
            return;
        }

        // Поиск целей с интервалом для оптимизации
        if (Time.time - lastSearchTime >= searchInterval)
        {
            SearchForTargets();
            lastSearchTime = Time.time;
        }

        // Активация Lock-On
        if (Input.GetKeyDown(lockOnKey))
        {
            if (isLockedOn)
            {
                ReleaseLockOn(); // ИСПРАВЛЕНО: Правильное название метода
            }
            else
            {
                TryLockOn();
            }
        }

        // Переключение между целями
        if (isLockedOn && Input.GetKeyDown(switchTargetKey))
        {
            SwitchToNextTarget();
        }

        // Проверка потери цели
        if (isLockedOn && (currentTarget == null || !currentTarget.gameObject.activeInHierarchy))
        {
            ReleaseLockOn();
        }
    }

    private void SearchForTargets()
    {
        availableTargets.Clear();

        // Проверка на наличие игрока
        if (playerTransform == null) return;

        // Поиск всех коллайдеров в радиусе
        Collider[] colliders = Physics.OverlapSphere(playerTransform.position, lockOnRadius);

        foreach (var collider in colliders)
        {
            // Проверка тега Enemy
            if (collider.CompareTag("Enemy"))
            {
                Transform targetTransform = collider.transform;

                // Проверка угла обзора
                Vector3 directionToTarget = targetTransform.position - playerTransform.position;
                float angle = Vector3.Angle(playerTransform.forward, directionToTarget);

                if (angle <= fieldOfViewAngle)
                {
                    availableTargets.Add(targetTransform);
                }
            }
        }

        // Сортировка по расстоянию (ближайшие первыми)
        availableTargets.Sort((a, b) =>
        {
            float distA = Vector3.Distance(playerTransform.position, a.position);
            float distB = Vector3.Distance(playerTransform.position, b.position);
            return distA.CompareTo(distB);
        });

        // Если текущая цель больше не в списке, сбрасываем индекс
        if (currentTarget != null && !availableTargets.Contains(currentTarget))
        {
            currentTarget = null;
            currentTargetIndex = 0;
        }
    }

    private void TryLockOn()
    {
        // ПРОВЕРКА НА НАЛИЧИЕ ЦЕЛЕЙ
        if (availableTargets.Count == 0)
        {
            Debug.Log("Нет доступных целей в радиусе!");
            return;
        }

        // Если есть текущая цель, используем её
        if (currentTarget != null && availableTargets.Contains(currentTarget))
        {
            ActivateLockOn(currentTarget);
        }
        else
        {
            // Иначе берём первую из списка
            currentTargetIndex = 0;
            currentTarget = availableTargets[0];
            ActivateLockOn(currentTarget);
        }
    }

    private void ActivateLockOn(Transform target)
    {
        // ДОБАВЛЕНА ПРОВЕРКА НА NULL
        if (target == null || defaultCamera == null || lockedOnCamera == null || cameraPivot == null)
        {
            Debug.LogError("Ошибка при активации Lock-On: недостающие ссылки!");
            return;
        }

        currentTarget = target;
        isLockedOn = true;

        // Установка приоритетов
        defaultCamera.Priority = DEFAULT_CAM_PRIORITY;
        lockedOnCamera.Priority = LOCKED_CAM_PRIORITY;

        // Установка цели для CameraPivot
        cameraPivot.SetLockOnTarget(target);

        // Установка цели для камеры
        lockedOnCamera.LookAt = target;

        Debug.Log($"Lock-On активирован на: {target.name}");
    }

    // ИСПРАВЛЕНО: Правильное название метода
    private void ReleaseLockOn()
    {
        isLockedOn = false;
        currentTarget = null;
        currentTargetIndex = 0;

        // ПРОВЕРКА НА NULL
        if (defaultCamera != null && lockedOnCamera != null)
        {
            // Сброс приоритетов
            defaultCamera.Priority = DEFAULT_CAM_PRIORITY;
            lockedOnCamera.Priority = DEFAULT_CAM_PRIORITY;

            // Сброс целей
            lockedOnCamera.LookAt = null;
        }

        // Сброс в CameraPivot
        cameraPivot.SetLockOnTarget(null);

        Debug.Log("Lock-On деактивирован");
    }

    private void SwitchToNextTarget()
    {
        // ПРОВЕРКА НА НАЛИЧИЕ ЦЕЛЕЙ
        if (availableTargets.Count <= 1)
            return;

        currentTargetIndex++;

        if (currentTargetIndex >= availableTargets.Count)
            currentTargetIndex = 0;

        currentTarget = availableTargets[currentTargetIndex];
        ActivateLockOn(currentTarget);

        Debug.Log($"Переключение на цель {currentTargetIndex + 1} из {availableTargets.Count}");
    }

    // Визуализация в редакторе
    void OnDrawGizmos()
    {
        if (!showDebugGizmos || playerTransform == null)
            return;

        // Радиус поиска
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(playerTransform.position, lockOnRadius);

        // Доступные цели
        Gizmos.color = Color.green;
        foreach (var target in availableTargets)
        {
            if (target != null)
            {
                Gizmos.DrawSphere(target.position, 0.3f);

                // Линия к цели
                if (isLockedOn && target == currentTarget)
                {
                    Gizmos.color = Color.red;
                    Gizmos.DrawLine(playerTransform.position, target.position);
                }
            }
        }
    }

    // Публичные методы для доступа из других скриптов
    public bool IsLockedOn() => isLockedOn;
    public Transform GetCurrentTarget() => currentTarget;
    public List<Transform> GetAvailableTargets() => new List<Transform>(availableTargets);
}