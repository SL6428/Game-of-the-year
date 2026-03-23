using UnityEngine;

/// <summary>
/// Простой CameraPivot для управления камерой от третьего лица.
/// Камера вращается вокруг игрока при зажатой ПКМ.
/// </summary>
public class CameraPivot : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform player;
    
    [Header("Camera Settings")]
    [SerializeField] private float mouseSensitivity = 2f;
    [SerializeField] private float minVerticalAngle = -30f;
    [SerializeField] private float maxVerticalAngle = 60f;
    [SerializeField] private float defaultDistance = 3f;

    private float horizontalRotation = 0f;
    private float verticalRotation = 15f;
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

        // Сохраняем начальный угол
        Vector3 euler = transform.eulerAngles;
        horizontalRotation = euler.y;
        verticalRotation = Mathf.Clamp(euler.x > 180 ? euler.x - 360 : euler.x, minVerticalAngle, maxVerticalAngle);
    }

    void LateUpdate()
    {
        if (player == null)
            return;

        // Всегда следуем за игроком
        transform.position = player.position;

        // Обработка ввода мыши (только когда не в Lock-On режиме и зажата ПКМ)
        if (!isLockedOn && Input.GetMouseButton(1))
        {
            float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
            float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

            horizontalRotation += mouseX;
            verticalRotation -= mouseY;
            verticalRotation = Mathf.Clamp(verticalRotation, minVerticalAngle, maxVerticalAngle);
        }

        // Применяем вращение
        transform.rotation = Quaternion.Euler(verticalRotation, horizontalRotation, 0f);
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
                horizontalRotation = Mathf.Atan2(directionToTarget.x, directionToTarget.z) * Mathf.Rad2Deg;
                verticalRotation = 15f;
            }
        }
    }

    public void ResetRotation()
    {
        horizontalRotation = 0f;
        verticalRotation = 15f;
    }

    public bool IsLockedOn() => isLockedOn;
    
    /// <summary>
    /// Получить направление вперёд относительно камеры (только горизонтальное!)
    /// </summary>
    public Vector3 GetForwardDirection()
    {
        Quaternion rotation = Quaternion.Euler(0, horizontalRotation, 0);
        return rotation * Vector3.forward;
    }

    /// <summary>
    /// Получить направление вправо относительно камеры (только горизонтальное!)
    /// </summary>
    public Vector3 GetRightDirection()
    {
        Quaternion rotation = Quaternion.Euler(0, horizontalRotation, 0);
        return rotation * Vector3.right;
    }
}
