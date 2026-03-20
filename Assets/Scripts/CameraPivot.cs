using UnityEngine;

public class CameraPivot : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private float mouseSensitivity = 2f;
    [SerializeField] private float minVerticalAngle = -30f;
    [SerializeField] private float maxVerticalAngle = 60f;
    [SerializeField] private float defaultDistance = 3f;

    private float horizontalRotation = 0f;
    private float verticalRotation = 15f;
    private bool isLockedOn = false;
    private Transform lockOnTarget;
    private Vector3 defaultPosition;
    private Quaternion defaultRotation;

    void Start()
    {
        if (player == null)
            player = GameObject.FindWithTag("Player").transform;

        // Сохраняем исходное положение
        defaultPosition = transform.position;
        defaultRotation = transform.rotation;
        ResetRotation();
    }

    void LateUpdate()
    {
        // Следуем за игроком ТОЛЬКО по позиции, НЕ поворачиваясь с ним
        transform.position = player.position;

        // Если в Lock-On режиме, позиция камеры определяется иначе
        if (isLockedOn && lockOnTarget != null)
        {
            // В Lock-On режиме камера находится между игроком и целью
            Vector3 directionToTarget = (lockOnTarget.position - player.position).normalized;
            directionToTarget.y = 0;

            // Позиция камеры - за игроком, но смещена в сторону цели
            Vector3 offset = -directionToTarget * defaultDistance;
            transform.position = player.position + offset;
        }
        else
        {
            // Возвращаемся в исходное положение
            transform.position = player.position + defaultPosition - player.position;
            transform.rotation = defaultRotation;
        }

        // Обработка ввода мыши
        if (Input.GetMouseButton(1) || isLockedOn)
        {
            float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
            float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

            horizontalRotation += mouseX;
            verticalRotation -= mouseY;
            verticalRotation = Mathf.Clamp(verticalRotation, minVerticalAngle, maxVerticalAngle);
        }

        // ИСПРАВЛЕНО: Добавлен третий параметр
        transform.rotation = Quaternion.Euler(verticalRotation, horizontalRotation, 0f);
    }

    public void SetLockOnTarget(Transform target)
    {
        lockOnTarget = target;
        isLockedOn = (target != null);

        if (isLockedOn)
        {
            // Вычисляем начальный угол к цели
            Vector3 directionToTarget = (target.position - player.position).normalized;
            directionToTarget.y = 0;

            horizontalRotation = Mathf.Atan2(directionToTarget.x, directionToTarget.z) * Mathf.Rad2Deg;
            verticalRotation = 15f;
        }
    }

    public void ResetRotation()
    {
        horizontalRotation = 0f;
        verticalRotation = 15f;
    }

    public bool IsLockedOn() => isLockedOn;
}