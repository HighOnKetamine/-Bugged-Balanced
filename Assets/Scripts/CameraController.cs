using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Target")]
    public Transform target;

    [Header("Camera Settings")]
    public float height = 10f;         // How high the camera is above the player
    public float distance = 8f;        // How far behind the player
    public float tiltAngle = 45f;      // Downward tilt
    public float zoomSpeed = 5f;       // How fast scroll wheel zooms
    public float minZoom = 4f;
    public float maxZoom = 16f;

    void LateUpdate()
    {
        if (target == null)
            return;

        // Handle zoom
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scroll) > 0.01f)
        {
            distance -= scroll * zoomSpeed;
            distance = Mathf.Clamp(distance, minZoom, maxZoom);
        }

        // Calculate new position — fixed angle, instant update
        Vector3 offset = Quaternion.Euler(tiltAngle, 0, 0) * new Vector3(0, 0, -distance);
        Vector3 targetPosition = target.position + Vector3.up * height + offset;

        transform.position = targetPosition;
        transform.LookAt(target.position + Vector3.up * 1.5f);
    }
}
