using UnityEngine;

/// <summary>
/// CameraPivot только для получения направления камеры.
/// Вращение камеры обрабатывается через Cinemachine FreeLook.
/// </summary>
public class CameraPivot : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform player;

    private bool isLockedOn = false;
    private Transform lockOnTarget;

    // Публичное свойство для получения текущей позиции камеры
    public Transform CameraTransform => transform;

    void Start()
    {
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
                player = playerObj.transform;
        }
    }

    void LateUpdate()
    {
        if (player == null)
            return;

        // Всегда следуем за игроком (позиция)
        transform.position = player.position;

        // Поворачиваем pivot по направлению камеры (только горизонталь)
        if (Camera.main != null)
        {
            Vector3 cameraForward = Camera.main.transform.forward;
            cameraForward.y = 0; // Убираем вертикальную составляющую
            
            if (cameraForward.magnitude > 0.01f)
            {
                cameraForward.Normalize();
                transform.rotation = Quaternion.LookRotation(cameraForward);
            }
        }
    }

    public void SetLockOnTarget(Transform target)
    {
        lockOnTarget = target;
        isLockedOn = (target != null);

        if (isLockedOn && player != null && target != null)
        {
            Vector3 directionToTarget = (target.position - player.position).normalized;
            directionToTarget.y = 0;

            if (directionToTarget.magnitude > 0.01f)
            {
                transform.rotation = Quaternion.LookRotation(directionToTarget);
            }
        }
    }

    public void ResetRotation()
    {
        if (player != null)
        {
            Vector3 forward = (player.position + Vector3.forward - transform.position).normalized;
            transform.rotation = Quaternion.LookRotation(forward);
        }
    }

    public bool IsLockedOn() => isLockedOn;

    /// <summary>
    /// Получить направление вперёд относительно камеры (только горизонтальное!)
    /// </summary>
    public Vector3 GetForwardDirection()
    {
        return transform.forward;
    }

    /// <summary>
    /// Получить направление вправо относительно камеры (только горизонтальное!)
    /// </summary>
    public Vector3 GetRightDirection()
    {
        return transform.right;
    }
}