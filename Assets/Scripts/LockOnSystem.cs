using UnityEngine;
using Unity.Cinemachine;
using System.Collections.Generic;

/// <summary>
/// Lock-On система для поиска и захвата целей.
/// Работает в паре с CameraPivot, который управляет камерой.
/// </summary>
public class LockOnSystem : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform playerTransform;
    [SerializeField] private CameraPivot cameraPivot;
    [SerializeField] private CinemachineCamera defaultCamera;
    [SerializeField] private CinemachineCamera lockedOnCamera;

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

        // Инициализация ссылок, если не назначены в инспекторе
        if (cameraPivot == null)
            cameraPivot = FindFirstObjectByType<CameraPivot>();

        if (defaultCamera == null || lockedOnCamera == null)
        {
            // Пытаемся найти Cinemachine камеры на сцене
            CinemachineCamera[] allCameras = FindObjectsByType<CinemachineCamera>(FindObjectsSortMode.InstanceID);
            if (allCameras.Length > 0)
            {
                defaultCamera = allCameras[0];
                if (allCameras.Length > 1)
                    lockedOnCamera = allCameras[1];
            }
        }

        // Инициализация приоритетов
        if (defaultCamera != null)
            defaultCamera.Priority = DEFAULT_CAM_PRIORITY;
        if (lockedOnCamera != null)
            lockedOnCamera.Priority = DEFAULT_CAM_PRIORITY;

        // Настройка камер
        SetupCameras();
    }

    private void SetupCameras()
    {
        if (defaultCamera != null && cameraPivot != null)
        {
            defaultCamera.Follow = cameraPivot.CameraTransform;
            defaultCamera.LookAt = cameraPivot.CameraTransform;
        }
        
        if (lockedOnCamera != null && cameraPivot != null)
        {
            lockedOnCamera.Follow = cameraPivot.CameraTransform;
            lockedOnCamera.LookAt = cameraPivot.CameraTransform;
        }
    }

    void Update()
    {
        // Проверка ссылок на NULL
        if (defaultCamera == null || lockedOnCamera == null || cameraPivot == null)
        {
            Debug.LogError("Одна из ссылок не назначена в LockOnSystem!");
            return;
        }

        // Поиск целей каждые N секунд
        if (Time.time - lastSearchTime >= searchInterval)
        {
            SearchForTargets();
            lastSearchTime = Time.time;
        }

        // Переключение Lock-On
        if (Input.GetKeyDown(lockOnKey))
        {
            if (isLockedOn)
            {
                ReleaseLockOn();
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

        // Сброс если цель пропала
        if (isLockedOn && (currentTarget == null || !currentTarget.gameObject.activeInHierarchy))
        {
            ReleaseLockOn();
        }
    }

    private void SearchForTargets()
    {
        availableTargets.Clear();

        if (playerTransform == null) return;

        // Ищем все коллайдеры в радиусе
        Collider[] colliders = Physics.OverlapSphere(playerTransform.position, lockOnRadius);

        foreach (var collider in colliders)
        {
            // Проверяем тег Enemy
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

        // Сортировка по дистанции (ближайшие первыми)
        availableTargets.Sort((a, b) =>
        {
            float distA = Vector3.Distance(playerTransform.position, a.position);
            float distB = Vector3.Distance(playerTransform.position, b.position);
            return distA.CompareTo(distB);
        });

        // Если текущая цель больше не в списке, сбрасываем
        if (currentTarget != null && !availableTargets.Contains(currentTarget))
        {
            currentTarget = null;
            currentTargetIndex = 0;
        }
    }

    private void TryLockOn()
    {
        if (availableTargets.Count == 0)
        {
            Debug.Log("Нет доступных целей в радиусе!");
            return;
        }

        // Если уже есть цель, активируем
        if (currentTarget != null && availableTargets.Contains(currentTarget))
        {
            ActivateLockOn(currentTarget);
        }
        else
        {
            // Берем ближайшую цель
            currentTargetIndex = 0;
            currentTarget = availableTargets[0];
            ActivateLockOn(currentTarget);
        }
    }

    private void ActivateLockOn(Transform target)
    {
        if (target == null || defaultCamera == null || lockedOnCamera == null || cameraPivot == null)
        {
            Debug.LogError("Ошибка при активации Lock-On: некорректные ссылки!");
            return;
        }

        currentTarget = target;
        isLockedOn = true;

        // Переключение приоритетов
        defaultCamera.Priority = DEFAULT_CAM_PRIORITY;
        lockedOnCamera.Priority = LOCKED_CAM_PRIORITY;

        // Передаем цель в CameraPivot
        cameraPivot.SetLockOnTarget(target);

        Debug.Log($"Lock-On активирован на: {target.name}");
    }

    private void ReleaseLockOn()
    {
        isLockedOn = false;
        currentTarget = null;
        currentTargetIndex = 0;

        if (defaultCamera != null && lockedOnCamera != null)
        {
            // Возвращаем приоритеты
            defaultCamera.Priority = DEFAULT_CAM_PRIORITY;
            lockedOnCamera.Priority = DEFAULT_CAM_PRIORITY;
        }

        // Сбрасываем CameraPivot
        cameraPivot.SetLockOnTarget(null);

        Debug.Log("Lock-On сброшен");
    }

    private void SwitchToNextTarget()
    {
        if (availableTargets.Count <= 1)
            return;

        currentTargetIndex++;

        if (currentTargetIndex >= availableTargets.Count)
            currentTargetIndex = 0;

        currentTarget = availableTargets[currentTargetIndex];
        ActivateLockOn(currentTarget);

        Debug.Log($"Переключен на цель {currentTargetIndex + 1} из {availableTargets.Count}");
    }

    // Отладка
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

                if (isLockedOn && target == currentTarget)
                {
                    Gizmos.color = Color.red;
                    Gizmos.DrawLine(playerTransform.position, target.position);
                }
            }
        }
    }

    // Публичные методы
    public bool IsLockedOn() => isLockedOn;
    public Transform GetCurrentTarget() => currentTarget;
    public List<Transform> GetAvailableTargets() => new List<Transform>(availableTargets);
}
