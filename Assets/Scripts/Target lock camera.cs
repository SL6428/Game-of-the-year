using UnityEngine;

public class LockOnSystem : MonoBehaviour
{
    [Header("Настройки захвата")]
    public float lockRange = 5f;
    public float lockAngle = 60f;
    public LayerMask enemyLayer;

    [Header("Отладка")]
    public KeyCode lockKey = KeyCode.U;

    public Transform currentTarget { get; private set; }
    public bool isLocked { get; private set; }

    private Transform _playerTransform;

    void Start()
    {
        _playerTransform = transform;
        if (enemyLayer.value == 0)
            Debug.LogError("LockOnSystem: Не назначен слой врагов в Инспекторе!");
    }

    void Update()
    {
        // 1. Проверяем, видит ли Unity нажатие вообще
        // Если этот лог НЕ появляется при нажатии U - проблема точно в Input или Фокусе
        if (Input.GetKeyDown(lockKey))
        {
            Debug.Log(">>> UPDATE: Физическое нажатие U получено!");
            ToggleLock();
        }

        // 2. Проверяем состояние системы каждый кадр
        // Если этот лог появляется, а ToggleLock нет - значит кто-то меняет isLocked извне!
        if (isLocked && currentTarget != null)
        {
            // Пишем в консоль только раз в 60 кадров (чтобы не заспамить насмерть), 
            // либо если цель сменилась
            if (Time.frameCount % 60 == 0)
            {
                Debug.Log($">>> UPDATE: Система в захвате. Цель: {currentTarget.name}");
            }

            bool shouldUnlock = false;
            if (currentTarget == null) shouldUnlock = true;
            else if (Vector3.Distance(_playerTransform.position, currentTarget.position) > lockRange * 1.5f) shouldUnlock = true;

            if (shouldUnlock) Unlock();
        }
    }

    void ToggleLock()
    {
        // <-- ДОБАВЛЕНО: Лог, чтобы ты точно знал, что кнопка нажалась
        Debug.Log($"LockOnSystem: Кнопка нажата! Текущий статус: {(isLocked ? "ЗАХВАЧЕН" : "СВОБОДЕН")}");

        if (isLocked)
        {
            Unlock();
        }
        else
        {
            TryLock();
        }
    }

    void TryLock()
    {
        Collider[] hits = Physics.OverlapSphere(_playerTransform.position, lockRange, enemyLayer);

        Transform bestTarget = null;
        float closestDist = Mathf.Infinity;

        foreach (Collider hit in hits)
        {
            if (hit.transform == _playerTransform) continue;

            Vector3 dirToEnemy = (hit.transform.position - _playerTransform.position).normalized;
            float dist = Vector3.Distance(_playerTransform.position, hit.transform.position);

            float dot = Vector3.Dot(_playerTransform.forward, dirToEnemy);
            float angle = Mathf.Acos(dot) * Mathf.Rad2Deg;

            if (angle < lockAngle && dist < closestDist)
            {
                closestDist = dist;
                bestTarget = hit.transform;
            }
        }

        if (bestTarget != null)
        {
            currentTarget = bestTarget;
            isLocked = true; // <-- Явно ставим флаг
            Debug.Log($"LOCKED ON: {currentTarget.name}");
        }
        else
        {
            Debug.Log("LockOnSystem: Врагов в радиусе нет.");
        }
    }

    void Unlock()
    {
        // <-- ИЗМЕНЕНО: Убрал проверку if (isLocked). 
        // Теперь этот метод всегда сбрасывает состояние в ноль,不管 что было до этого.
        Debug.Log("UNLOCKED (Сброс состояния)");

        currentTarget = null;
        isLocked = false;
    }

    void OnDrawGizmos()
    {
        if (_playerTransform == null) _playerTransform = transform;

        Gizmos.color = new Color(1, 0, 0, 0.2f);
        Gizmos.DrawSphere(_playerTransform.position, lockRange);

        if (isLocked && currentTarget != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(_playerTransform.position, currentTarget.position);
            Gizmos.DrawSphere(currentTarget.position, 0.5f);
        }
    }
}