using UnityEngine;

public class CameraScript : MonoBehaviour // Changed from NetworkBehaviour to MonoBehaviour to simplify
{
    private Quaternion initialRotation;
    private Vector3 initialOffset;
    private Transform parentTransform;
    private bool isInitialized = false;

    // The PlayerController will call this to manually kick off the camera logic safely
    public void InitializeCamera(Transform playerTransform)
    {
        parentTransform = playerTransform;
        initialRotation = transform.rotation;
        initialOffset = transform.position - parentTransform.position;
        isInitialized = true;
    }

    private void LateUpdate()
    {
        // Only follow if initialized (which only happens for the local owner)
        if (isInitialized && parentTransform != null)
        {
            transform.rotation = initialRotation;
            transform.position = parentTransform.position + initialOffset;
        }
    }
}