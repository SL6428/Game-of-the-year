using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform target;
    public Vector3 offset = new Vector3(0.5f, 1.5f, -2.5f); // ��� �� �����
    public float rotationSpeed = 2f;
    public float zoomSpeed = 2f;
    [HideInInspector] public float sensitivityMultiplier = 1f;

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

        // �������� ������ ������ ������� ����
        if (Input.GetMouseButton(1))
        {
            currentX += Input.GetAxis("Mouse X") * rotationSpeed * sensitivityMultiplier;
            currentY -= Input.GetAxis("Mouse Y") * rotationSpeed * sensitivityMultiplier;
            currentY = Mathf.Clamp(currentY, -10f, 40f);
        }

        // ��� ��������� ����
        currentOffset.z += Input.GetAxis("Mouse ScrollWheel") * zoomSpeed;
        currentOffset.z = Mathf.Clamp(currentOffset.z, -5f, -1f);

        // ��������� ������� ������
        Quaternion rotation = Quaternion.Euler(currentY, currentX, 0);
        Vector3 desiredPosition = target.position + rotation * currentOffset;

        transform.position = desiredPosition;
        transform.LookAt(target.position + Vector3.up * 1f);
    }
}