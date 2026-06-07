using UnityEngine;
using FishNet.Object;

public class CameraScript : NetworkBehaviour
{
    private Quaternion initialRotation;
    private Vector3 initialOffset;
    private Transform parentTransform;

    public override void OnStartClient()
    {
        base.OnStartClient();

        // If this is our local player's camera, cache the offsets for movement calculation
        if (IsOwner)
        {
            initialRotation = transform.rotation;
            parentTransform = transform.parent;

            if (parentTransform != null)
            {
                initialOffset = transform.position - parentTransform.position;
            }
        }
    }

    private void LateUpdate()
    {
        // Only run the camera follow logic for the local player who owns this camera
        if (IsOwner)
        {
            transform.rotation = initialRotation;
            if (parentTransform != null)
            {
                transform.position = parentTransform.position + initialOffset;
            }
        }
    }
}