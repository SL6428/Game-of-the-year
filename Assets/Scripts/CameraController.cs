using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform target;
    public Vector3 offset = new Vector3(0.5f, 1.5f, -2.5f); // Вид от плеча
    public float rotationSpeed = 2f;
    public float zoomSpeed = 2f;

    private float currentX = 0f;
    private float currentY = 15f;
    private Vector3 currentOffset;

    void Start()
    {
        currentOffset = offset;
    }

    void LateUpdate()
    {
        if (target == null) return;

        // Вращение камеры правой кнопкой мыши
        if (Input.GetMouseButton(1))
        {
            currentX += Input.GetAxis("Mouse X") * rotationSpeed;
            currentY -= Input.GetAxis("Mouse Y") * rotationSpeed;
            currentY = Mathf.Clamp(currentY, -10f, 40f);
        }

        // Зум колесиком мыши
        currentOffset.z += Input.GetAxis("Mouse ScrollWheel") * zoomSpeed;
        currentOffset.z = Mathf.Clamp(currentOffset.z, -5f, -1f);

        // Вычисляем позицию камеры
        Quaternion rotation = Quaternion.Euler(currentY, currentX, 0);
        Vector3 desiredPosition = target.position + rotation * currentOffset;

        transform.position = desiredPosition;
        transform.LookAt(target.position + Vector3.up * 1f);
    }
}